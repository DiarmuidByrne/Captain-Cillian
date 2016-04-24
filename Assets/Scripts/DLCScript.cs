using UnityEngine;
using System.Collections;

public class DLCScript : MonoBehaviour {
	private bool checkedForContent;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool hasCheckedForContent() {
		return checkedForContent;
	}

	public void setCheckedContent(bool check) {
		checkedForContent = check;
	}
}
