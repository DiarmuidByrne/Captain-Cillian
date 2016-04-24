using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

public class AddImageScript : MonoBehaviour {

	
	public Camera mainCamera;
	public Canvas sceneCanvas;
	
	public GameObject loadingPanel;

	public GameObject butPic1;
	public GameObject butPic2;
	public GameObject butPic3;
	public GameObject butPic4;
	public GameObject butPic5;
	public GameObject butPic6;
	public GameObject butPic7;

	public GameObject butWord1;
	public GameObject butWord2;
	public GameObject butWord3;
	public GameObject butWord4;
	public GameObject butWord5;
	public GameObject butWord6;
	public GameObject butWord7;
	public GameObject words, pics;
	private Dictionary<string, GameObject> buttonPics = new Dictionary<string, GameObject>();
	private Dictionary<string, GameObject> buttonWords = new Dictionary<string, GameObject>();

	private List<GameObject> wordList = new List<GameObject>();
	private List<GameObject> picList = new List<GameObject>();

	public GameObject[] goArray;
	private int[] numsShuffle;

	private Text loadingText;
	private Animator anim;
	
	private Dictionary<string, GameObject> themes = new Dictionary<string, GameObject>();
	
	private List<Sticker1> downloadedStickers = new List<Sticker1>();
	private float loadingTimer = 3f;
	private bool initialized = false;

	// Use this for initialization
	void Start () {
		// loading panel to indicate game is loading (downloading images from database).
		loadingText = loadingPanel.transform.GetChild(0).GetComponent<Text>();

		foreach (Transform child in words.transform) {
			wordList.Add(child.gameObject);
			buttonWords.Add(child.name, child.gameObject);
		}
		foreach (Transform child in pics.transform) {
			picList.Add(child.gameObject);
			buttonPics.Add(child.name, child.gameObject);
			//child.name.Substring(child.name.Length - 1);
		}
		Debug.Log(pics.transform.childCount);
	}
	
	void Update() {
		if (!mainCamera.GetComponent<AWSManager2>().isFinishedDownload()) {
			loadingTimer-=Time.deltaTime;
			if(loadingTimer <= 0) loadingTimer = 3f;
			else if (loadingTimer <= 3f && loadingTimer >= 2f) loadingText.text = "Downloading Content.";
			else if (loadingTimer <= 2f && loadingTimer >= 1f) loadingText.text = "Downloading Content..";
			else if (loadingTimer <= 1f && loadingTimer >= 0f) loadingText.text = "Downloading Content...";
			
		} else if (!initialized) {
			
			loadingPanel.GetComponent<CanvasGroup>().alpha = 0;
			loadingPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
			downloadedStickers = mainCamera.GetComponent<AWSManager2>().getDownloadedStickers();
			
			// Add new Themes from downloaded content
			addDownloadedStickers();
			initialized = true;
		}
	}
	
	void detachChildren(GameObject go) {
		go.transform.SetParent(null);
	}
	
