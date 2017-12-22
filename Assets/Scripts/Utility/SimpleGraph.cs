using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

[RequireComponent(typeof(LineRenderer))]
public class SimpleGraph : MonoBehaviour {

	public Color color;

	private float _currYMax = 0.0f;
	private float _currYMin = 0.0f;

	private float _currXMin = 0.0f;
	private float _currXMax = 0.0f;

	public int maxEntries = 2000;

	private LineRenderer _line;

	public Rect renderArea;

	//private List<Vector3> _points;

	public Text minYText;
	public Text maxYText;
	public Text minXText;
	public Text maxXText;

    private SimpleDataSet<float> _data;

    private float _smoothing = 0.0f;
    public float Smoothing {
        get { return _smoothing; }
        set {
            _smoothing = Mathf.Clamp01( value );
            AdjustPoints( );
        }
    }

    private float _trendSmoothing = 1.0f;
    public float TrendSmoothing {
        get { return _trendSmoothing; }
        set {
            _trendSmoothing = Mathf.Clamp01( value );
            AdjustPoints( );
        }
    }

    private bool _drawTrend = false;
    public bool DrawTrend {
        get { return _drawTrend; }
        set {
            _drawTrend = value;
            AdjustPoints( );
        }
    }
		
	void Start( )
	{
		_line = GetComponent<LineRenderer>( );
		_line.useWorldSpace = true;
		_line.SetColors( color, color );

		if( minYText != null ) minYText.color = color;
		if( maxYText != null ) maxYText.color = color;
		if( minXText != null ) minXText.color = color;
		if( maxXText != null ) maxXText.color = color;

        _data = new SimpleDataSet<float>( );

		//_points = new List<Vector3>( );

		ClearDataPoints( );
	}

	public void AddDataPoint( float time, float data )
	{
		SetNewYMax( data, false );
		SetNewYMin( data, false );
		SetNewXMax( time, false );
		SetNewXMin( time, false );

        _data.AddDataPoint( time, data );
        
		/*if( _points.Count > maxEntries ) {
			_points.RemoveRange( 0, _points.Count - maxEntries );
			float newMin = float.PositiveInfinity;
			for( int i = 0; i < _points.Count; ++i ) {
				if( _points[i].x < newMin ) {
					newMin = _points[i].x;
				}
			}
			ForceNewXMin( newMin );
		}*/

		AdjustPoints( );
	}

	private void AdjustPoints( )
	{
		if( _data.Count < 2 ) {
			return;
		}

        SimpleDataSet<Vector3> newDataSet = _data.GenerateNewDataSet<Vector3>(
            ( f, i ) => {
                return new Vector3( 0.0f, f, 0.0f );
            } );

        float prevY = 0.0f;
        float prevZ = 0.0f;
        newDataSet.RunOverAllData( ( d, t, i ) => {
            if( i == 0 ) {
                d.z = d.y;
            } else {
                d.y = Mathf.Lerp( d.y, prevY, _smoothing );
                d.z = Mathf.Lerp( d.y, prevZ, _smoothing );
            }
            prevY = d.y;
            prevZ = d.z;
            d.x = t;
            return d;
        } );






        SimpleDataSet<Vector3>.SimpleDataEntry[] data = newDataSet.GetLastEntries( maxEntries );
        Vector3[] adjPoints = new Vector3[data.Length];

        for( int i = 0; i < adjPoints.Length; ++i ) {
            adjPoints[i] = data[i].Data;
            if( _drawTrend ) {
                adjPoints[i].y = adjPoints[i].z;
            }
            adjPoints[i].z = 0.0f;
        }

        _currXMax = float.NegativeInfinity;
        _currYMin = float.PositiveInfinity;
        _currYMax = float.NegativeInfinity;
        _currYMin = float.PositiveInfinity;
        for( int i = 0; i < data.Length; ++i ) {
            if( _currXMax < adjPoints[i].x ) _currXMax = adjPoints[i].x;
            if( _currXMin > adjPoints[i].x ) _currXMin = adjPoints[i].x;
            if( _currYMax < adjPoints[i].y ) _currYMax = adjPoints[i].y;
            if( _currYMin > adjPoints[i].y ) _currYMin = adjPoints[i].y;
        }

        // create display
		for( int i = 0; i < adjPoints.Length; ++i ) {
			adjPoints[i].x = Mathf.Lerp( renderArea.xMin, renderArea.xMax, Mathf.InverseLerp( _currXMin, _currXMax, adjPoints[i].x ) );
			adjPoints[i].y = Mathf.Lerp( renderArea.yMin, renderArea.yMax, Mathf.InverseLerp( _currYMin, _currYMax, adjPoints[i].y ) );
            adjPoints[i].z = 0.0f;
		}
        
        _line.positionCount = adjPoints.Length;
		_line.SetPositions( adjPoints );
	}

