using UnityEngine;
using System.Collections;

public class FitToScreen : MonoBehaviour {

	// Made to fit a plane object to an orthographic camera

	public Camera cameraToFit;

	void Start( )
	{
		Vector3 newScale = transform.localScale;
		newScale.x = (float)cameraToFit.pixelWidth / 10.0f;
		newScale.z = (float)cameraToFit.pixelHeight / 10.0f;
		transform.localScale = newScale;
	}
}
