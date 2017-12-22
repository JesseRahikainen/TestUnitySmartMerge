using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

// Simple audio system. Useful for an initial pass and UI sounds. Doesn't handle location based audio.
public class Audio : MonoBehaviour {

    public class AudioHandle {
        private AudioSource _source;

        public AudioHandle( AudioSource source )
        {
            _source = source;
        }

        public float Pitch {
            get {
                return _source.pitch;
            }

            set {
                _source.pitch = value;
            }
        }

        public float Volume {
            get {
                return _source.volume;
            }

            set {
                _source.volume = value;
            }
        }
    }


	// just allow us to more easily play sounds, just use this instead of always having a clip and group
	[System.Serializable]
	public class AudioGroupClip {
		public AudioClip clip = null;
		public int groupID = 0;
		public bool loops = false;
        public float minPitch = 1.0f;
        public float maxPitch = 1.0f;
	}

	private static Audio _instance = null;
	public static Audio Instance {
		get { return _instance; }
	}
		
	public AudioMixer mixer;
	public MixerGroupSetup[] mixerGroupSetups;

    public class AudioClipResource {
        private AudioSource _source;

        public AudioClipResource( AudioSource source )
        {
            _source = source;
        }

        public float Pitch {
            get { return _source.pitch; }
            set { _source.pitch = value; }
        }
    }

	[System.Serializable]
	public class MixerGroupSetup {
		public int numSources = 10;
		public int numLoopingSources = 4;

		public AudioMixerGroup mixerGroup;

		private AudioSource[] _sources;
		private AudioSource[] _loopingSources;

        private AudioSource _defaultAudioSource;

		public void Init( GameObject gameObject )
		{
			_sources = new AudioSource[numSources];
			for( int i = 0; i < numSources; ++i ) {
				_sources[i] = gameObject.AddComponent<AudioSource>( );
				_sources[i].outputAudioMixerGroup = mixerGroup;
			}
			
			_loopingSources = new AudioSource[numLoopingSources];
			for( int i = 0; i < numLoopingSources; ++i ) {
				_loopingSources[i] = gameObject.AddComponent<AudioSource>( );
				_loopingSources[i].outputAudioMixerGroup = mixerGroup;
				_loopingSources[i].loop = true;
			}

            _defaultAudioSource = gameObject.AddComponent<AudioSource>( );
		}

		public AudioHandle PlayClip( AudioClip clip, bool loops, float pitch )
		{
			AudioSource[] srcs;
			if( loops ) {
				srcs = _loopingSources;
			} else {
				srcs = _sources;
			}

            AudioHandle handle = FindUnusedSourceAndPlayClip( srcs, clip, pitch );
			if( handle == null ) {
				Debug.LogWarning( "Unable to find source to play clip." );
			}
            return handle;
		}

		public void StopClip( AudioClip clip, bool loops )
		{
			if( loops ) {
				FindSourcesPlayingClipAndStop( _loopingSources, clip );
			} else {
				FindSourcesPlayingClipAndStop( _sources, clip );
			}
		}

		public bool IsClipPlaying( AudioClip clip, bool loops )
		{
			if( loops ) {
				return ( FindSourcePlayingClip( _loopingSources, clip ) != null );
			} else {
				return ( FindSourcePlayingClip( _sources, clip ) != null );
			}
		}

		public void StopAllClips( )
		{
			StopAllPlayingClips( _sources );
			StopAllPlayingClips( _loopingSources );
		}

		private void StopAllPlayingClips( AudioSource[] sources )
		{
			for( int i = 0; i < sources.Length; ++i ) {
				if( ( sources[i] != null ) && sources[i].isPlaying ) {
					sources[i].Stop( );
                    ResetSourceValues( sources[i] );
				}
			}
		}

		private AudioSource FindSourcePlayingClip( AudioSource[] sources, AudioClip clip )
		{
			for( int i = 0; i < sources.Length; ++i ) {
				if( ( sources[i] != null ) && sources[i].isPlaying && ( sources[i].clip == clip ) ) {
					return sources[i];
				}
			}

			return null;
		}

