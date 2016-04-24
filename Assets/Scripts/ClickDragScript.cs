using UnityEngine;

using System.Collections;

[RequireComponent(typeof(BoxCollider))]

public class ClickDragScript : MonoBehaviour {
	public Transform gameCanvas;
	public Transform gamePanel;
	private Vector3 screenPoint; private Vector3 offset; private float _lockedYPosition;
	private bool activeSticker;
	private Vector3 initialScale;

	private Vector3 maxScale, minScale;
	private float scaleFactor = 0.001f; // The rate of change of the scale

	void Start() {
		gameCanvas = GameObject.Find("Canvas").transform;
		gamePanel = GameObject.Find("Stickers").transform;
		initialScale = transform.localScale;
		// Max scale is 4 times the original size
		maxScale = initialScale * 4;
		// Min scale is the original size
		minScale = initialScale;
	}

	void Update() {
		if(Input.GetMouseButton(0)) {
			RaycastHit hit = new RaycastHit();
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			

			if (Physics.Raycast(ray, out hit)) {
				if (hit.collider.gameObject == gameObject) {
					activeSticker = true;
				}
			}
		}

		if (Input.touchCount == 2) {
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			// Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = ((touchZeroPrevPos - touchOnePrevPos).magnitude) / 2;
			float touchDeltaMag = ((touchZero.position - touchOne.position).magnitude) / 2;

			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = ((prevTouchDeltaMag - touchDeltaMag) / 4) * -1;

			// Scale object if it's greater than half and less than 3 times the original size
			if (activeSticker) {
				Debug.Log("Reached " + transform.name);
				if (transform.localScale.magnitude >= minScale.magnitude
					&& transform.localScale.magnitude < maxScale.magnitude) {
					transform.localScale += new Vector3(deltaMagnitudeDiff, deltaMagnitudeDiff, scaleFactor);
				} 
				else if (transform.localScale.magnitude < minScale.magnitude) {
					// If sticker is less than minimum size, return to minimum size
					transform.localScale = initialScale;

				} else if (transform.localScale.magnitude > (maxScale.magnitude)) {
					// If sticker is more than max size, return to regular size
					transform.localScale = initialScale;
				}
			}
		}
	}
	void OnMouseDown() {
#if !UNITY_EDITOR
		if(Input.touchCount == 1) {
			// Center object on camera
			screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

			offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
		} 
#endif
#if UNITY_EDITOR
		
		// Center object on camera
		screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

		// Set the active sticker as the last one dragged
		offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
#endif
	}

	void OnMouseDrag() {
#if !UNITY_EDITOR
		if (Input.touchCount == 1) {
			transform.SetParent(gameCanvas);
			Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
			Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
			transform.position = curPosition;
		}
#endif
#if UNITY_EDITOR
		transform.SetParent(gameCanvas);
		Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
		Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
		transform.position = curPosition;
#endif
	}

	void OnMouseUp() {
		transform.SetParent(gamePanel);
		activeSticker = false;
	}
}