	public void ClearDataPoints( )
	{
		_data.Clear( );
		_currYMin = float.PositiveInfinity;
		_currYMax = float.NegativeInfinity;
		_currXMin = float.PositiveInfinity;
		_currXMax = float.NegativeInfinity;
		AdjustPoints( );
	}

	public void SetNewYMax( float newMax, bool reposition = true )
	{
		if( newMax <= _currYMax ) return;
		_currYMax = newMax;
		if( maxYText != null ) maxYText.text = _currYMax.ToString( );
		if( reposition ) AdjustPoints( );
	}

	public void SetNewYMin( float newMin, bool reposition = true )
	{
		if( newMin >= _currYMin ) return;
		_currYMin = newMin;
		if( minYText != null ) minYText.text = _currYMin.ToString( );
		if( reposition ) AdjustPoints( );
	}

	public void SetNewXMax( float newMax, bool reposition = true )
	{
		if( newMax <= _currXMax ) return;
		_currXMax = newMax;
		if( maxXText != null ) maxXText.text = _currXMax.ToString( );
		if( reposition ) AdjustPoints( );
	}

	public void SetNewXMin( float newMin, bool reposition = true )
	{
		if( newMin >= _currXMin ) return;
		_currXMin = newMin;
		if( minXText != null ) minXText.text = _currXMin.ToString( );
		if( reposition ) AdjustPoints( );
	}

	private void ForceNewXMin( float newMin )
	{
		_currXMin = newMin;
		if( minXText != null ) minXText.text = _currXMin.ToString( );
	}

	public float GetXMin( )
	{
		return _currXMin;
	}

	public float GetXMax( )
	{
		return _currXMax;
	}

	public float QueryValue( float time )
	{
        SimpleDataSet<float>.SimpleDataEntry before = _data.GetBefore( time );
        SimpleDataSet<float>.SimpleDataEntry after = _data.GetAfter( time );

        if( ( before != null ) && ( after != null ) ) {
		    float t = Mathf.InverseLerp( before.Time, after.Time, time );
		    float result = Mathf.Lerp( before.Data, after.Data, t );

		    return result;
        }

        return float.PositiveInfinity;
	}

	void OnDrawGizmos( )
	{
		Utils.DrawGizmoBox( renderArea, Color.green );
	}

    public void DumpNewData( string fileName )
    {
        DumpData( fileName, false );
    }

    public void DumpAppendData( string fileName )
    {
        DumpData( fileName, true );
    }

    private string SerializeEntry( float time, float obj )
    {
        string str = time + "," + obj + "\n";
        return str;
    }
    private string StoragePath {
        get { return Application.persistentDataPath + "/"; }
    }
    public void DumpData( string fileName, bool append )
    {
        string path = StoragePath + fileName + ".dat";

        Debug.Log( "Attempting to create directory: " + StoragePath );
        DirectoryInfo dataDir = new DirectoryInfo( StoragePath );
        dataDir.Create( );

        using( StreamWriter sw = new StreamWriter( path, append ) ) {
            Debug.Log( "Opened file: " + path );
            try {
                sw.WriteLine( "===" ); // new sample separator
                sw.Write( _data.SerializeData( SerializeEntry ) );
		    } catch( Exception e ) {
			    Debug.LogError( "problem: " + e );
		    }
            Debug.Log( "Done writing: " + path );
        }//*/
    }

    public void SetData( SimpleDataSet<float> data )
    {
        _data = data;
        AdjustPoints( );
    }
}
