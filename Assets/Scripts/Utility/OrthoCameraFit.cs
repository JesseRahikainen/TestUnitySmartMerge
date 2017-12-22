using UnityEngine;

using System.Collections;

[RequireComponent(typeof(Camera))]
public class OrthoCameraFit : MonoBehaviour {

	public RectTransform baseRT;

	private Camera _camera;

	void Start( )
	{
		_camera = GetComponent<Camera>( );
		FitCameraToBaseRectTransform( );
	}

	private void FitCameraToBaseRectTransform( )
	{
		if( baseRT == null ) {
			Debug.LogError( "OrthoCameraFit has no baseRT." );
			return;
		}

		Vector2 size = Utils.GetWorldRectTransformSize( baseRT );
		Vector2 cameraWorldSize;

		float baseRatio = size.x / size.y;
		if( _camera.aspect < baseRatio ) {
			cameraWorldSize.y = size.x / _camera.aspect;
			cameraWorldSize.x = cameraWorldSize.y * _camera.aspect;
		} else {
			cameraWorldSize.x = size.y * _camera.aspect;
			cameraWorldSize.y = cameraWorldSize.x / _camera.aspect;
		}

		_camera.orthographicSize = cameraWorldSize.y / 2.0f;
	}

	void Update( )
	{
		FitCameraToBaseRectTransform( );
	}
}
