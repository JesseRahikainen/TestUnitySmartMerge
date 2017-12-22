using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Just, like, store some data, no real design behind this, just adding stuff as it's needed
public class SimpleDataSet<T> {

    public class SimpleDataEntry {
        private float _time;
        public float Time {
            get { return _time; }
        }

        private T _data;
        public T Data {
            get { return _data; }
            set { _data = value; }
        }

        public SimpleDataEntry( float time, T data )
        {
            _time = time;
            _data = data;
        }
    }

    private List<SimpleDataEntry> _dataList;

    public SimpleDataSet( )
    {
        _dataList = new List<SimpleDataEntry>( );
    }

    public void AddDataPoint( float time, T data )
    {
        _dataList.Add( new SimpleDataEntry( time, data ) );

        // sort by time
        _dataList.Sort( ( SimpleDataEntry lhs, SimpleDataEntry rhs ) => {
            if( lhs.Time < rhs.Time ) return -1;
            else if( lhs.Time > rhs.Time ) return 1;
            return 0;
        } );
    }

    public SimpleDataEntry[] GetLastEntries( int count )
    {
        int len = Mathf.Min( count, _dataList.Count );
        int start = _dataList.Count - len;
        SimpleDataEntry[] data = null;
        if( len > 0 ) {
            data = _dataList.GetRange( start, len ).ToArray( );
        }
        return data;
    }

    public SimpleDataEntry GetBefore( float time )
    {
        SimpleDataEntry data = null;

        for( int i = 0; i < _dataList.Count; ++i ) {
            if( _dataList[i].Time < time ) {
                data = _dataList[i];
            } else {
                return data;
            }
        }

        return data;
    }

    public SimpleDataEntry GetAfter( float time )
    {
        SimpleDataEntry data = null;

        for( int i = 0; i < _dataList.Count; ++i ) {
            if( _dataList[i].Time >= time ) {
                return data;
            } else {
                data = _dataList[i];
            }
        }

        return data;
    }

    public delegate string Serialize( float time, T obj );
    public delegate bool Deserialize( string str, out float time, out T obj );

    public string SerializeData( Serialize serialize )
    {
        if( serialize == null ) {
            throw new System.ArgumentNullException( );
        }

        string data = "";
        
        foreach( SimpleDataEntry sde in _dataList ) {
            data += serialize( sde.Time, sde.Data );
        }

        return data;
    }

    public void LoadData( string dataBlock, Deserialize deserialize )
    {
        if( deserialize == null ) {
            throw new System.ArgumentNullException( );
        }

        float time;
        T obj;

        string[] lines = dataBlock.Split( new char[] { '\n' } );

        foreach( string str in lines ) {
            if( deserialize( str, out time, out obj ) ) {
                _dataList.Add( new SimpleDataEntry( time, obj ) );
            }
        }
    }

    public void Clear( )
    {
        _dataList.Clear( );
    }

    public int Count {
        get { return _dataList.Count; }
    }

    public delegate T DataFunction( T data, float time, int idx );
    public void RunOverAllData( DataFunction func )
    {
        for( int i = 0; i < _dataList.Count; ++i ) {
            _dataList[i].Data = func( _dataList[i].Data, _dataList[i].Time, i );
        }
    }

    public delegate N ConvertDataFunction<N>( T data, int idx );
    public SimpleDataSet<N> GenerateNewDataSet<N>( ConvertDataFunction<N> func )
    {
        SimpleDataSet<N> newSDS = new SimpleDataSet<N>( );

        for( int i = 0; i < _dataList.Count; ++i ) {
            N newData = func( _dataList[i].Data, i );
            newSDS.AddDataPoint( _dataList[i].Time, newData );
        }

        return newSDS;
    }
}
