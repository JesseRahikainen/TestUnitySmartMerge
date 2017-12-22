using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple message dispatcher. To be used primarily by the Achievements, since we don't know what we're going to get.
//  Can be used for anything though.
// This is easier to debug than the standard Unity message handling, and separates the sender and reciever so they
//  don't have to know about each other.
public class Dispatcher : MonoBehaviour {
    private static Dispatcher _instance;
    public static Dispatcher Instance {
        get { return _instance; }
    }
    
	void OnEnable( )
    {
		if( ( _instance != null ) && ( _instance != this ) ) {
            Debug.Log( "Attempting to create second Dispatcher, destroying new one." );
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

    public delegate void MessageHandler( int messageType, object data );

    private Dictionary<int, MessageHandler> _handlers = null;

    public void RegisterMessageHandler( int messageType, MessageHandler handler )
    {
        if( _handlers == null ) {
            _handlers = new Dictionary<int, MessageHandler>( );
        }

        if( !_handlers.ContainsKey( messageType ) ) {
            _handlers.Add( messageType, handler );
        } else {
            _handlers[messageType] += handler;
        }
    }

    public void DeRegisterMessageHandler( int messageType, MessageHandler handler )
    {
        if( ( _handlers != null ) && _handlers.ContainsKey( messageType ) ) {
            _handlers[messageType] -= handler;
        }
    }

    public void DispatchMessage( int messageType, object data = null )
    {
        if( ( _handlers != null ) && _handlers.ContainsKey( messageType ) && ( _handlers[messageType] != null ) ) {
            _handlers[messageType].Invoke( messageType, data );
        }
    }
}
