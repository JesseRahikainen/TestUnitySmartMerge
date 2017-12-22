using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

// Stores a collection of object pools. Can create objects and store them for later use.
public class ObjectPoolManager : MonoBehaviour
{
	[System.Serializable]
	public class PreInitEntry
	{
		public GameObject obj;
		public int count;
	};
	
	static Dictionary< GameObject, LinkedList<GameObject> > activePools;
	static Dictionary< GameObject, LinkedList<GameObject> > inactivePools;
	static bool leakCheck = false;
	static uint idNum = 0;
	
	// Used to pre-initialize lists for certain objects, should be hidden behind a loading screen since
	//  this will be quite expensive.
	public List<PreInitEntry> preInit;
	
	// Use this for initialization
	void Start( )
	{
		activePools=new Dictionary< GameObject, LinkedList<GameObject> >();
		inactivePools=new Dictionary< GameObject, LinkedList<GameObject> >();

		foreach( PreInitEntry pie in preInit ) {
			if( ( pie.obj != null ) && ( pie.count > 0 ) ) {
				InitializePool( pie.obj, pie.count );
			}
		}
	}
	
#if UNITY_EDITOR
	void Update( )
	{
		if( Input.GetKeyDown( KeyCode.O ) ) {
			LeakCheck();
		}
	}
#endif
	
	// Get an instantiation of the type prefab. Pull it from the inactive pool if one exists
	//  if it doesn't than create one and use that.
	public static GameObject CreateObject( GameObject prefab )
	{
		if( prefab == null ) {
			Debug.LogError("Prefab is null! WTF! Don't create null objects!");
			return null;
		}
		
		if( ( activePools == null ) || ( inactivePools == null ) ) {
			Debug.LogError( "Pool dictionaries not created!" );
			return null;
		}
		
		CheckAndCreatePools( prefab );
		
		LinkedList<GameObject> activePool = activePools[prefab];
		LinkedList<GameObject> inactivePool = inactivePools[prefab];
		LinkedListNode<GameObject> objectNode;
		
		if( inactivePool.Count <= 0 ) {
			objectNode=CreatePoolObject(prefab);
		} else {
			objectNode = inactivePool.First;
			inactivePool.RemoveFirst( );
		}
		
		activePool.AddFirst( objectNode );
		objectNode.Value.SetActive( true );
		objectNode.Value.BroadcastMessage( "PoolAwake", SendMessageOptions.DontRequireReceiver );
		objectNode.Value.BroadcastMessage( "PoolStart", SendMessageOptions.DontRequireReceiver );
		
		if( leakCheck ) {
			LeakCheck();
        }
		
		return objectNode.Value;
	}
	
	// Object was destroyed, remove it from it's active pool and put it in the inactive pool
	//  for later use.
	public static void ObjectDestroyed(GameObject obj)
	{
		if( ( activePools == null ) || ( inactivePools == null ) ) {
			Debug.LogError( "Pool dictionaries not created!" );
			return;
		}
		
		ManagedPoolObject mpo = obj.GetComponent<ManagedPoolObject>( );
	
		if( mpo == null ) {
			Debug.LogError( "Attempting to move unmanaged object to an inactive pool! Destroying object! " + obj );
			Destroy( obj );
			return;
		}
		
		if( !CheckPools( mpo.managedPrefab ) ) {
			Debug.LogError( "Attempting to move an object to an inactive pool that doesn't exist! Destorying object! " + obj );
			Destroy( obj );
			return;
		}
		
		LinkedList<GameObject> activePool = activePools[mpo.managedPrefab];
		LinkedList<GameObject> inactivePool = inactivePools[mpo.managedPrefab];
		LinkedListNode<GameObject> objectNode;
		
		objectNode = activePool.Find( obj );
		if( objectNode == null ) {
			Debug.LogError( "Attempting to move an object that isn't in an active pool to an inactive pool! " + obj );
			return;
		}
		
		// do the deactivation of the object
		obj.transform.parent = null;
		obj.BroadcastMessage( "PoolDestroy", SendMessageOptions.DontRequireReceiver );
		obj.SetActive( false );
		
		activePool.Remove( objectNode );
		inactivePool.AddFirst( objectNode );
		
		if( leakCheck ) {
			LeakCheck( );
        }
	}
	
	static bool CheckPools( GameObject prefab )
	{
		return ( activePools.ContainsKey( prefab ) && inactivePools.ContainsKey( prefab ) );
	}
	
