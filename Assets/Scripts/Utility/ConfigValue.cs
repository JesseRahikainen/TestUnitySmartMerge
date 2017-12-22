using UnityEngine;
using System.Collections;

[System.Serializable]
public class ConfigValue {
	public int minimum;
	public int incrementAmt;
	public int numIncrements;
	public int currIncrement;
	public string prefName;
	public int defaultIncrements;

	[ReadOnly]
	public int maxDisp; // for display only, never use

	[ReadOnly]
	public int valDisp; // for display only, never use

	public int Maximum {
		get { return ( minimum + ( numIncrements * incrementAmt ) ); }
	}

	public int Value {
		get { return ( minimum + ( currIncrement * incrementAmt ) ); }
	}

	public float NormalizedValue {
		get {
			float curr = (float)Value;
			float max = (float)Maximum;
			float min = (float)minimum;

			return Mathf.InverseLerp( min, max, curr );
		}
	}
	
	public ConfigValue( int min, int incAmt, int numIncs, string name, int defaultIncs )
	{
		minimum = min;
		incrementAmt = incAmt;
		numIncrements = numIncs;
		prefName = name;
		currIncrement = defaultIncs;
		defaultIncrements = defaultIncs;

		maxDisp = Maximum;
		valDisp = Value;
	}
	
	public void Load( )
	{
		if( string.IsNullOrEmpty( prefName ) ) {
			Debug.LogWarning( "Unable to load configuration value, no player prefs name." );
			return;
		}

		currIncrement = PlayerPrefs.GetInt( prefName, defaultIncrements );
		//Debug.Log( "Loading " + prefName + ": " + currIncrement );
	}
	
	public void Save( )
	{
		if( string.IsNullOrEmpty( prefName ) ) {
			Debug.LogWarning( "Unable to save configuration value, no player prefs name." );
			return;
		}

		PlayerPrefs.SetInt( prefName, currIncrement );
		//Debug.Log( "Saving " + prefName + ": " + currIncrement + "    newValue: " + PlayerPrefs.GetInt( prefName ) );
	}

	public void IncreaseIncrement( )
	{
		++currIncrement;
		if( currIncrement > numIncrements ) {
			currIncrement = 0;
		}
	}

	// for use by the hud after something has changed the increment
	public void ReClampIncrement( )
	{
		currIncrement = Mathf.Clamp( currIncrement, 0, numIncrements );
		defaultIncrements = Mathf.Clamp( defaultIncrements, 0, numIncrements );
	}

	public void ValuesChanged( )
	{
		currIncrement = Mathf.Clamp( currIncrement, 0, numIncrements );
		defaultIncrements = Mathf.Clamp( defaultIncrements, 0, numIncrements );
		maxDisp = Maximum;
		valDisp = Value;
	}

	public void Reset( )
	{
		currIncrement = defaultIncrements;
	}
}