using System;
using System.Collections.Generic;
using System.Configuration;
using Amazon.DynamoDBv2;
using UnityEngine.UI;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using UnityEngine;
using System.IO;
using Amazon;
using Amazon.Util;
using Amazon.CognitoIdentity;


public class AWSManager3 : DynamoDbBaseExample {
	/********************************************************************************/
	
	public GameObject gameCanvas;
	public GameObject stickerMenu;
	public GameObject dlc;
	
	private IAmazonDynamoDB _client;
	private DynamoDBContext _context;
	
	private bool downloaded = false;
	
	private IAmazonS3 _s3Client;
	private AWSCredentials _credentials;
	private string S3BucketName;
	
	private List<Sound> downloadedStickers = new List<Sound>();
	private Dictionary<string, Sound> stickers = new Dictionary<string, Sound>();
	
	private bool retrievedAllFromDatabase = false;
	private bool finishedDownload = false;
	
	private int completedDownload = 0;

	//for AudioClip creation
	private int position = 0;
	private int sampleRate = 0;
	private float frequency = 440;
	
	
	
	private RegionEndpoint _CognitoIdentityRegion {
		get { return RegionEndpoint.GetBySystemName(RegionEndpoint.EUWest1.SystemName); }
	}
	
	private RegionEndpoint _S3Region = RegionEndpoint.EUWest1;
	
	private AWSCredentials Credentials {
		get {
			if (_credentials == null)
				_credentials = new CognitoAWSCredentials(IdentityPoolId, _CognitoIdentityRegion);
			return _credentials;
		}
	}
	
	private IAmazonS3 S3Client {
		get {
			if (_s3Client == null) {
				_s3Client = new AmazonS3Client(Credentials, _S3Region);
			}
			return _s3Client;
		}
	}
	
	private DynamoDBContext Context {
		get {
			if (_context == null)
				_context = new DynamoDBContext(_client);
			
			return _context;
		}
	}
	
	void Start() {
		_client = Client;
		
		S3BucketName = "sounds-learn-game";
	}
	
	/********************************************************************************/
	
	void Update() {
		if (!downloaded) {
			FindAllStickers(null);
			downloaded = true;
		}
		if (retrievedAllFromDatabase && !finishedDownload) {
			foreach (Sound s in stickers.Values) {
				GetObjects(s);
				downloadedStickers.Add(s);
			}
			retrievedAllFromDatabase = false;
		}
	}
	
	private void FindAllStickers(Dictionary<string, AttributeValue> lastKeyEvaluated) {
		Debug.Log("Started retrieval");
		var request = new ScanRequest {
			TableName = "WordsInIrish",
			ExclusiveStartKey = lastKeyEvaluated
		};
		
		_client.ScanAsync(request, (result) => {
			foreach (Dictionary<string, AttributeValue> item
			         in result.Response.Items) {
				// New sticker found, add info to new sticker object
				addItem(item);
			}
			lastKeyEvaluated = result.Response.LastEvaluatedKey;
			if (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0) {
				FindAllStickers(lastKeyEvaluated);
			} else {
				retrievedAllFromDatabase = true;
			}
		});
	}
	
	private void addItem(Dictionary<string, AttributeValue> attributeList) {
		Sound sticker = new Sound();
		
		foreach (var kvp in attributeList) {
			string attributeName = kvp.Key;
			AttributeValue value = kvp.Value;
			// Create a new sticker based on each Key/Value pair
			if (attributeName == "Theme") {
				sticker.themeName = value.S;
			} else if (attributeName == "StickerName") {
				sticker.stickerName = value.S;
			} else if (attributeName == "Link") {
				sticker.link = value.S;
			}
		}
		stickers.Add(sticker.stickerName, sticker);
	}
	
	public void GetObjects(Sound s) {

		string file = s.themeName + "/" + s.stickerName;
		
		S3Client.GetObjectAsync(S3BucketName, file, (responseObj) =>
		                        
		                        {
			var response = responseObj.Response;

			if (response.ResponseStream != null) {
				AudioClip t = new AudioClip();
				byte[] imageData = new byte[response.ResponseStream.Length];
				response.ResponseStream.Read(imageData, 0, (int)imageData.Length);
				t.LoadAudioData();
				t.name = s.stickerName;
				instantiateSounds(t, s);

				AudioClip soundInstance = Instantiate(t) as AudioClip;
			} else {
				Debug.Log("Nothing found in Bucket");
			}
		});
	}
	
	// Change this to a foreach, when all are downloaded
	/*private void instantiateTexture(Texture2D t, Sound s) {
		
		// Instantiate a sticker object and assign it the texture
		Rect r = new Rect(0, 0, t.width, t.height);
		Sprite temp = Sprite.Create(t, r, new Vector2());
		temp.name = t.name;
		
		GameObject stickerButton = Instantiate(Resources.Load("StickerSelector")) as GameObject;
		stickerButton.transform.SetParent(dlc.transform);
		stickerButton.transform.position = stickerButton.transform.parent.position;
		stickerButton.GetComponent<Image>().sprite = temp;
		stickerButton.name = Path.GetFileNameWithoutExtension(temp.name);
		// Add Texture to sticker object
		s.stickerGO = stickerButton;
		completedDownload++;
		if (completedDownload == downloadedStickers.Count)
			finishedDownload = true;
	}*/

	private void instantiateSounds(AudioClip t, Sound s) {
		
		// Instantiate a sticker object and assign it the texture
		AudioClip temp = AudioClip.Create ("MySinoid", 44100, 1, 44100, false, true, OnAudioRead, OnAudioSetPosition);
		temp.name = t.name;
		
		GameObject stickerButton = Instantiate(Resources.Load("StickerSelector")) as GameObject;
		stickerButton.transform.SetParent(dlc.transform);
		stickerButton.transform.position = stickerButton.transform.parent.position;
		stickerButton.GetComponent<AudioSource>().clip = temp;
		stickerButton.name = Path.GetFileNameWithoutExtension(temp.name);
		// Add Texture to sticker object
		s.stickerGO = stickerButton;
		completedDownload++;
		if (completedDownload == downloadedStickers.Count)
			finishedDownload = true;
	}

	
	public List<Sound> getDownloadedStickers() {
		return downloadedStickers;
	}
	
	public bool isFinishedDownload() {
		return finishedDownload;
	}
	
	
	public void setFinishedDownload(bool finished) {
		this.finishedDownload = finished;
	}

	public void OnAudioRead(float[] data)
	{
		for (var count = 0; count < data.Length; count++)
		{
			data[count] = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * frequency * position / sampleRate));
			position++;
		}
	}
	public void OnAudioSetPosition(int newPosition)
	{
		position = newPosition;
	}
	
}

public class Sound {
	public string themeName;
	public string stickerName;
	public string link;
	public GameObject stickerGO;
}
