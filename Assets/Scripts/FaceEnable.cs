using UnityEngine;
using System.Collections;

public class FaceEnable : MonoBehaviour {

    public GameObject fact;
    private bool isActive = false;

    // Use this for initialization
    void Start()
    {


    }
    public void showHide()
    {
        if (!isActive)
        {
            fact.GetComponent<CanvasGroup>().alpha = 0;
            isActive = true;
        }
        else
        {
            fact.GetComponent<CanvasGroup>().alpha = 1;
            isActive = false;
        }

    }



}
