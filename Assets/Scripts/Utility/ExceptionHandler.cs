using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to get support messages for exceptions and assertions. Requires the MessageDialogs are in
//  the project, along with the ConfirmationDialog. Create a child class to override GeneralSupportInfo
//  based on the project.
public class ExceptionHandler : MonoBehaviour {

    public string emailAddress = "jesse.rahikainen+support@zymo.io";
    public string title = "App Error";

    private void OnEnable( )
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable( )
    {
        Application.logMessageReceived -= HandleLog;
    }

    protected virtual string GeneralSupportInfo( )
    {
        return "";
    }

    public void HandleLog( string logString, string stackTrace, LogType type )
    {
        if( ( type == LogType.Exception ) || ( type == LogType.Assert ) ) {

            string msg = "There was an error. Please don't modify anything below.\n\n";
            msg += "General Info:\n" + GeneralSupportInfo( ) + "\n\n";
            msg += "Log: " + logString + "\n\n";
            msg += "Stack: " + stackTrace;

            MessageDialogs.ShowConfirmationDialog( "There was an error.",
                "Do you wish to send an e-mail with the error data?",
                "Yes", ( ) => {
                    Utils.OpenEMailClient( emailAddress, title, msg );
                },
                "No", null );
        }
    }
}
