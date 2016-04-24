
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.SceneManagement;

public class CreateScript : MonoBehaviour {
	
	public Camera mainCamera;
	public Canvas sceneCanvas;
	public RectTransform sceneSelector;
	public GameObject uiPanel;
	public GameObject scenes;

	public GameObject stickerSelector;
	public Transform startPos;
	public Text placeHolder;
	public Text storyText;
	public Button saveNameButton;
	public Button confirmNameButton;
	public Button saveStoryButton;
	public GameObject nameStoryPanel;
	public GameObject confirmNamePanel;

	public GameObject gamePanel;
	public GameObject loadingPanel;
	public GameObject sideBar;
	public GameObject stickerSidebar;
	public Button btnMenu;
	public Text storyNameText;
	public InputField setNameInput;
	public InputField confirmNameInput;


	public GameObject sceneScroller;
	public GameObject themeScroller;
	public GameObject stickerPool;
	public InputField storyTextInput;

	private Text loadingText;
	private Animator anim;
	private Animator animStickers;

	private Dictionary<string, Sprite> stickerSprites = new Dictionary<string, Sprite>();
	private HashSet<string> themeStrings = new HashSet<string>();
	private Dictionary<string, GameObject> themes = new Dictionary<string, GameObject>();
	private HashSet<string> newStickerThemeNames = new HashSet<string>();
	private const int MAX_SCENES = 5;

	private List<Sticker> downloadedStickers = new List<Sticker>();
	private GameObject activeTheme;
	private GameObject stickerPanelTemplate;
	//private Texture2D[] savedScenes = new Texture2D[MAX_SCENES];
	private List<Texture2D> savedScenes = new List<Texture2D>();


	private float loadingTimer = 3f;
	private float loadingTimeout = 20f;
	private bool storyFinished = false;
	private bool initialized = false;
	private bool populatedLists = false;
	private bool sideBarActive = false;
	private bool stickerBarActive = false;
	private string storyName = "";

	void Start() {
		// Display loading screen
		loadingPanel.GetComponent<CanvasGroup>().alpha = 1;
		loadingPanel.GetComponent<CanvasGroup>().blocksRaycasts = true;
		
		// Initialize variables
		stickerPanelTemplate = Instantiate(stickerSelector);
		togglePanels(sceneSelector.gameObject, true);
		loadingText = loadingPanel.transform.GetChild(0).GetComponent<Text>();
		populateMenus("BackgroundThumbnails", "Scene", sceneScroller);
		addListeners(sceneScroller);
		populateSidebar();
		addListeners(themeScroller);
		anim = sideBar.GetComponent<Animator>();
		anim.enabled = false;

		animStickers = stickerSidebar.GetComponent<Animator>();
		animStickers.enabled = false;
	}

	void Update() {
		if (scenes.transform.childCount == 0) {
			saveStoryButton.interactable = false;
		} else {
			saveStoryButton.interactable = true;
		}

		if (storyText.text.Length == 0) placeHolder.text = "Write your story here!";
		else placeHolder.text = "";

		if (storyNameText.text.Length != 0) {
			saveNameButton.interactable = true;
		} else saveNameButton.interactable = false;
		if (confirmNameInput.text.Length != 0) {
			confirmNameButton.interactable = true;
		} else {
			confirmNameButton.interactable = false;
		}

		if (!mainCamera.GetComponent<AWSManagerScript>().isFinishedDownload()) {
			loadingTimer-=Time.deltaTime;
			loadingTimeout -= Time.deltaTime;

			if (loadingTimeout <= 0) mainCamera.GetComponent<AWSManagerScript>().setFinishedDownload(true);
			if(loadingTimer <= 0) loadingTimer = 3f;
			else if (loadingTimer <= 3f && loadingTimer >= 2f) loadingText.text = "Checking for new Content.";
			else if (loadingTimer <= 2f && loadingTimer >= 1f) loadingText.text = "Checking for new Content..";
			else if (loadingTimer <= 1f && loadingTimer >= 0f) loadingText.text = "Checking for new Content...";

		} else if (!initialized) {

			loadingPanel.GetComponent<CanvasGroup>().alpha = 0;
			loadingPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
			downloadedStickers = mainCamera.GetComponent<AWSManagerScript>().getDownloadedStickers();

			// Add new Themes from downloaded content
			addDownloadedStickers();
		}
	}
		
