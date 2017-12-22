using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class VersionText : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Text t = GetComponent<Text>( );
        t.text = Application.version;
	}
}
