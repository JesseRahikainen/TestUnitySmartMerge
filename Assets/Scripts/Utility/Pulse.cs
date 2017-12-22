using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour {

    public AnimationCurve pulseCurve;

    private float _timePassed;
    private float _timeScale = 1.0f;

    private bool _pulsing;

    // Use this for initialization
    private void OnEnable( )
    {
        _pulsing = false;
        _timePassed = 0.0f;
    }	

	// Update is called once per frame
	void Update( )
    {
        if( !_pulsing ) return;

		_timePassed += Time.deltaTime * _timeScale;

        float s = 1.0f;
        if( _timePassed >= pulseCurve[pulseCurve.length-1].time ) {
            _pulsing = false;
        } else {
            s = pulseCurve.Evaluate( _timePassed );
        }
        transform.localScale = new Vector3( s, s, s );
	}

    public void StartPulse( float scale = 1.0f )
    {
        _timeScale = scale;
        _pulsing = true;
        _timePassed = 0.0f;
    }
}
