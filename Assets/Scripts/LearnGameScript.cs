using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class LearnGameScript : MonoBehaviour {

	public GameObject butPic1;
	public GameObject butWord1;
	public GameObject butPic2;
	public GameObject butWord2;
	public GameObject butPic3;
	public GameObject butWord3;
	public GameObject butPic4;
	public GameObject butWord4;
	public GameObject butPic5;
	public GameObject butWord5;
	public GameObject butPic6;
	public GameObject butWord6;
	public GameObject butPic7;
	public GameObject butWord7;
	public GameObject words, pics;
	public List<float> yPositions = new List<float>();
	public List<float> xPositions = new List<float>();
	public List<float> yPositions1 = new List<float>();
	public List<float> xPositions1 = new List<float>();
	//private List<int> numsShuffle = new List<int>{0,1,2,3,4,5,6};
	//private List<int> numsShuffle1 = new List<int>{0,1,2,3,4,5,6};
	private int[] numsShuffle2 = new int[]{0,1,2,3,4,5,6};
	private int[] numsShuffle3 = new int[]{0,1,2,3,4,5,6};
	private Random r;
	private int pic1 = 0;
	private int pic2 = 100;
	private int pic3 = 400;
	private int pic4 = 600;
	private int pic5 = 800;
	private int pic6 = 1000;
	private int pic7 = 1200;

	private int word1 = 200;
	private int word2 = 300;
	private int word3 = 500;
	private int word4 = 700;
	private int word5 = 900;
	private int word6 = 1100;
	private int word7 = 1300;

	private string irishWord1;
	private string irishWord2;
	private string irishWord3;
	private string irishWord4;
	private string irishWord5;
	private string irishWord6;
	private string irishWord7;

	private int clickCount = 0;
	private static int endGame;

	private AudioClip wordInIrish;


	// Use this for initialization
	void Start () {

		// Not used in game. First attempt at shuffling numbers.
		// shuffle List of numbers. 
		// for words transform
		//numsShuffle.Sort((a, b)=> 1 - 2 * Random.Range(0, 6));
		// for pics transform
		//numsShuffle1.Sort((a, b)=> 1 - 2 * Random.Range(0, 6));

		// shuffle the int[] to be used when randomising positioning of buttons on screen.
		reshuffle (numsShuffle2);
		reshuffle (numsShuffle3);

		// set endGame to zero 
		endGame = 0;

		initializeWords ();
		initializePics ();
	}

	// Sets the positions for the word buttons.
	// loops through each word button and adds it's position to two List<float>'s.
	// assign each word button a random xPosition & yPosition.
	private void initializeWords() {
		// Looping through each word button
		foreach (Transform child in words.transform) {
			yPositions.Add(child.position.y);
			xPositions.Add(child.position.x);
		}

		List<float> temp = yPositions;
		List<float> temp1 = xPositions;
		int i = 0;

		foreach (Transform child in words.transform) {

			if(i < numsShuffle2.Count()){
				// assign each word button a random xPosition & yPosition.
				child.position = new Vector2(temp1.ElementAt(numsShuffle2.ElementAt(i)), temp.ElementAt(numsShuffle2.ElementAt(i)));
				
				i++;
			}

		}

	}
	
	private void initializePics() {
		// Looping through each pic button
		foreach (Transform child in pics.transform) {
			yPositions1.Add(child.position.y);
			xPositions1.Add(child.position.x);
		}
		
		List<float> temp = yPositions1;
		List<float> temp1 = xPositions1;

		int i = 0;
		
		foreach (Transform child in pics.transform) {
			
			if(i < numsShuffle3.Count()){
				// assign each pic button a random xPosition & yPosition.
				child.position = new Vector2(temp1.ElementAt(numsShuffle3.ElementAt(i)), temp.ElementAt(numsShuffle3.ElementAt(i)));
				
				i++;
			}
			
		}
		
	}


	// Game logic
	// if clickcount divided by 2 has no remainder, it is  an even number( 2 buttons have been clicked).
	// if even number enter game evaluation.
	// if pic and word buttons are equal they are removed from game.
	// if not equal reset values at bottom.
	// endGame is incremented by 1.
	void Update () {
	
		if (endGame == 7) {
			Destroy(GameObject.Find("Main Camera"));
			Application.LoadLevel("EndOfGame");
		}

		if(clickCount % 2 == 0)
		{
			clickCount = 0;
	
			if (pic1 == word1) {

				butPic1.SetActive (false);
				butWord1.SetActive (false);
				endGame++;
			} 

			if (pic2 == word2) {

				butPic2.SetActive (false);
				butWord2.SetActive (false);
				endGame++;
			} 
			if (pic3 == word3) {
			
				butPic3.SetActive (false);
				butWord3.SetActive (false);
				endGame++;
			} 
			if (pic4 == word4) {
			
				butPic4.SetActive (false);
				butWord4.SetActive (false);
				endGame++;
			}
			if (pic5 == word5) {
				
				butPic5.SetActive (false);
				butWord5.SetActive (false);
				endGame++;
			}
			if (pic6 == word6) {
				
				butPic6.SetActive (false);
				butWord6.SetActive (false);
				endGame++;
			}
			if (pic7 == word7) {
				
				butPic7.SetActive (false);
				butWord7.SetActive (false);
				endGame++;
			}
			if (pic1 == word1 && pic2 == word2 && pic3 == word3 && pic4 == word4) {
				Application.LoadLevel ("EndOfGame");
			}

			else{
			//Debug.Log("Reached");
				// reset values if the 2 buttons not equal.
				pic1 = 0;
				pic2 = 100;
				pic3 = 400;
				pic4 = 600;
				pic5 = 800;
				pic6 = 1000;
				pic7 = 1200;
				word1 = 200;
				word2 = 300;
				word3 = 500;
				word4 = 700;
				word5 = 900;
				word6 = 1100;
				word7 = 1300;
			}

		}

	}

	public void screenShot() {
		Debug.Log ("Screen Shot captured");
		Application.CaptureScreenshot("Screenshot.png");
	}

	// sets pic1 to whatever value you have given it when button pic1 is pressed.
	public void setPic1(int picNum)
	{
		pic1 = picNum;
		Debug.Log ("pic1 = " + pic1);
		getIrishWord (irishWord1);
	}

	public void setPic2(int picNum2)
	{
		pic2 = picNum2;
		Debug.Log ("pic2 = " + pic2);
		getIrishWord (irishWord2);
	}

	public void setPic3(int picNum3)
	{
		pic3 = picNum3;
		Debug.Log ("pic3 = " + pic3);
		getIrishWord (irishWord3);
	}

	public void setPic4(int picNum4)
	{
		pic4 = picNum4;
		Debug.Log ("pic4 = " + pic4);
		getIrishWord (irishWord4);
	}

	public void setPic5(int picNum5)
	{
		pic5 = picNum5;
		Debug.Log ("pic5 = " + pic5);
		getIrishWord (irishWord5);
	}

	public void setPic6(int picNum6)
	{
		pic6 = picNum6;
		Debug.Log ("pic6 = " + pic6);
		getIrishWord (irishWord6);
	}

	public void setPic7(int picNum7)
	{
		pic7 = picNum7;
		getIrishWord (irishWord7);
	}

	public void setWord1(int wrdNum)
	{
		word1 = wrdNum;
	}
	
	public void setWord2(int wrdNum2)
	{
		word2 = wrdNum2;
	}

	public void setWord3(int wrdNum3)
	{
		word3 = wrdNum3;
	}

	public void setWord4(int wrdNum4)
	{
		word4 = wrdNum4;
	}

	public void setWord5(int wrdNum5)
	{
		word5 = wrdNum5;
	}

	public void setWord6(int wrdNum6)
	{
		word6 = wrdNum6;
	}

	public void setWord7(int wrdNum7)
	{
		word7 = wrdNum7;
	}

	// incremented by 1 everytime a button is clicked
	public void setClickCount()
	{
		clickCount++;
		//Debug.Log ("clickCount = " + clickCount);
	}

	public void endGameCount()
	{
		endGame++;
	}

	public void setTheIrishWord1(string word)
	{
		word = word.Replace (' ', '_');
		irishWord1 = word;
	}

	public void setTheIrishWord2(string word)
	{
		word = word.Replace (' ', '_');
		irishWord2 = word;
	}

	public void setTheIrishWord3(string word)
	{
		word = word.Replace (' ', '_');
		irishWord3 = word;
	}

	public void setTheIrishWord4(string word)
	{
		word = word.Replace (' ', '_');
		irishWord4 = word;
	}

	public void setTheIrishWord5(string word)
	{
		word = word.Replace (' ', '_');
		irishWord5 = word;
	}

	public void setTheIrishWord6(string word)
	{
		word = word.Replace (' ', '_');
		irishWord6 = word;
	}

	public void setTheIrishWord7(string word)
	{
		word = word.Replace (' ', '_');
		irishWord7 = word;
	}

	public void getIrishWord(string irishWord)
	{
		AudioSource audio = GetComponent<AudioSource> ();

		wordInIrish = Resources.Load ("Audio/"+irishWord, typeof(AudioClip)) as AudioClip;
		audio.PlayOneShot (wordInIrish);
	}

	// shuffle array of numbers 
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
