using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour {
	public Button saveSettingsButton;
	public GameObject optionsPanel;
	private CanvasGroup cgOptions;
	public Toggle checkForContent;
	public InputField email;
	public InputField password;

	public void Start() {
		if (!PlayerPrefs.HasKey("userEmail")) {
			PlayerPrefs.SetString("userEmail", "");
		}
		if (!PlayerPrefs.HasKey("checkForContent")) {
			PlayerPrefs.SetInt("checkForContent", 1);
		}

		email.text = PlayerPrefs.GetString("userEmail");
		password.text = PlayerPrefs.GetString("userPassword");

		if (PlayerPrefs.GetInt("checkForContent") == 0) {
			checkForContent.isOn = false;
		} else checkForContent.isOn = true;
	}

	public void showOptions() {
		CanvasGroup cg = optionsPanel.GetComponent<CanvasGroup>();
		cg.alpha = 1;
		cg.interactable = true;
		cg.blocksRaycasts = true;
	}

	public void hideOptions() {
		saveSettings();
		CanvasGroup cg = optionsPanel.GetComponent<CanvasGroup>();
		cg.alpha = 0;
		cg.interactable = false;
		cg.blocksRaycasts = false;
	}

	private void saveSettings() {
		if (checkForContent.isOn) PlayerPrefs.SetInt("checkForContent", 1);
		else PlayerPrefs.SetInt("checkForContent", 0);

		PlayerPrefs.SetString("userEmail", email.text);
		PlayerPrefs.SetString("userPassword", password.text);
	}
}
