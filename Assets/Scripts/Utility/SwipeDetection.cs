using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// Contains a list of swipe directions and what event should be triggered when they happen.
public class SwipeDetection : MonoBehaviour {

	[System.Serializable]
	public struct Swipe {
		public Vector2 direction;
		public UnityEvent onSwipe;
	}

	public float minDistance = 10.0f;
	public float maxDistance = 30.0f;
	public float maxTime = 0.5f;
	public float detectionToleranceDeg = 10.0f;

	private float _detectTolerance;

	public Swipe[] detectableSwipes;

	private class SwipeInput {
		public SwipeInput( int id )
		{
			_totalInput = Vector2.zero;
			_numInputs = 0;
			_timeAlive = 0.0f;
			_fingerID = id;
			_distTraveled = 0.0f;
		}

		public bool Update( Vector2 delta, float dt, float minDistance, float maxDistance, float maxTime )
		{
			++_numInputs;
			_totalInput += delta;
			_distTraveled += delta.magnitude;
			_timeAlive += dt;

			if( _distTraveled < minDistance ) {
				return false;
			}

			if( ( _distTraveled > maxDistance ) ||
				( _timeAlive >= maxTime ) ) {
				return true;
			}

			return false;
		}

		public bool IsValid( float minDistance )
		{
			if( _distTraveled < minDistance ) {
				return false;
			}

			return true;
		}

		public Vector2 AverageInput( )
		{
			Vector2 avg = ( _totalInput / (float)_numInputs ).normalized;
			return avg;
		}

		private Vector2 _totalInput;
		private float _distTraveled;

		private int _numInputs;
		private float _timeAlive;

		private int _fingerID;
		public int FingerID {
			get { return _fingerID; }
		}
	}

	private SwipeInput[] _inputDetection;

	// Use this for initialization
	void Start( )
	{
		if( !Input.touchSupported ) {
			enabled = false;
			return;
		}

		_detectTolerance = Mathf.Cos( detectionToleranceDeg );

		// make sure all the swipe vectors are normalized
		for( int i = 0; i < detectableSwipes.Length; ++i ) {
			detectableSwipes[i].direction.Normalize( );
		}

		_inputDetection = new SwipeInput[10]; // first guess, if we get more inputs just expand the array
		for( int i = 0; i < _inputDetection.Length; ++i ) {
			_inputDetection[i] = null;
		}
	}
	
	// Update is called once per frame
	void Update( )
	{
		for( int i = 0; i < Input.touchCount; ++i ) {
			switch( Input.touches[i].phase ) {
			case TouchPhase.Began:
				CreateNewSwipeInput( Input.touches[i] );
				break;
			case TouchPhase.Ended:
			case TouchPhase.Canceled:
				EndSwipeInput( Input.touches[i] );
				break;
			default:
				UpdateSwipeInput( Input.touches[i], Time.deltaTime );
				break;
			}
		}
	}

	private void CreateNewSwipeInput( Touch t )
	{
		int idx = FindNullInput( );
		if( idx < 0 ) {
			return;
		}
        
		_inputDetection[idx] = new SwipeInput( t.fingerId );
	}

	private void UpdateSwipeInput( Touch t, float dt )
	{
		// first find if the input actually exists
		int idx = FindInputWithFingerID( t.fingerId );
		if( idx < 0 ) {
			return;
		}

		if( _inputDetection[idx].Update( t.deltaPosition, dt, minDistance, maxDistance, maxTime ) ) {
			FinishSwipeInput( idx );
		}
	}

	private void EndSwipeInput( Touch t )
	{
		// first find if the input actually exists
		int idx = FindInputWithFingerID( t.fingerId );
		if( idx < 0 ) {
			return;
		}

		FinishSwipeInput( idx );
	}

	private void FinishSwipeInput( int idx )
	{
		if( _inputDetection[idx].IsValid( minDistance ) ) {
			Vector2 normalizedInput = _inputDetection[idx].AverageInput( );

			int best = -1;
			float bestScore = float.MaxValue;
			for( int i = 0; i < detectableSwipes.Length; ++i ) {
				float score = Vector2.Dot( detectableSwipes[i].direction, normalizedInput );
				if( ( score <= _detectTolerance ) && ( score < bestScore ) ) {
					best = i;
					bestScore = score;
				}
			}

			if( best >= 0 ) {
				detectableSwipes[best].onSwipe.Invoke( );
			}
		}

		// no longer detect this input
		_inputDetection[idx] = null;
	}

	private int FindInputWithFingerID( int fid )
	{
		for( int i = 0; i < _inputDetection.Length; ++i ) {
			if( ( _inputDetection[i] != null ) && ( _inputDetection[i].FingerID == fid ) ) {
				return i;
			}
		}

		return -1;
	}

	private int FindNullInput( )
	{
		for( int i = 0; i < _inputDetection.Length; ++i ) {
			if( _inputDetection[i] == null ) {
				return i;
			}
		}

		return -1;
	}
}