	void addListeners(GameObject section) {
		foreach (Transform child in section.transform) {
			// Save name of each child to a string
			string captured = child.name;
			switch (section.name) {
				case "SceneSelectionPanel":
					// Add listener to Scene selections
					child.GetComponent<Button>().onClick.AddListener(() => changeImage(captured));
					break;
				case "ThemeSelectionPanel":
					// Add listener to theme selections
					child.GetComponent<Button>().onClick.AddListener(() => selectTheme(captured));
					break;
				case "StickerSelectionPanel":
					// Add listeners to stickers
					child.GetComponent<Button>().onClick.AddListener(() => addSticker(captured));
					break;
			}
		}
	}

	void changeImage(string name) {
		name = Regex.Replace(name, "[0-9]", "");
		string sceneName = "Backgrounds/"+name;
		Texture2D background = Instantiate(Resources.Load(sceneName)) as Texture2D;
		Rect r = new Rect(0, 0, background.width, background.height);

		sceneCanvas.GetComponent<Image>().sprite = Sprite.Create(background, r, new Vector2());
		togglePanels(sceneSelector.gameObject, false);

		// Start with Sidebar displayed
		anim.enabled = true;
		sideBarButton();
    }
		
	void selectTheme(string theme) {
		if(stickerSelector.transform.childCount > 0) 
			detachChildren(stickerSelector.transform.GetChild(0).gameObject);
		
		// If previous active theme exists, detach & make invisible
		// Make new active theme visible and add listeners
		// Check if scene resources have already been imported
		if(themeStrings.Contains(theme) || newStickerThemeNames.Contains(theme)) {
			// If another theme is active, make it invisible
			if(activeTheme != null && activeTheme != themes[theme]) {
				togglePanels(activeTheme, false);
			}
			// Make new active theme visible
			togglePanels(themes[theme], true);
			activeTheme = themes[theme];
			activeTheme.transform.SetParent(stickerSelector.transform);

			// Slide in animation
			animStickers.enabled = true;
			slideIn(animStickers);
			stickerBarActive = true;
			return;
		}
	}

	void detachChildren(GameObject go) {
		go.transform.SetParent(null);
	}

	// Adding each theme type to the theme sidebar
	void populateSidebar() {
		populateMenus("Themes", "Theme", themeScroller);

		foreach (Transform theme in themeScroller.transform) {
			// Remove numbers from file name
			theme.name = Regex.Replace(theme.name, "[0-9]", "");
			populateStickers(theme.name);
            theme.GetChild(0).GetComponent<Text>().text = theme.name;
		}
		sideBarActive = true;
	}

