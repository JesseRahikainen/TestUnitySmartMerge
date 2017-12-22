using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MessageDialogs : MonoBehaviour {

    public ConfirmDialog confirmDialog;
    public OkayDialog okayDialog;

    private static MessageDialogs _instance = null;
    public static MessageDialogs Instance {
        get { return _instance; }
    }

	// Use this for initialization
	void OnEnable( )
    {
		if( ( _instance != null ) && ( _instance != this ) ) {
            Debug.LogError( "Attempting to create a second MessageDialogs instance, destroying new one." );
            Destroy( this );
            return;
        }

        _instance = this;
	}

    private void OnDisable( )
    {
        if( _instance == this ) {
            _instance = null;
        }
    }

    public static void ShowConfirmationDialog(
        string question, string title = "",
        string confirmBtn = "Yes", Action confirmAction = null,
        string cancelBtn = "No", Action cancelAction = null )
    {
        if( _instance == null ) {
            Debug.LogError( "MessageDialogs is null." );
            return;
        }

        if( _instance.confirmDialog == null ) {
            Debug.LogError( "ConfirmDialog is null." );
            return;
        }

        _instance.confirmDialog.Show( title, question, confirmBtn, confirmAction, cancelBtn, cancelAction );
    }

    public static void ShowMessageDialog(
        string notice, string title = "", string okBtn = "OK", Action okAction = null )
    {
        if( _instance == null ) {
            Debug.LogError( "MessageDialogs is null." );
            return;
        }

        if( _instance.okayDialog == null ) {
            Debug.LogError( "OkayDialog is null." );
            return;
        }
        
        _instance.okayDialog.Show( title, notice, okBtn, okAction );
    }
}
