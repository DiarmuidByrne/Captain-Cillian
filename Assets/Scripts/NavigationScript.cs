using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NavigationScript : MonoBehaviour {
	public GameObject camera;
	public GameObject loadingPanel;
    // Manages the navigations between different scenes

	public void loadExploreScene() {
       SceneManager.LoadScene("Explore");
    }

	public void loadCreateSelectorScene() {
		SceneManager.LoadScene("CreateSelector");
	}

	public void loadStoryViewerScene() {
		SceneManager.LoadScene("StoryViewer");
	}

    public void loadMainMenu() {
		if (SceneManager.GetActiveScene().name == "Create") {
			Destroy(camera);
		}
        SceneManager.LoadScene("MainMenu");
    }

    public void loadLearnScene() {
        SceneManager.LoadScene("Learn");
    }

    public void loadCreateScene() {
		loadingPanel.GetComponent<CanvasGroup>().alpha = 1;
        SceneManager.LoadSceneAsync("Create");
    }
    public void loadMatchUp()
    {
        SceneManager.LoadScene("Main");
    }
    public void loadslidingpuzzle()
    {
        SceneManager.LoadScene("PM Sliding Pieces Example");
    }
    public void loadpickAndPlacepuzzle()
    {
        SceneManager.LoadScene("PM Pick And Place Example");
    }
    public void loadjoinpuzzle()
    {
        SceneManager.LoadScene("PM Join Pieces Example");
    }

    public void loadFunScene() {
         SceneManager.LoadScene("Fun");
    }

    public void loadStormySea()
    {
        Application.OpenURL("http://www.captaincillian.com/store/p2/Stormy_Sea_Adventure.html");
    }
    public void loadSurfing()
    {
        Application.OpenURL("http://www.captaincillian.com/store/p4/Surfing_Adventure.html");
    }
    public void loadHiddenCaves()
    {
        Application.OpenURL("http://www.captaincillian.com/store/p5/Hidden_Caves_Adventure.html");
    }
}