	private void addDownloadedStickers() {
		// Return and begin game if no new stickers exist
		if(downloadedStickers.Count == 0) {
			initialized = true;
			return;
		}

		else {
			Dictionary<string, GameObject> newStickerThemes = new Dictionary<string, GameObject>();
			GameObject newThemePanel;
			GameObject newThemeIcon = new GameObject();
			// Add each sticker to the scene under their respective theme
			foreach (Sticker s in downloadedStickers) {
				if(s.stickerGO != null) {
					string captured = Path.GetFileNameWithoutExtension(s.stickerName);
					s.stickerGO.GetComponent<Button>().onClick.AddListener(() => addSticker(captured));

					// Theme already exists, add to list
					if(themes.ContainsKey(s.themeName)) {
						s.stickerGO.transform.SetParent(themes[s.themeName].transform);
					}
					// Theme doesn't exist
					// Create new GameObject for Icon & Scroller
					else {
						// Will only enter once for each new Theme
						newStickerThemeNames.Add(s.themeName);

						newStickerThemes.Add(s.themeName, s.stickerGO);
						// Create new sticker scroller panel for each new thme
						newThemePanel = Instantiate(stickerPanelTemplate);
						newThemePanel.name = s.themeName+"Panel";
						themes.Add(s.themeName, newThemePanel);
						s.stickerGO.transform.SetParent(newThemePanel.transform);
						// Initialize 
						newThemeIcon = Instantiate(Resources.Load("Theme")) as GameObject;
						newThemeIcon.name = s.themeName;
						newThemeIcon.transform.GetChild(0).GetComponent<Text>().text = s.themeName;
						newThemeIcon.GetComponent<Image>().sprite = s.stickerGO.GetComponent<Image>().sprite;
						newThemeIcon.transform.SetParent(themeScroller.transform);
						newThemeIcon.GetComponent<Button>().onClick.AddListener(() => selectTheme(newThemeIcon.name));
						// Initialize sticker in dictionary
						s.stickerName = Path.GetFileNameWithoutExtension(s.stickerName);
                        stickerSprites.Add(s.stickerName, s.stickerGO.GetComponent<Image>().sprite);
					}
				}
			}
			initialized = true;
		}
	}
	// Save scenes by taking screenshot of camera state.
	// Up to 5 scenes can be saved for each story
	public void takeScreenshot() {
		if (scenes.transform.childCount < 5) {
			screenshot();
			// remove all stickers and remove text from scene
			removeAllStickers();
			storyTextInput.text = "";
		}
	}

	private void removeAllStickers() {
		foreach(Transform child in  stickerPool.transform) {
			Destroy(child.gameObject);
		}
	}
	// Save scene as snapshot to save the scene for viewing later
	private void screenshot() {
		if (storyText.text.Length == 0) {
			storyText.transform.parent.GetComponent<CanvasGroup>().alpha = 0;
		}

		uiPanel.GetComponent<CanvasGroup>().alpha = 0;
		RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
		mainCamera.targetTexture = rt;

		GameObject nextScene = Instantiate(Resources.Load("Scene")) as GameObject;
		nextScene.name = "Scene" + (scenes.transform.childCount + 1);
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = mainCamera.targetTexture;
		mainCamera.Render();
		Rect r = new Rect(0, 0, mainCamera.targetTexture.width, mainCamera.targetTexture.height);
		Texture2D imageOverview = new Texture2D(mainCamera.targetTexture.width, mainCamera.targetTexture.height, TextureFormat.RGB24, false);
		imageOverview.ReadPixels(r, 0, 0);
		imageOverview.Apply();
		imageOverview.name = nextScene.name;
		savedScenes.Add(imageOverview);
		RenderTexture.active = currentRT;

		Sprite sp = Sprite.Create(imageOverview, r, new Vector2());

		nextScene.GetComponent<Image>().sprite = sp;
		nextScene.transform.SetParent(scenes.transform);
		nextScene.transform.position = nextScene.transform.parent.position;
		mainCamera.targetTexture = null;

		togglePanels(sceneSelector.gameObject, true);

		// Show instructions panel to prompt user
		if (scenes.transform.childCount == 1) showHideReminder(true);
		storyText.text = null;
		storyText.transform.parent.GetComponent<CanvasGroup>().alpha = 1;
		uiPanel.GetComponent<CanvasGroup>().alpha = 1;
	}

	void populateStickers(string theme) {
		themeStrings.Add(theme);
		string themeName = "Stickers/" + theme;
		stickerBarActive = true;

		GameObject themePanel = Instantiate(stickerSelector);
		populateMenus(themeName, "StickerSelector", themePanel);

		populatedLists = true;

		// Instantiate the stickers from the theme and add them to a list
		themePanel.name = theme + "Panel";
		themes.Add(theme, themePanel);

		foreach(Transform child in themePanel.transform) {
			string captured = child.name;
			child.GetComponent<Button>().onClick.AddListener(() => addSticker(captured));
		}
	}

	private void slideIn(Animator anim) {
		anim.Play("SlideIn");
	}

	private void slideOut(Animator anim) {
		anim.Play("SideBarSlideOut");
	}

