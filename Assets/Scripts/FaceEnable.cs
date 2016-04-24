using UnityEngine;
using System.Collections;

public class FaceEnable : MonoBehaviour {

    public GameObject fact;
    private bool isActive = false;

    // Use this for initialization
	void Start() {
		if (fact.GetComponent<CanvasGroup>().alpha == 0) isActive = false;
		else isActive = true;
	}

    public void showHide()
    {
        if (!isActive)
        {
            fact.GetComponent<CanvasGroup>().alpha = 1;
            isActive = true;
        }
        else
        {
            fact.GetComponent<CanvasGroup>().alpha = 0;
            isActive = false;
        }

    }



}