	// A pool of a specific type has been requested, make sure it exists
	static void CheckAndCreatePools( GameObject prefab )
	{
		if( !activePools.ContainsKey( prefab ) ) {
			activePools.Add( prefab, new LinkedList<GameObject>( ) );
		}
		
		if( !inactivePools.ContainsKey( prefab ) ) {
			inactivePools.Add( prefab, new LinkedList<GameObject>( ) );
		}
	}
	
	// Initialize a certain number of objects into a pool, if this isn't done the pool will be created dynamically
	//  when an object is requested
	// If the pool already exists then increase it's size if it's less than what's passed in
	static void InitializePool( GameObject prefab, int size )
	{
		CheckAndCreatePools( prefab );
		
		LinkedList<GameObject> activePool = activePools[prefab];
		LinkedList<GameObject> inactivePool = inactivePools[prefab];
		LinkedListNode<GameObject> newObject;
		
		while( ( inactivePool.Count + activePool.Count ) < size ) {
			newObject = CreatePoolObject( prefab );
			newObject.Value.SetActive( false );
			inactivePool.AddFirst( newObject );
		}
	}
	
	// Create a LinkedListNode containing an instance of prefab as the value.
	static LinkedListNode<GameObject> CreatePoolObject(GameObject prefab)
	{
		//Debug.Log ("***** Creating pool object: "+prefab);
		GameObject newObj = (GameObject)Instantiate( (UnityEngine.Object)prefab );
		LinkedListNode<GameObject> objectNode = new LinkedListNode<GameObject>( newObj );
		newObj.name += idNum;
		++idNum;
		
		// make sure the new object doesn't have the managed component
		if( newObj.GetComponent<ManagedPoolObject>( ) != null ) {
			Debug.LogWarning( "Creating object from prefab that already has the ManagedPoolObject component. Please remove it. " + prefab );
			Destroy( newObj.GetComponent<ManagedPoolObject>( ) );
		}
		ManagedPoolObject mpo = newObj.AddComponent<ManagedPoolObject>( );
		mpo.managedPrefab = prefab;
		
		return objectNode;
	}
	
	static void LeakCheck()
	{
		int count;
		// go through all the lists and see if any of them have a node set to null (which should never happen)
		LinkedListNode<GameObject> node;
		
		Debug.Log( "Object Pool Manager Check" );
		
		Debug.Log( " Checking active pools:" );
		foreach( GameObject go in activePools.Keys ) {
			if( !inactivePools.ContainsKey( go ) ) {
				Debug.Log( "  No inactive pools exists for active pool with key " + go + "!" );
            }
			
			count = 0;
			node = activePools[go].First;
			while( node != null ) {
				if( node.Value == null ) {
					++count;
				}
				node = node.Next;
			}
			
			Debug.Log( "  Active pool " + go + " -  total size: " + activePools[go].Count + "   null objects: " + count );
		}
		
		foreach( GameObject go in inactivePools.Keys ) {
			if( !activePools.ContainsKey( go ) ) Debug.Log("  No active pools exists for inactive pool with key "+go+"!");
			
			count = 0;
			node = inactivePools[go].First;
			while( node != null ) {
				if( node.Value == null ) {
					++count;
				}
				node = node.Next;
			}
			
			Debug.Log( "  Inactive pool " + go + " -  total size: " + inactivePools[go].Count + "   null objects: " + count );
		}
		
		// dump data to a text csv file
		string fileName = Directory.GetParent( Directory.GetParent( Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly( ).Location ) ).FullName).FullName;
		fileName += "/data_dump.csv";
		
		if( File.Exists( fileName ) ) {
			File.Delete( fileName );
		}
		
		FileStream fs = File.OpenWrite( fileName );
		foreach( GameObject go in activePools.Keys ) {
			// count stuff in active pools
			count = 0;
			node = activePools[go].First;
			while( node != null ) {
				if( node.Value != null ) {
					++count;
				}
				node = node.Next;
			}
			
			// count stuff in inactive pools
			if( inactivePools.ContainsKey( go ) ) {
				node = inactivePools[go].First;
				while( node != null ) {
					if( node.Value != null ) {
						++count;
					}
					node = node.Next;
				}
			}
			
			// dump stuff out to file now
			String countString = go.ToString( ).Remove( go.ToString( ).IndexOf( ' ' ) ) + " " + count + Environment.NewLine;
			byte[] byteDump = new UTF8Encoding( true ).GetBytes( countString );
			fs.Write( byteDump, 0, byteDump.Length );
		}
		
		fs.Close( );
	}
}
