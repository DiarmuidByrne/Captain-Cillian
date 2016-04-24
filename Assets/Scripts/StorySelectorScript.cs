using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class StorySelectorScript : MonoBehaviour {

	public GameObject storySelectionPanel;
	public GameObject storySelector, storyViewer;
	public GameObject storyImage, emailPanel;
	public GameObject confirmationPanel;
	public Button sendEmailButton;

	public InputField messageInput, recipientAddress;
	public Text messagePlaceHolder, recipientPlaceholder;
	public Text buttonText, confirmationText;

	private GameObject storyTemplate;
	private string pathPrefix;
	private List<Texture2D> currentStory = new List<Texture2D>();
	private List<string> currentStoryImagePaths = new List<string>();
	private int imageIndex = 0;
	private bool error = false;
	// Use this for initialization
	void Start () {
		pathPrefix = "file:/";
        string rootPath = Application.persistentDataPath;
		string[] directories = Directory.GetDirectories(rootPath);
		// Load the story prefab as a template
		storyTemplate = Instantiate(Resources.Load("Story")) as GameObject;
		int i = 0;
		// Get each story folder in the root app path
		foreach(string storyFolder in directories) {
			// Get each file in the story folder
			string captured = storyFolder;
			string[] fileEntries = Directory.GetFiles(storyFolder);
			string lastFolderName;
			// Load any found textures
			if (fileEntries.Length > 0) {
				lastFolderName = Path.GetFileName(storyFolder);

				Texture2D thumbnail = loadPNG(fileEntries[0]);

				Sprite s = convertTexToSprite(thumbnail);
				GameObject temp = Instantiate(storyTemplate);
				RectTransform rt = (RectTransform)storySelectionPanel.transform;
				temp.name = lastFolderName;
				Transform p = temp.transform.GetChild(0);
				//temp.transform. = new Vector3(1, rt.rect.height, 1);
				p.GetChild(0).GetComponent<Text>().text = temp.name;
				temp.transform.SetParent(storySelectionPanel.transform, false);
				temp.GetComponent<Image>().sprite = s;

				// Add listeners to each story
				temp.GetComponent<Button>().onClick.AddListener(() => showStory(captured));

				i++;
			}
		}
    }

	public void showEmailPanel() {
		togglePanels(emailPanel, true);
	}

	public void showConfirmationPanel() {
		togglePanels(confirmationPanel, true);
		if (error) {
			buttonText.text = "OK";
			confirmationText.text = "Error\nEmail not sent";
		} else {
			buttonText.text = "Continue";
			confirmationText.text = "Email sent!";
		}
	}

	public void hideConfirmationPanel() {
		togglePanels(confirmationPanel, false);

		if (!error) {
			togglePanels(emailPanel, false);
		}
	}

	public void prepareEmail() {
		// Check conditions for sending
		if (PlayerPrefs.GetString("userEmail").Length > 0) {
			// Send email
			sendEmail();
			// Show email sent message
			showConfirmationPanel();
		}
		else {
			// User hasn't saved an email address, show prompt to add one
		}
	}

	private void sendEmail() {
		string email = PlayerPrefs.GetString("userEmail");
		string password = PlayerPrefs.GetString("userPassword");
		string recipientEmail = recipientAddress.text;
		MailMessage mail = new MailMessage();

		if(!isValidEmail(recipientEmail) || !isValidEmail(email)) {
			error = true;
			return;
		}

		mail.From = new MailAddress(email);
		mail.To.Add(recipientEmail);
		mail.Subject = "You got sent a Captain Cillian story!";
		mail.Body = messageInput.text;

		foreach (string filePath in currentStoryImagePaths) {
			Attachment att = new Attachment(filePath);
			mail.Attachments.Add(att);
		}

		SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
		smtpServer.Port = 587;

		smtpServer.Credentials = new System.Net.NetworkCredential(email, password) as ICredentialsByHost;
		smtpServer.EnableSsl = true;

		ServicePointManager.ServerCertificateValidationCallback =
			delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
			{ return true; };

		try {
			smtpServer.Send(mail);
			error = false;
		} catch (SmtpException ex) {
			// Show error panel
			error = true;
			Debug.Log(ex);
		}
    }

	void Update() {
		if (messageInput.text.Length != 0) messagePlaceHolder.text = "";
		else messagePlaceHolder.text = "Write your message here";

		if (recipientAddress.text.Length != 0) {
			sendEmailButton.interactable = true;
			recipientPlaceholder.text = "";
		} else {
			recipientPlaceholder.text = "Recipient's email";
			sendEmailButton.interactable = false;
		}
	}

	public void nextImage() {
		if(imageIndex < currentStory.Count-1) {
			imageIndex++;
			storyImage.GetComponent<Image>().sprite = convertTexToSprite(currentStory[imageIndex]);
		}
	}

	bool isValidEmail(string email) {
		if (email.Contains("@") && email.Contains(".")) return true;
		else return false;
	}

	public void prevImage() {
		if (imageIndex > 0) {
			imageIndex--;
			storyImage.GetComponent<Image>().sprite = convertTexToSprite(currentStory[imageIndex]);
		}
	}

	public void returnToStorySelection() {
		currentStory.Clear();
		togglePanels(storyViewer, false);
		togglePanels(storySelector, true);
	}

	public void showStory(string storyFolder) {
		// Get each image path as a string
		string[] fileEntries = Directory.GetFiles(storyFolder);
		
		for(int i=0; i<fileEntries.Length; i++) {
			currentStoryImagePaths.Add(fileEntries[i]);
            currentStory.Add(loadPNG(fileEntries[i]));
		}

		storyImage.GetComponent<Image>().sprite = convertTexToSprite(currentStory[0]);
		togglePanels(storyViewer, true);
		togglePanels(storySelector, false);
	}

	private void togglePanels(GameObject g, bool activate) {
		CanvasGroup cg = g.GetComponent<CanvasGroup>();

		if(!activate) {
			cg.alpha = 0;
			cg.blocksRaycasts = false;
			cg.interactable = false;
		} else {
			cg.alpha = 1;
			cg.blocksRaycasts = true;
			cg.interactable = true;
		}
	}

	private Sprite convertTexToSprite(Texture2D t) {
		Rect r = new Rect(0, 0, t.width, t.height);// (RectTransform)storyTemplate.transform;
		Sprite s = Sprite.Create(t, r, new Vector2());
		return s;
	}

	private Texture2D loadPNG(string filePath) {
		Texture2D tex = null;
		byte[] fileData;

		if (File.Exists(filePath)) {
			fileData = File.ReadAllBytes(filePath);
			tex = new Texture2D(2, 2);
			tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		}

		return tex;
	}
}