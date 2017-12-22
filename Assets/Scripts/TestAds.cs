using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class TestAds : MonoBehaviour {
    public void ShowAd( )
    {
        if( !Advertisement.isSupported ) {
            Debug.Log( "Advertisements not supported" );
            return;
        }

        if( !Advertisement.testMode ) {
            Debug.Log( "Test mode is not activated." );
            return;
        }

        if( Advertisement.isShowing ) {
            Debug.Log( "Already showing an ad." );
            return;
        }

        ShowOptions options = new ShowOptions( );
        options.resultCallback = HandleAdResult;
        if( Advertisement.IsReady( ) ) {
            Advertisement.Show( "rewardedVideo", options );
        } else {
            Debug.Log( "No advertisement ready." );
        }
    }

    void HandleAdResult( ShowResult result )
    {
        if( result == ShowResult.Finished ) {
            Debug.Log( "Ad finished" );
        } else if( result == ShowResult.Skipped ) {
            Debug.Log( "Ad skipped" );
        } else if( result == ShowResult.Failed ) {
            Debug.Log( "Ad failed" );
        } else {
            Debug.LogError( "Something went really wrong!" );
        }
    }
}