	private void addDownloadedStickers() {
		// Return and begin game if no new stickers exist
		if(downloadedStickers.Count == 0) {
			initialized = true;
			return;
		}
		
		else {
			HashSet<string> newStickerThemeNames = new HashSet<string>();
			Dictionary<string, GameObject> newStickerThemes = new Dictionary<string, GameObject>();
			GameObject newThemeScroller = new GameObject();
			GameObject newThemeIcon = new GameObject();
			// Add each sticker to the scene under their respective theme
			foreach (Sticker1 s in downloadedStickers) {
				if(s.stickerGO != null) {				
					// replace '_' character with ' ' character for all image-names from database.
					s.stickerGO.GetComponent<Image>().name = s.stickerGO.GetComponent<Image>().name.Replace('_', ' ');

					// Theme already exists, add to list
					if(themes.ContainsKey(s.themeName)) {
						s.stickerGO.transform.SetParent(themes[s.themeName].transform);
					}
					// Theme doesn't exist
					// Create new GameObject
					else {
						// Will only enter once for each new Theme
						if(!newStickerThemes.ContainsKey(s.themeName)) {
							newStickerThemeNames.Add(s.themeName);
							newStickerThemes.Add(s.themeName, s.stickerGO);
						}
						s.stickerGO.transform.SetParent(newThemeScroller.transform);
					}
				}
			}
			if(newStickerThemeNames.Count > 0) {
				if (newThemeScroller.transform.childCount > 0) {
					
					Debug.Log("Number of stickers downloaded = " + downloadedStickers.Count);
					
					//New way
					goArray = new GameObject[downloadedStickers.Count];
					
					for(int i=0; i < downloadedStickers.Count; i++)
					{
						GameObject child = newThemeScroller.transform.GetChild(i).gameObject;
						newThemeIcon = child.GetComponent<Image>().gameObject;
						//goArray.SetValue(newThemeIcon.transform.gameObject, i);
						
						newThemeIcon.transform.GetComponent<Image>().sprite = child.GetComponent<Image>().sprite;
						goArray[i] = newThemeIcon.GetComponent<Image>().gameObject;
					}
					
					numsShuffle = new int[downloadedStickers.Count];
					Debug.Log("numsShuffle = " + numsShuffle.Length);
					
					// populate numsShuffle array
					for(int i=0; i < numsShuffle.Length; i++){
						numsShuffle[i] = i;
					}
					// shuffle List of numbers.
					reshuffle(numsShuffle);

					// Randomize values of pictures and corresponding words
					int j = 0;
					foreach (GameObject imageButton in buttonPics.Values) {
						Debug.Log("IMAGEBUTTON: " + imageButton.name);
						Debug.Log(j);
						imageButton.GetComponent<Image>().sprite = goArray[numsShuffle[j]].GetComponent<Image>().sprite;
						string index = imageButton.name.Substring(imageButton.name.Length - 1);
						GameObject temp = buttonWords["Word" + index];
						wordList.Remove(temp);
						Debug.Log("Removed Word" + index);

						int randomIndex = Random.Range(0, wordList.Count);

						wordList[randomIndex].transform.GetChild(0).GetComponent<Text>().text = goArray[numsShuffle[j]].GetComponent<Image>().name;
						wordList.Remove(wordList[randomIndex]);
						wordList.Add(temp);
						j++;
					}
					Debug.Log(wordList.Count);
					/*
					butPic1.GetComponent<Image>().sprite = goArray[numsShuffle[0]].GetComponent<Image>().sprite;
					// set the name of the button Word1 = to the name of the image added to the button of picture 1. 
					butWord1.GetComponent<Image>().name = goArray[numsShuffle[0]].GetComponent<Image>().name;
					// set the Text of button Word1 = to the Name of the button.
					butWord1.transform.GetChild(0).GetComponent<Text>().text = butWord1.gameObject.name.ToString(); 
					
					butPic2.GetComponent<Image>().sprite = goArray[numsShuffle[1]].GetComponent<Image>().sprite;
					butWord2.GetComponent<Image>().name = goArray[numsShuffle[1]].GetComponent<Image>().name;
					butWord2.transform.GetChild(0).GetComponent<Text>().text = butWord2.gameObject.name.ToString();
					
					butPic3.GetComponent<Image>().sprite = goArray[numsShuffle[2]].GetComponent<Image>().sprite;
					butWord3.GetComponent<Image>().name = goArray[numsShuffle[2]].GetComponent<Image>().name;
					butWord3.transform.GetChild(0).GetComponent<Text>().text = butWord3.gameObject.name.ToString();
					
					butPic4.GetComponent<Image>().sprite = goArray[numsShuffle[3]].GetComponent<Image>().sprite;
					butWord4.GetComponent<Image>().name = goArray[numsShuffle[3]].GetComponent<Image>().name;
					butWord4.transform.GetChild(0).GetComponent<Text>().text = butWord4.gameObject.name.ToString();

					butPic5.GetComponent<Image>().sprite = goArray[numsShuffle[4]].GetComponent<Image>().sprite;
					butWord5.GetComponent<Image>().name = goArray[numsShuffle[4]].GetComponent<Image>().name;
					butWord5.transform.GetChild(0).GetComponent<Text>().text = butWord5.gameObject.name.ToString();

					butPic6.GetComponent<Image>().sprite = goArray[numsShuffle[5]].GetComponent<Image>().sprite;
					butWord6.GetComponent<Image>().name = goArray[numsShuffle[5]].GetComponent<Image>().name;
					butWord6.transform.GetChild(0).GetComponent<Text>().text = butWord6.gameObject.name.ToString();

					butPic7.GetComponent<Image>().sprite = goArray[numsShuffle[6]].GetComponent<Image>().sprite;
					butWord7.GetComponent<Image>().name = goArray[numsShuffle[6]].GetComponent<Image>().name;
					butWord7.transform.GetChild(0).GetComponent<Text>().text = butWord7.gameObject.name.ToString();*/
				}
			}
			initialized = true;
		}
	}

	// shuffle array
	void reshuffle(int[] nums)
	{
		// Knuth shuffle algorithm :: courtesy of Wikipedia :)
		for (int t = 0; t < nums.Length; t++ )
		{
			int tmp = nums[t];
			int r = Random.Range(t, nums.Length);
			nums[t] = nums[r];
			nums[r] = tmp;
		}
	}
}