		private AudioHandle FindUnusedSourceAndPlayClip( AudioSource[] sources, AudioClip clip, float pitch )
		{
			AudioSource src = null;
			for( int i = 0; ( i < sources.Length ) && ( src == null ); ++i ) {
				if( !sources[i].isPlaying ) {
					src = sources[i];
					break;
				}
			}
			
			if( src == null ) {
				Debug.LogWarning( "Ran out of audio sources." );
				return null;
			}
			
			src.clip = clip;
            src.pitch = pitch;
			src.Play( );
			
			return new AudioHandle( src );
		}
		
		private void FindSourcesPlayingClipAndStop( AudioSource[] sources, AudioClip clip )
		{
			for( int i = 0; i < sources.Length; ++i ) {
				if( ( sources[i] != null ) && sources[i].isPlaying && ( sources[i].clip == clip ) ) {
					sources[i].Stop( );
                    ResetSourceValues( sources[i] );
				}
			}
		}

        private void ResetSourceValues( AudioSource source )
        {
            source.bypassEffects = _defaultAudioSource.bypassEffects;
            source.bypassListenerEffects = _defaultAudioSource.bypassListenerEffects;
            source.bypassReverbZones = _defaultAudioSource.bypassReverbZones;
            source.dopplerLevel = _defaultAudioSource.dopplerLevel;
            source.maxDistance = _defaultAudioSource.maxDistance;
            source.minDistance = _defaultAudioSource.minDistance;
            source.panStereo = _defaultAudioSource.panStereo;
            source.pitch = _defaultAudioSource.pitch;
            source.reverbZoneMix = _defaultAudioSource.reverbZoneMix;
            source.rolloffMode = _defaultAudioSource.rolloffMode;
            source.spatialBlend = _defaultAudioSource.spatialBlend;
            source.spatialize = _defaultAudioSource.spatialize;
            source.spread = _defaultAudioSource.spread;
            source.velocityUpdateMode = _defaultAudioSource.velocityUpdateMode;
            source.volume = _defaultAudioSource.volume;
        }
	}

	public string volumeParamName = "";

	private bool _mute = false;
	public bool Mute {
		get { return _mute; }
		set {
			_mute = value;
			if( _mute ) {
				PlayerPrefs.SetInt( "Mute", 1 );
				mixer.SetFloat( volumeParamName, -80.0f );
			} else {
				PlayerPrefs.SetInt( "Mute", 0 );
				mixer.SetFloat( volumeParamName, 0.0f );
			}
		}
	}

	// Use this for initialization
	void Start( )
	{
		if( ( _instance != null ) && ( _instance != this ) ) {
			Debug.Log( "Attempting to create a second instance of Audio, destroying." );
			Destroy( this );
			return;
		}

		_instance = this;

		Mute = ( PlayerPrefs.GetInt( "Mute", 0 ) == 1 );
		AdjustVolume( 0.5f );

		for( int i = 0; i < mixerGroupSetups.Length; ++i ) {
			mixerGroupSetups[i].Init( gameObject );
		}
	}

	void OnDisable( )
	{
		if( _instance == this ) _instance = null;
	}

	// the volumes should be given in normalized values [0,1]
	public void AdjustVolume( float volume )
	{
		volume = ( volume > 0.0f ) ? Mathf.Lerp( -20.0f, 20.0f, volume ) : -80.0f;
		mixer.SetFloat( volumeParamName, volume );
	}

	public bool IsGroupClipPlaying( AudioGroupClip groupClip )
	{
		if( groupClip.loops ) {
			return IsLoopingClipPlaying( groupClip.clip, groupClip.groupID );
		} else {
			return IsClipPlaying( groupClip.clip, groupClip.groupID );
		}
	}

