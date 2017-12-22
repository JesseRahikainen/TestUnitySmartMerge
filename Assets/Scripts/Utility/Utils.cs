using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Linq;

// General helper code that isn't substantial enough to require it's own class or smaller classes. Also extensions to existing classes.
public class Utils {

    // pulled from here: https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
    public static class StringCipher
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            var rngCsp = new RNGCryptoServiceProvider();
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }

    private static string _data = "V7K6ilUqSRsAlG3fcxBtjX5G0LjFzY4yVf7xzzreTMY9YZvoxFlIFmagdEV4";

	static public Rect GetWorldRect( RectTransform rt )
	{
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners( corners );

		float left = float.MaxValue;
		float right = float.MinValue;
		float top = float.MinValue;
		float bottom = float.MaxValue;

		for( int i = 0; i < corners.Length; ++i ) {
			if( corners[i].x < left ) left = corners[i].x;
			if( corners[i].x > right ) right = corners[i].x;
			if( corners[i].y > top ) top = corners[i].y;
			if( corners[i].y < bottom ) bottom = corners[i].y;
		}

		Rect rect = new Rect( left, bottom, ( right - left ), ( top - bottom ) );
		return rect;
	}

	static public Vector2 GetWorldRectTransformSize( RectTransform rt )
	{
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners( corners );

		float left = float.MaxValue;
		float right = float.MinValue;
		float top = float.MinValue;
		float bottom = float.MaxValue;

		for( int i = 0; i < corners.Length; ++i ) {
			if( corners[i].x < left ) left = corners[i].x;
			if( corners[i].x > right ) right = corners[i].x;
			if( corners[i].y > top ) top = corners[i].y;
			if( corners[i].y < bottom ) bottom = corners[i].y;
		}

		Vector2 size = new Vector2(
			Mathf.Abs( left - right ),
			Mathf.Abs( top - bottom ) );

		return size;
	}

    static public Rect GetLocalRect( RectTransform rt )
    {
        Vector3[] corners = new Vector3[4];
		rt.GetLocalCorners( corners );

		float left = float.MaxValue;
		float right = float.MinValue;
		float top = float.MinValue;
		float bottom = float.MaxValue;

		for( int i = 0; i < corners.Length; ++i ) {
			if( corners[i].x < left ) left = corners[i].x;
			if( corners[i].x > right ) right = corners[i].x;
			if( corners[i].y > top ) top = corners[i].y;
			if( corners[i].y < bottom ) bottom = corners[i].y;
		}

		Rect rect = new Rect( left, bottom, ( right - left ), ( top - bottom ) );
		return rect;
    }

    static public Vector2 GetLocalRectTransformSize( RectTransform rt )
    {
        Vector3[] corners = new Vector3[4];
        rt.GetLocalCorners( corners );

        float left = float.MaxValue;
		float right = float.MinValue;
		float top = float.MinValue;
		float bottom = float.MaxValue;

		for( int i = 0; i < corners.Length; ++i ) {
			if( corners[i].x < left ) left = corners[i].x;
			if( corners[i].x > right ) right = corners[i].x;
			if( corners[i].y > top ) top = corners[i].y;
			if( corners[i].y < bottom ) bottom = corners[i].y;
		}

		Vector2 size = new Vector2(
			Mathf.Abs( left - right ),
			Mathf.Abs( top - bottom ) );

		return size;
    }

	static public float WrapFloat( float min, float max, float value )
	{
		if( min > max ) {
			float temp = min;
			min = max;
			max = temp;
		}

		float diff = max - min;

		if( Mathf.Approximately( Mathf.Abs( diff ), 0.001f ) ) {
			return min;
		}

		while( value > max ) {
			value -= diff;
		}
		while( value < min ) {
			value += diff;
		}

		return value;
	}

	static public float ClampWithinVariance( float center, float variance, float value )
	{
		float min = center - variance;
		float max = center + variance;

		if( min > max ) {
			float temp = min;
			min = max;
			max = temp;
		}

		return Mathf.Clamp( value, min, max );
	}

	static public bool TryGetCircleCenter( Vector2 p1, Vector2 p2, Vector2 p3, out Vector2 center )
	{
		// given three points on the circle we can get two chords, find their mid-points and
		//  vectors perpendicular from them
		// from these four points we can form two lines, one for each chord mid point, where
		//  these two lines intersect is the center of the circle defined by the three points
		// just have to make sure none of the points are equal
		Vector2 chordAMidPoint = Vector2.Lerp( p1, p2, 0.5f );
		Vector2 chordADir = p1 - p2;
		Vector2 chordAPerp = new Vector2( -chordADir.y, chordADir.x );

		Vector2 chordBMidPoint = Vector2.Lerp( p2, p3, 0.5f );
		Vector2 chordBDir = p2 - p3;
		Vector2 chordBPerp = new Vector2( -chordBDir.y, chordBDir.x );

		return TryLineLineIntersection( chordAMidPoint, ( chordAMidPoint + chordAPerp),
			chordBMidPoint, ( chordBMidPoint + chordBPerp ), out center );
	}

	static public bool TryLineLineIntersection( Vector2 p1, Vector2 p2,
		Vector2 p3, Vector2 p4, out Vector2 intersectionPoint )
	{
		intersectionPoint = Vector2.zero;

		float denom = ( p1.x - p2.x ) * ( p3.y - p4.y ) - ( p1.y - p2.y ) * ( p3.x - p4.x );
		if( Mathf.Approximately( denom, 0.0f ) ) {
			return false;
		}

		float numX = ( p1.x * p2.y - p1.y * p2.x ) * ( p3.x - p4.x ) - ( p1.x - p2.x ) * ( p3.x * p4.y - p3.y * p4.x );
		float numY = ( p1.x * p2.y - p1.y * p2.x ) * ( p3.y - p4.y ) - ( p1.y - p2.y ) * ( p3.x * p4.y - p3.y * p4.x );

		intersectionPoint.x = numX / denom;
		intersectionPoint.y = numY / denom;

		return true;
	}

	// draws a 2d circle on the xy plane
	public static void DrawGizmoCircle( Vector3 center, float radius, Color clr, int resolution )
	{
		Vector3 from, to;
		float angleStep = ( Mathf.PI * 2.0f ) / (float)resolution;

		Gizmos.color = clr;

		from = center + new Vector3( 0.0f, radius );
		for( int i = 0; i <= resolution; ++i ) {
			float angle = angleStep * (float)i;
			to = center + new Vector3( Mathf.Sin( angle ) * radius, Mathf.Cos( angle ) * radius );
			Gizmos.DrawLine( from, to );
			from = to;
		}
	}

	// draws a 2d AAB on the xy plane
	public static void DrawGizmoAAB( Rect rect, Color clr )
	{
		Gizmos.color = clr;

		Vector3 topLeft = new Vector3( rect.xMin, rect.yMin );
		Vector3 topRight = new Vector3( rect.xMax, rect.yMin );
		Vector3 bottomLeft = new Vector3( rect.xMin, rect.yMax );
		Vector3 bottomRight = new Vector3( rect.xMax, rect.yMax );

		Gizmos.DrawLine( topLeft, topRight );
		Gizmos.DrawLine( topRight, bottomRight );
		Gizmos.DrawLine( bottomRight, bottomLeft );
		Gizmos.DrawLine( bottomLeft, topLeft );
	}

	// draw a 2d arrow on the xy plane
	public static void DrawGizmoArrow( Vector3 from, Vector3 to, Color clr )
	{
		Gizmos.color = clr;

		Gizmos.DrawLine( from, to );

		Vector3 diff = from - to;
		diff *= 0.1f;

		Vector3 arrowBranchOne = new Vector3( );
		Vector3 arrowBranchTwo = new Vector3( );
		float c = 0.9659f;
		float s = 0.2588f;
		arrowBranchOne.x = ( diff.x * c ) - ( diff.y * s );
		arrowBranchOne.y = ( diff.x * s ) + ( diff.y * c );

		s = -s;
		arrowBranchTwo.x = ( diff.x * c ) - ( diff.y * s );
		arrowBranchTwo.y = ( diff.x * s ) + ( diff.y * c );

		//Vector3 arrowEnd = 
		Gizmos.DrawLine( to, to + arrowBranchOne );
		Gizmos.DrawLine( to, to + arrowBranchTwo );
	}

	// draw a 2d box on the xy plane
	public static void DrawGizmoBox( Rect r, Color clr )
	{
		Gizmos.color = clr;

		Vector3 from = new Vector3( r.xMin, r.yMin, 0.0f );
		Vector3 to = new Vector3( r.xMin, r.yMax, 0.0f );
		Gizmos.DrawLine( from, to );

		from = to;
		to.x = r.xMax;
		Gizmos.DrawLine( from, to );

		from = to;
		to.y = r.yMin;
		Gizmos.DrawLine( from, to );

		from = to;
		to.x = r.xMin;
		Gizmos.DrawLine( from, to );
	}

	// smooth blend
	static public float HermiteBlend( float t )
	{
		t = Mathf.Clamp01( t );
		return ( ( 3.0f * ( t * t ) ) - ( 2.0f * ( t * t * t ) ) );
	}

	// smoother blend (1st and 2nd derivatives have a slope of 0)
	//  gotten from a paper about simplex noise, hence the name
	static public float PerlinBlend( float t )
	{
		t = Mathf.Clamp01( t );
		return ( ( 6.0f * ( t * t * t * t * t ) ) - ( 15.0f * ( t * t * t * t ) ) + ( 10.0f * ( t * t * t ) ) );
	}

	public const float TWO_PI = 2.0f * Mathf.PI;

    static public bool TriangleIsCCW( Vector3 p1, Vector3 p2, Vector3 p3, Vector3 forward )
    {
        Vector3 p12 = p1 - p2;
        Vector3 p13 = p1 - p3;
        Vector3 x = Vector3.Cross( p12, p13 );
        return ( Vector3.Dot( x, forward ) > 0 );
    }

    public static float TriArea2D( float x1, float y1, float x2, float y2, float x3, float y3 )
    {
        return ( ( x1 - x2 ) * ( y2 - y3 ) - ( x2 - x3 ) * ( y1 - y2 ) );
    }

    // Compute the barycentric coordinates (u,v,w) for point p in triangle (a,b,c)
    static public void Barycentric( Vector3 a, Vector3 b, Vector3 c, Vector3 p, out float u, out float v, out float w )
    {
        Vector3 m = Vector3.Cross( b - a, c - a );

        float nu, nv, ood;

        float x = Mathf.Abs( m.x );
        float y = Mathf.Abs( m.y );
        float z = Mathf.Abs( m.z );

        if( ( x >= y ) && ( x >= z ) ) {
            // x largest, project onto yz plane
            nu = TriArea2D( p.y, p.z, b.y, b.z, c.y, c.z ); // area of PBC in yz plane
            nv = TriArea2D( p.y, p.z, c.y, c.z, a.y, a.z ); // area of PCA in yz plane
            ood = 1.0f / m.x;
        } else if( ( y >= x ) && ( y >= z ) ) {
            // y is largest, project on xz plane
            nu = TriArea2D( p.x, p.z, b.x, b.z, c.x, c.z );
            nv = TriArea2D( p.x, p.z, c.x, c.z, a.x, a.z );
            ood = 1.0f / -m.y;
        } else {
            // z is largest, project onto xy plane
            nu = TriArea2D( p.x, p.y, b.x, b.y, c.x, c.y );
            nv = TriArea2D( p.x, p.y, c.x, c.y, a.x, a.y );
            ood = 1.0f / m.z;
        }

        u = nu * ood;
        v = nv * ood;
        w = 1.0f - u - v;
    }

    static public bool PointInsideTriangle( Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3 )
    {
        float u, v, w;
        Barycentric( t1, t2, t3, p, out u, out v, out w );
        return ( ( v >= 0.0f ) && ( w >= 0.0f ) && ( ( v + w ) >= 1.0f ) );
    }

    // will search through all the devactivated children
    static public T GetAllChildrenComponent<T>( GameObject baseObject ) where T : Component
    {
        if( baseObject != null ) {
            Transform root = baseObject.transform;
            T cmp = baseObject.GetComponent<T>( );
            if( cmp != null ) {
                return cmp;
            }

            for( int i = 0; i < root.childCount; ++i ) {
                cmp = GetAllChildrenComponent<T>( root.GetChild( i ).gameObject );
                if( cmp != null ) return cmp;
            }
        }

        return null;
    }

    // will also search through deactivated children, which GetComponentsInChildren<T> doesn't do
    static public List<T> GetAllChildrenComponents<T>( GameObject baseObject ) where T : Component
    {
        List<T> comps = new List<T>( );

        if( baseObject != null ) {
            Transform root = baseObject.transform;
            T cmp = baseObject.GetComponent<T>( );
            if( cmp != null ) {
                comps.Add( cmp );
            }

            for( int i = 0; i < root.childCount; ++i ) {
                comps.AddRange( GetAllChildrenComponents<T>( root.GetChild( i ).gameObject ) );
            }
        }

        return comps;
    }

    static public void CheckAndUnspawn( GameObject obj )
    {
        if( obj == null ) return;
        if( CheckForOnNetwork( obj ) ) {
            NetworkServer.UnSpawn( obj );
        }
    }

    static public void CheckNetworkIDAndDestroy( GameObject obj )
    {
        if( obj == null ) return;
        if( CheckForOnNetwork( obj ) ) {
            NetworkServer.Destroy( obj );
        } else {
            GameObject.Destroy( obj );
        }
    }

    static public bool CheckForOnNetwork( GameObject obj )
    {
        if( obj == null ) return false;

        NetworkIdentity nid = obj.GetComponent<NetworkIdentity>( );
        if( nid == null ) return false;

        GameObject gob = NetworkServer.FindLocalObject( nid.netId );
        if( gob == null ) return false;

        return true;
    }

    static public float RemapFloat( float minInput, float maxInput, float minOutput, float maxOutput, float val )
    {
        return Mathf.Lerp( minOutput, maxOutput, Mathf.InverseLerp( minInput, maxInput, val ) );
    }

    static public void OpenEMailClient( string sendTo, string subject = null, string body = null )
    {
        string url = "mailto:" + sendTo;
        bool first = true;

        if( !string.IsNullOrEmpty( subject ) ) {
            subject = WWW.EscapeURL( subject ).Replace( "+", "%20" );
            url += ( first ? "?" : "&" ) + "subject=" + subject;
            first = false;
        }

        if( !string.IsNullOrEmpty( body ) ) {
            body = WWW.EscapeURL( body ).Replace( "+", "%20" );    
            url += ( first ? "?" : "&" ) + "body=" + body;
            first = false;
        }
        
        Application.OpenURL( url );
    }

    public class Weight {
        private float _weight; // base weight is in stones

        public static Weight FromPounds( float lbs )
        {
            Weight w = new Weight( );
            w.Pounds = lbs;
            return w;
        }

        public float Raw {
            get { return _weight; }
            set { _weight = value; }
        }

        public float Pounds {
            get { return ( _weight * 14.0f ); }
            set { _weight = value / 14.0f; }
        }

        public float Kilos {
            get { return ( _weight * 6.35029f ); }
            set { _weight = value / 6.35029f; }
        }
    }

    public class Length {
        private float _length; // base length is in yards

        public float Raw {
            get { return _length; }
            set { _length = value; }
        }

        public float Feet {
            get { return _length * 3.0f; }
            set { _length = value / 3.0f; }
        }

        public float Inches {
            get { return _length * 36.0f; }
            set { _length = value / 36.0f; }
        }

        public float Meters {
            get { return _length * 0.9144f; }
            set { _length = value / 0.9144f; }
        }

        public float Miles {
            get { return _length / 1760.0f; }
            set { _length = value * 1760.0f; }
        }

        public float Kilometers {
            get { return _length / 1093.61f; }
            set { _length = value * 1093.61f; }
        }

        public static Length FromMiles( float miles )
        {
            Length l = new Length( );
            l.Miles = miles;
            return l;
        }

        public static Length FromRaw( float raw )
        {
            Length l = new Length( );
            l.Raw = raw;
            return l;
        }

        public static Length FromFeet( float feet )
        {
            Length l = new Length( );
            l.Feet = feet;
            return l;
        }

        public static Length FromInches( float inches )
        {
            Length l = new Length( );
            l.Inches = inches;
            return l;
        }

        public static Length FromMeters( float meters )
        {
            Length l = new Length( );
            l.Meters = meters;
            return l;
        }

        public static Length operator *( float f, Length t )
        {
            Length l = new Length( );
            l._length = t._length * f;
            return l;
        }

        public static Length operator *( Length t, float f )
        {
            Length l = new Length( );
            l._length = t._length * f;
            return l;
        }

        public static Length operator +( Length lhs, Length rhs )
        {
            Length l = new Length( );
            l._length = lhs._length + rhs._length;
            return l;
        }
    }

    public static long Lerpl( long a, long b, float t )
    {
        return a + (long)( t * ( b - a ) );
    }

    public static void WriteOutVersionNumber( System.IO.Stream s, int version )
    {
        List<byte> data = new List<byte>( );
        data.AddRange( BitConverter.GetBytes( version ) );
        s.Write( data.ToArray( ), 0, data.Count );
    }

    public static int ReadInVersionNumber( System.IO.Stream s )
    {
        byte[] data = new byte[sizeof(int)];
        s.Read( data, 0, data.Length );
        return BitConverter.ToInt32( data, 0 );
    }

    // simple compact way to store the average value of a series of floats
    public class CumulativeMovingAverage {

        private float _numSamples = 0.0f;
        private float _avg = 0.0f;

        public float Avg { get { return _avg; } }
        public float NumSamples { get { return _numSamples; } }

        public void AddValue( float newVal )
        {
            _avg = ( newVal + ( _avg * _numSamples ) ) / ( _numSamples + 1.0f );
            _numSamples += 1.0f;
        }

        public void Reset( )
        {
            _numSamples = 0.0f;
            _avg = 0.0f;
        }
    }

    public static DateTime GetStartOfWeek( DateTime currTime )
    {
        DateTime result = currTime;
        while( result.DayOfWeek != DayOfWeek.Sunday) {
            try {
                result = result.AddDays( -1 );
            } catch( ArgumentOutOfRangeException ) {
                result = DateTime.MaxValue;
                while( result.DayOfWeek != DayOfWeek.Sunday ) {
                    result = result.AddDays( -1 );
                }
            }
        }

        return result;
    }

    public static DateTime GetStartOfMonth( DateTime currTime )
    {
        DateTime dt = new DateTime( currTime.Year, currTime.Month, 1 );
        return dt;
    }

    public static bool AreBLEUUIDsEqual( string uuidOne, string uuidTwo )
    {
        uuidOne = FullBLEUUID( uuidOne );
        uuidTwo = FullBLEUUID( uuidTwo );

        return uuidOne.Equals( uuidTwo, StringComparison.InvariantCultureIgnoreCase );
    }

    public static string FullBLEUUID( string uuid )
    {
        if( uuid.Length == 4 ) {
            uuid = "0000" + uuid + "-0000-1000-8000-00805f9b34fb";
        }

        return uuid;
    }

    public static void OpenAppStoreLink( string androidID, string iosID )
    {
        #if UNITY_ANDROID
            Application.OpenURL( "market://details?id=" + androidID );
        #elif UNITY_IPHONE
            Application.OpenURL( "itms-apps://itunes.apple.com/app/id" + iosID );
        #endif
    }
}