	public void sideBarButton() {
		if(!stickerBarActive) {
			if (sideBarActive) {
				slideOut(anim);
				sideBarActive = false;
				btnMenu.transform.GetChild(0).GetComponent<Text>().text = "Menu";
			} else {
				btnMenu.transform.GetChild(0).GetComponent<Text>().text = "Cancel";
				slideIn(anim);
				sideBarActive = true;
			}
		} else {
			slideOut(animStickers);
			stickerBarActive = false;
		}
	}

	void addSticker(string name) {
		// Spawn a new sticker in the middle of the game area
		Debug.Log("STICKER " + name);
		GameObject sticker = Instantiate(Resources.Load("Sticker")) as GameObject;

		Sprite temp = stickerSprites[name];
		sticker.name = name;
		sticker.GetComponent<Image>().sprite = temp;
		sticker.transform.position = startPos.transform.position;
		sticker.transform.SetParent(stickerPool.transform, true);
	}

	public void deleteSticker() {
		if(stickerPool.transform.childCount > 0) {
			// Destroy last used sticker
			Destroy(stickerPool.transform.GetChild(stickerPool.transform.childCount-1).gameObject);
		}
	}

	private void populateMenus(string resource, string prefab, GameObject parent) {
		foreach (Texture2D t in Resources.LoadAll(resource, typeof(Texture2D))) {
			Rect r = new Rect(0, 0, t.width, t.height);
			GameObject i = Instantiate(Resources.Load(prefab)) as GameObject;
			Sprite temp = Sprite.Create(t, r, new Vector2());
            i.GetComponent<Image>().sprite = temp;

			if (prefab.Equals("StickerSelector")) {
				stickerSprites.Add(t.name, temp);
			}

			i.transform.SetParent(parent.transform);
			i.transform.position = i.transform.parent.position;
			i.name = t.name;
		}
	}

	public void showHideReminder(bool show) {
		CanvasGroup cg = GameObject.Find("ReminderPanel").GetComponent<CanvasGroup>();

		if (!show) {
			cg.alpha = 0;
			cg.interactable = false;
			cg.blocksRaycasts = false;
		} else {
			cg.alpha = 1;
			cg.interactable = true;
			cg.blocksRaycasts = true;
		}
	}

	public void saveStory() {
		storyFinished = true;
		confirmNameInput.text = storyName;

		if (savedScenes.Count != 0) {
			CanvasGroup cg = confirmNamePanel.GetComponent<CanvasGroup>();
			cg.alpha = 1;
			cg.interactable = true;
			cg.blocksRaycasts = true;
		}
	}

	public void setName() {
		CanvasGroup cg = nameStoryPanel.GetComponent<CanvasGroup>();
		cg.alpha = 0;
		cg.interactable = false;
		cg.blocksRaycasts = false;
		storyName = storyNameText.text;

		storyNameText.transform.parent.GetComponent<InputField>().text = "";
	}

	public void saveName() {
		string fileName = confirmNameInput.text.Trim();
		// For each child of scenes, save as name to filename/sceneName
		foreach (Texture2D scene in savedScenes) {
			if(scene != null) {
				byte[] bytes = scene.EncodeToPNG();

				string path = Application.persistentDataPath + "/" + fileName + "/";
				System.IO.Directory.CreateDirectory(path);
				File.WriteAllBytes(path + scene.name + ".png", bytes);
			}
		}
		Destroy(mainCamera);
		SceneManager.LoadScene("StoryViewer");
	}

	public void removePlaceHolder() {
		placeHolder.text = "";
	}

	// Deactivate/Reactivate panels
	void togglePanels(GameObject go, bool activate) {
		CanvasGroup cg = go.GetComponent<CanvasGroup>();

		if (activate) {
			cg.alpha = 1;
			cg.blocksRaycasts = true;
			cg.interactable = true;
		} else {
			cg.alpha = 0;
			cg.blocksRaycasts = false;
			cg.interactable = false;
		}
	}

	public bool isListPopulated() {
		return populatedLists;
	}
}