	public bool IsClipPlaying( AudioClip clip, int mixerGroupID )
	{
		if( ( mixerGroupID < 0 ) || ( mixerGroupID >= mixerGroupSetups.Length ) ) {
			Debug.LogWarning( "Invalid mixer group ID." );
			return false;
		}

		if( clip == null ) {
			Debug.LogWarning( "Attempting to query null clip." );
			return false;
		}

		return mixerGroupSetups[mixerGroupID].IsClipPlaying( clip, false );
	}

	public bool IsLoopingClipPlaying( AudioClip clip, int mixerGroupID )
	{
		if( ( mixerGroupID < 0 ) || ( mixerGroupID >= mixerGroupSetups.Length ) ) {
			Debug.LogWarning( "Invalid mixer group ID." );
			return false;
		}

		if( clip == null ) {
			Debug.LogWarning( "Attempting to query null clip." );
			return false;
		}

		return mixerGroupSetups[mixerGroupID].IsClipPlaying( clip, true );
	}

	public AudioHandle PlayGroupClip( AudioGroupClip groupClip )
	{
        float pitch = Random.Range( groupClip.minPitch, groupClip.maxPitch );
		if( groupClip.loops ) {
			return PlayLoopingClip( groupClip.clip, groupClip.groupID, pitch );
		} else {
			return PlayClip( groupClip.clip, groupClip.groupID, pitch );
		}
	}

	public void StopGroupClip( AudioGroupClip groupClip )
	{
		if( groupClip.loops ) {
			StopLoopingClip( groupClip.clip, groupClip.groupID );
		} else {
			StopClip( groupClip.clip, groupClip.groupID );
		}
	}

	public AudioHandle PlayClip( AudioClip clip, int mixerGroupID, float pitch )
	{
		if( ( mixerGroupID < 0 ) || ( mixerGroupID >= mixerGroupSetups.Length ) ) {
			Debug.LogWarning( "Invalid mixer group ID." );
			return null;
		}

		if( clip == null ) {
			Debug.LogWarning( "Attempting to play null clip." );
			return null;
		}

		return mixerGroupSetups[mixerGroupID].PlayClip( clip, false, pitch );
	}

	public void StopClip( AudioClip clip, int mixerGroupID )
	{
		if( ( mixerGroupID < 0 ) || ( mixerGroupID >= mixerGroupSetups.Length ) ) {
			Debug.LogWarning( "Invalid mixer group ID." );
			return;
		}

		if( clip == null ) {
			Debug.LogWarning( "Attempting to stop null clip." );
			return;
		}

		mixerGroupSetups[mixerGroupID].StopClip( clip, false );
	}

	public AudioHandle PlayLoopingClip( AudioGroupClip groupClip )
	{
		return PlayLoopingClip( groupClip.clip, groupClip.groupID, Random.Range( groupClip.minPitch, groupClip.maxPitch ) );
	}
    
	public AudioHandle PlayLoopingClip( AudioClip clip, int mixerGroupID, float pitch )
	{
		if( ( mixerGroupID < 0 ) || ( mixerGroupID >= mixerGroupSetups.Length ) ) {
			Debug.LogWarning( "Invalid mixer group ID." );
			return null;
		}

		if( clip == null ) {
			Debug.LogWarning( "Attempting to play null clip." );
			return null;
		}

		return mixerGroupSetups[mixerGroupID].PlayClip( clip, true, pitch );
	}

	public void StopLoopingClip( AudioClip clip, int mixerGroupID )
	{
		if( ( mixerGroupID < 0 ) || ( mixerGroupID >= mixerGroupSetups.Length ) ) {
			Debug.LogWarning( "Invalid mixer group ID." );
			return;
		}

		if( clip == null ) {
			Debug.LogWarning( "Attempting to stop null clip." );
			return;
		}
			
		mixerGroupSetups[mixerGroupID].StopClip( clip, true );
	}

	public void StopAllClips( int mixerGroupID )
	{
		if( ( mixerGroupID < 0 ) || ( mixerGroupID >= mixerGroupSetups.Length ) ) {
			Debug.LogWarning( "Invalid mixer group ID." );
			return;
		}

		mixerGroupSetups[mixerGroupID].StopAllClips( );
	}
}