public static class Vector2Extension {
	public static Vector2 ProjectOnto( this Vector2 thisVec, Vector2 onto )
	{
		return ( Vector2.Dot( thisVec, onto ) / onto.sqrMagnitude ) * onto;
	}

	public static Vector2 PerpindicularTo( this Vector2 thisVec, Vector2 perpTo )
	{
		return thisVec - ( ( Vector2.Dot( thisVec, perpTo ) / perpTo.sqrMagnitude ) * perpTo );
	}

    public static Vector2 HadamardProduct( this Vector2 thisVec, Vector2 other )
    {
        return new Vector2( ( thisVec.x * other.x ), ( thisVec.y * other.y ) );
    }
}

public static class Vector3Extension {
    public static Vector3 ProjectOnto( this Vector3 thisVec, Vector3 onto )
    {
        return ( Vector3.Dot( thisVec, onto ) / onto.sqrMagnitude ) * onto;
    }

    public static Vector3 PerpindicularTo( this Vector3 thisVec, Vector3 perpTo )
    {
        return thisVec - ( ( Vector3.Dot( thisVec, perpTo ) / perpTo.sqrMagnitude ) * perpTo );
    }

    public static Vector3 HadamardProduct( this Vector3 thisVec, Vector3 other )
    {
        return new Vector3( ( thisVec.x * other.x ), ( thisVec.y * other.y ), ( thisVec.z * other.z ) );
    }

    public static Vector2 ToVector2( this Vector3 thisVec )
    {
        return new Vector2( thisVec.x, thisVec.y );
    }
}