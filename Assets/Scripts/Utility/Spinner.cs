using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// often need something that spins, for loading or just to let us know that stuff is actually running and hasn't locked
//  up when creating test scenes
public class Spinner : MonoBehaviour {

	public Vector3 speed = new Vector3( 0.0f, 0.0f, 15.0f );
	
	void Update( )
	{
		transform.Rotate( speed * Time.deltaTime );
	}
}
