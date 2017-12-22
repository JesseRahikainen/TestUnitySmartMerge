using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class DebugText : MonoBehaviour {

	private static DebugText _instance;

	private Text _text;
	private CanvasGroup _canvasGroup;
	private bool _clearOnNewText;

	// Use this for initialization
	void OnEnable( )
	{
		_instance = this;
		StartCoroutine( "ClearText" );
		_clearOnNewText = false;
		_text = GetComponentInChildren<Text>( );
		if( _text == null ) {
			Debug.LogError( "Unable to find Text component for DebugText." );
			return;
		}
		_text.text = "";
		_canvasGroup = GetComponent<CanvasGroup>( );
	}

	void OnDisable( )
	{
		if( _instance == this ) {
			_instance = null;
		}
		StopCoroutine( "ClearText" );
	}

	void LateUpdate( )
	{
		if( string.IsNullOrEmpty( _text.text ) ) {
			_canvasGroup.alpha = 0.0f;
		} else {
			_canvasGroup.alpha = 1.0f;
		}
	}

	public static void Log( string txt )
	{
		Append( txt + '\n' );
	}

	public static void Append( string txt )
	{
		if( _instance == null ) return;
		if( _instance._text == null ) return;
		_instance.AppendText( txt );
	}

	private void AppendText( string txt )
	{
		if( _clearOnNewText ) {
			_text.text = "";
			_clearOnNewText = false;
		}

		_text.text += txt;
	}

	IEnumerator ClearText( )
	{
		while( true ) {
			yield return new WaitForEndOfFrame( );
			_clearOnNewText = true;
		}
	}
}
