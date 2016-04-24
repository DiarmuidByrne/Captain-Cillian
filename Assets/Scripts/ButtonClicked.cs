using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class ButtonClicked : MonoBehaviour {

	public GameObject butPic1;
	public GameObject butWord1;
	public GameObject butPic2;
	public GameObject butWord2;
	public GameObject butPic3;
	public GameObject butWord3;
	public GameObject butPic4;
	public GameObject butWord4;
	public GameObject words, pics;
	public List<float> yPositions = new List<float>();
	private List<int> numsShuffle = new List<int>{0,1,2,3};
	private List<int> numsShuffle1 = new List<int>{0,1,2,3};
	private Random r;
	private int pic1 = 0;
	private int pic2 = 100;
	private int pic3 = 400;
	private int pic4 = 600;
	private int word1 = 200;
	private int word2 = 300;
	private int word3 = 500;
	private int word4 = 700;

	private int clickCount = 0;
	private static int endGame = 0;


	// Use this for initialization
	void Start () {

		// shuffle List of numbers.
		numsShuffle.Sort((a, b)=> 1 - 2 * Random.Range(0, 3));

		numsShuffle1.Sort((a, b)=> 1 - 2 * Random.Range(0, 3));

		Debug.Log ("Numbers shuffled in position 0 = " + numsShuffle[0]);
		Debug.Log ("Numbers shuffled in position 1 = " + numsShuffle[1]);
		Debug.Log ("Numbers shuffled in position 2 = " + numsShuffle[2]);
		Debug.Log ("Numbers shuffled in position 3 = " + numsShuffle[3]);

		// set endGame to zero 
		endGame = 0;

		initializeWords ();
		initializePics ();
	}

	
	private void initializeWords() {
		// Looping through each word button
		foreach (Transform child in words.transform) {
			yPositions.Add(child.position.y);
		}

		List<float> temp = yPositions;
		int i = 0;

		foreach (Transform child in words.transform) {

			if(i < numsShuffle.Count()){
				// assign each word button a random yPosition. Keep x position the same.
				child.position = new Vector2(child.transform.position.x, temp.ElementAt(numsShuffle.ElementAt(i)));
				
				i++;
			}

		}

	}

	private void initializePics() {
		// Looping through each word button
		foreach (Transform child in pics.transform) {
			yPositions.Add(child.position.y);
		}
		
		List<float> temp = yPositions;
		int i = 0;
		
		foreach (Transform child in pics.transform) {
			
			if(i < numsShuffle1.Count()){
				// assign each word button a random yPosition. Keep x position the same.
				child.position = new Vector2(child.transform.position.x, temp.ElementAt(numsShuffle1.ElementAt(i)));
				
				i++;
			}
			
		}
		
	}


	// Update is called once per frame
	void Update () {
	
		if (endGame == 4) {
			Destroy(GameObject.Find("Main Camera"));
			Application.LoadLevel("EndOfGame");
			
		}


			if(clickCount % 2 == 0)
			{
				clickCount = 0;
				//endGame = 0;
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
			if (pic1 == word1 && pic2 == word2 && pic3 == word3 && pic4 == word4) {
				Destroy(GameObject.Find("Main Camera"));
				Application.LoadLevel ("EndOfGame");
			}
			else{
				//Debug.Log("Reached");
					pic1 = 0;
					pic2 = 100;
					pic3 = 400;
					pic4 = 600;
					word1 = 200;
					word2 = 300;
					word3 = 500;
					word4 = 700;
				}

			//Debug.Log ("Even");
			if (pic1 == word1 && pic2 == word2 && pic3 == word3 && pic4 == word4) {
				Destroy(GameObject.Find("Main Camera"));
				Application.LoadLevel ("EndOfGame");
			}
				//srtBool = false;

			}// end if

		if (endGame % 2 == 0) {
			
			if (pic1 == word1 && pic2 == word2 && pic3 == word3 && pic4 == word4) {
				Application.LoadLevel ("EndOfGame");
			}
		}
		
		if (endGame == 4) {
			
			Application.LoadLevel("EndOfGame");
			
		}
		
		//}// end while 

	}

	public void screenShot() {
		Debug.Log ("Screen Shot captured");
		Application.CaptureScreenshot("Screenshot.png");
	}


	public void setPic1(int picNum)
	{
		pic1 = picNum;
		Debug.Log ("pic1 = " + pic1);
	}

	public void setPic2(int picNum2)
	{
		pic2 = picNum2;
		Debug.Log ("pic2 = " + pic2);
	}

	public void setPic3(int picNum3)
	{
		pic3 = picNum3;
		Debug.Log ("pic3 = " + pic3);
	}

	public void setPic4(int picNum4)
	{
		pic4 = picNum4;
		Debug.Log ("pic4 = " + pic4);
	}

	public void setWord1(int wrdNum)
	{
		word1 = wrdNum;
		Debug.Log ("word1 = " + word1);
	}
	
	public void setWord2(int wrdNum2)
	{
		word2 = wrdNum2;
		Debug.Log ("word2 = " + word2);
	}

	public void setWord3(int wrdNum3)
	{
		word3 = wrdNum3;
		Debug.Log ("word3 = " + word3);
	}

	public void setWord4(int wrdNum4)
	{
		word4 = wrdNum4;
		Debug.Log ("word4 = " + word4);
	}

	public void setClickCount()
	{
		clickCount++;
		Debug.Log ("clickCount = " + clickCount);
	}

	public void endGameCount()
	{
		endGame++;
	}

	public void getSound(string soundName)
	{
		switch(soundName)
		{
		case "button":

			Debug.Log("Button pressed");


			break;
		}
	}

	//shuffle array of numbers (isn't used in game)
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
