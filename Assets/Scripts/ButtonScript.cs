using UnityEngine;
using System.Collections;

public class ButtonScript : MonoBehaviour {
	private string btnValue;

	public void setBtnValue(string value) {
		btnValue = value;
	}

	public string getBtnValue() {
		return btnValue;
	}
}
