using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FalseBezel : MonoBehaviour {
	public Camera displayCamera;

	public Material bezelMaterial;

	[Range(0.0f,1.0f)]
	public float distance;

	[Range(0.0f,1.0f)]
	public float percentBorderWidth;

	[Range(0.0f,1.0f)]
	public float percentBorderHeight;

	public bool recalculate;

	void Start()
	{
		CreateBorderGeometry( );
	}

	void Update() {
		if( recalculate ) {
			CreateBorderGeometry( );
			recalculate = false;
		}
	}
	
	void CreateBorderGeometry( )
	{
		float depth = Mathf.Lerp( displayCamera.nearClipPlane, displayCamera.farClipPlane, distance );

		Vector3 outerLowerLeft = displayCamera.ScreenToWorldPoint( new Vector3( 0.0f, 0.0f, depth ) ) - transform.position;
		Vector3 outerUpperLeft = displayCamera.ScreenToWorldPoint( new Vector3( 0.0f, displayCamera.pixelHeight, depth ) ) - transform.position;
		Vector3 outerLowerRight = displayCamera.ScreenToWorldPoint( new Vector3( displayCamera.pixelWidth, 0.0f, depth ) ) - transform.position;
		Vector3 outerUpperRight = displayCamera.ScreenToWorldPoint( new Vector3( displayCamera.pixelWidth, displayCamera.pixelHeight, depth ) ) - transform.position;

		float borderWidth = (float)displayCamera.pixelWidth * 0.5f * percentBorderWidth;
		float borderHeight = (float)displayCamera.pixelHeight * 0.5f * percentBorderHeight;

		Vector3 innerLowerLeft = displayCamera.ScreenToWorldPoint( new Vector3( borderWidth, borderHeight, depth ) ) - transform.position;
		Vector3 innerUpperLeft = displayCamera.ScreenToWorldPoint( new Vector3( borderWidth, displayCamera.pixelHeight - borderHeight, depth ) ) - transform.position;
		Vector3 innerLowerRight = displayCamera.ScreenToWorldPoint( new Vector3( displayCamera.pixelWidth - borderWidth, borderHeight, depth ) ) - transform.position;
		Vector3 innerUpperRight = displayCamera.ScreenToWorldPoint( new Vector3( displayCamera.pixelWidth - borderWidth, displayCamera.pixelHeight - borderHeight, depth ) ) - transform.position;

		/*
		 * 
		 *  0            1
		 *   |----------|
		 *   |          |
		 *   |  |----|  |
		 *   |  |4  5|  |
		 *   |  |    |  |
		 *   |  |7__6|  |
		 *   |          |
		 *   |__________|
		 *  3           2
		 * 
		 */
		Vector3[] verts = { outerUpperLeft, outerUpperRight, outerLowerRight, outerLowerLeft, innerUpperLeft, innerUpperRight, innerLowerRight, innerLowerLeft };
		int[] tris = {	0, 1, 5,
						0, 5, 4,
						5, 1, 6,
						6, 1, 2,
						7, 6, 2,
						3, 7, 2,
						3, 4, 7,
						0, 4, 3 };

		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>( );
		if( meshFilter == null ) {
			meshFilter = gameObject.AddComponent<MeshFilter>( );
			if( meshFilter == null ) {
				Debug.LogError( "Unable to create MeshFilter for border." );
				return;
			}
		}


		Mesh mesh;
#if UNITY_EDITOR
		mesh = Mesh.Instantiate( meshFilter.sharedMesh ) as Mesh;
		mesh.name = gameObject.name; // this is just for aesthetics in log messages
		meshFilter.mesh = mesh;
#else
		if( meshFilter.mesh == null ) {
			meshFilter.mesh = new Mesh();
			if( meshFilter.mesh == null ) {
				Debug.LogError( "Unable to create Mesh for border." );
				return;
			}
		}
		mesh = meshFilter.mesh;
#endif
			

		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>( );
		if( renderer == null ) {
			renderer = gameObject.AddComponent<MeshRenderer>( );
			if( renderer == null ) {
				Debug.LogError( "Unable to create MeshRenderer for border." );
				return;
			}
		}
		renderer.material = bezelMaterial;

		mesh.Clear( );
		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.subMeshCount = 1;
	}
}
