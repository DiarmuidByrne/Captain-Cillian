using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;

public class AddIrishWordsScript : MonoBehaviour {
	
	
	public Camera mainCamera;
	public Canvas sceneCanvas;



	private Animator anim;
	private Dictionary<string, GameObject> themes = new Dictionary<string, GameObject>();
	private List<Sound> downloadedStickers = new List<Sound>();
	private float loadingTimer = 3f;
	private bool initialized = false;
	
	private LearnGameScript learnGS;
	
	// Use this for initialization
	void Start () {

		
	}
	
	void Update() {

		if (!mainCamera.GetComponent<AWSManager3> ().isFinishedDownload ()) {
			/*loadingTimer -= Time.deltaTime;
			if (loadingTimer <= 0)
				loadingTimer = 3f;
			else if (loadingTimer <= 3f && loadingTimer >= 2f)
				//loadingText.text = "Downloading Content.";
			else if (loadingTimer <= 2f && loadingTimer >= 1f)
				//loadingText.text = "Downloading Content..";
			else if (loadingTimer <= 1f && loadingTimer >= 0f)
				//loadingText.text = "Downloading Content...";*/
			
		} else if (!initialized) {

			//loadingPanel.GetComponent<CanvasGroup> ().alpha = 0;
			//loadingPanel.GetComponent<CanvasGroup> ().blocksRaycasts = false;
			downloadedStickers = mainCamera.GetComponent<AWSManager3> ().getDownloadedStickers ();

			// Add new Themes from downloaded content
			addDownloadedStickers ();
		}

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
			foreach (Sound s in downloadedStickers) {
				if(s.stickerGO != null) {
					string captured = Path.GetFileNameWithoutExtension(s.stickerName);
					//s.stickerGO.GetComponent<Button>().onClick.AddListener(() => addSticker(captured));
					
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
					
					Debug.Log("Number of Sounds downloaded = " + downloadedStickers.Count);
					
					//New way
					//goArray = new GameObject[downloadedStickers.Count];
					
					for(int i=0; i < downloadedStickers.Count; i++)
					{
						GameObject child = newThemeScroller.transform.GetChild(i).gameObject;
						newThemeIcon = child.GetComponent<Image>().gameObject;
						//goArray.SetValue(newThemeIcon.transform.gameObject, i);
						
						newThemeIcon.transform.GetComponent<Image>().sprite = child.GetComponent<Image>().sprite;
						//goArray[i] = newThemeIcon.GetComponent<Image>().gameObject;
					}
					
					/*numsShuffle = new int[downloadedStickers.Count];
					Debug.Log("numsShuffle = " + numsShuffle.Length);
					
					// populate numsShuffle array
					for(int i=0; i < numsShuffle.Length; i++){
						numsShuffle[i] = i;
					}
					// shuffle List of numbers.
					reshuffle(numsShuffle);
					
					for(int i=0; i < numsShuffle.Length; i++){
						//numsShuffle[i] = i;
						Debug.Log("Shuffled numbers array = " + numsShuffle);
					}*/
					
					
					/*butPic1.GetComponent<Image>().sprite = goArray[numsShuffle[0]].GetComponent<Image>().sprite;
					// set the name of the button Word1 = to the name of the image added to the button of picture 1. 
					butWord1.GetComponent<Image>().name = goArray[numsShuffle[0]].GetComponent<Image>().name;
					// set the Text of button Word1 = to the Name of the button.
					butWord1.transform.GetChild(0).GetComponent<Text>().text = butWord1.gameObject.name.ToString();
					learnGS.setTheIrishWord1(butWord1.gameObject.name.ToString());
					
					butPic2.GetComponent<Image>().sprite = goArray[numsShuffle[1]].GetComponent<Image>().sprite;
					butWord2.GetComponent<Image>().name = goArray[numsShuffle[1]].GetComponent<Image>().name;
					butWord2.transform.GetChild(0).GetComponent<Text>().text = butWord2.gameObject.name.ToString();
					learnGS.setTheIrishWord2(butWord2.gameObject.name.ToString());
					
					butPic3.GetComponent<Image>().sprite = goArray[numsShuffle[2]].GetComponent<Image>().sprite;
					butWord3.GetComponent<Image>().name = goArray[numsShuffle[2]].GetComponent<Image>().name;
					butWord3.transform.GetChild(0).GetComponent<Text>().text = butWord3.gameObject.name.ToString();
					learnGS.setTheIrishWord3(butWord3.gameObject.name.ToString());
					
					butPic4.GetComponent<Image>().sprite = goArray[numsShuffle[3]].GetComponent<Image>().sprite;
					butWord4.GetComponent<Image>().name = goArray[numsShuffle[3]].GetComponent<Image>().name;
					butWord4.transform.GetChild(0).GetComponent<Text>().text = butWord4.gameObject.name.ToString();
					learnGS.setTheIrishWord4(butWord4.gameObject.name.ToString());
					
					butPic5.GetComponent<Image>().sprite = goArray[numsShuffle[4]].GetComponent<Image>().sprite;
					butWord5.GetComponent<Image>().name = goArray[numsShuffle[4]].GetComponent<Image>().name;
					butWord5.transform.GetChild(0).GetComponent<Text>().text = butWord5.gameObject.name.ToString();
					learnGS.setTheIrishWord5(butWord5.gameObject.name.ToString());
					
					butPic6.GetComponent<Image>().sprite = goArray[numsShuffle[5]].GetComponent<Image>().sprite;
					butWord6.GetComponent<Image>().name = goArray[numsShuffle[5]].GetComponent<Image>().name;
					butWord6.transform.GetChild(0).GetComponent<Text>().text = butWord6.gameObject.name.ToString();
					learnGS.setTheIrishWord6(butWord6.gameObject.name.ToString());
					
					butPic7.GetComponent<Image>().sprite = goArray[numsShuffle[6]].GetComponent<Image>().sprite;
					butWord7.GetComponent<Image>().name = goArray[numsShuffle[6]].GetComponent<Image>().name;
					butWord7.transform.GetChild(0).GetComponent<Text>().text = butWord7.gameObject.name.ToString();
					learnGS.setTheIrishWord7(butWord7.gameObject.name.ToString());*/
					
					//old way Below
					
					/*GameObject firstChild = newThemeScroller.transform.GetChild(0).gameObject;
					GameObject secondChild = newThemeScroller.transform.GetChild(1).gameObject;
					GameObject thirdChild = newThemeScroller.transform.GetChild(2).gameObject;
					GameObject fourthChild = newThemeScroller.transform.GetChild(3).gameObject;

					butPic1.GetComponent<Image>().sprite = firstChild.GetComponent<Image>().sprite;
					// set the name of the button Word1 = to the image added to the button of picture 1. 
					butWord1.GetComponent<Image>().name = firstChild.GetComponent<Image>().name;
					// set the Text of button Word1 = to the Name of the button.
					butWord1.transform.GetChild(0).GetComponent<Text>().text = butWord1.gameObject.name.ToString(); 

					butPic2.GetComponent<Image>().sprite = secondChild.GetComponent<Image>().sprite;
					butWord2.GetComponent<Image>().name = secondChild.GetComponent<Image>().name;
					butWord2.transform.GetChild(0).GetComponent<Text>().text = butWord2.gameObject.name.ToString();

					butPic3.GetComponent<Image>().sprite = thirdChild.GetComponent<Image>().sprite;
					butWord3.GetComponent<Image>().name = thirdChild.GetComponent<Image>().name;
					butWord3.transform.GetChild(0).GetComponent<Text>().text = butWord3.gameObject.name.ToString();

					butPic4.GetComponent<Image>().sprite = fourthChild.GetComponent<Image>().sprite;
					butWord4.GetComponent<Image>().name = fourthChild.GetComponent<Image>().name;
					butWord4.transform.GetChild(0).GetComponent<Text>().text = butWord4.gameObject.name.ToString();*/
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
