using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnPlay : MonoBehaviour {

    public bool hideWhenFromEditor;

    private void OnEnable( )
    {
#if UNITY_EDITOR
        if( !hideWhenFromEditor ) {
            return;
        }
#endif
        gameObject.GetComponent<MeshRenderer>( ).enabled = false;
    }
}
