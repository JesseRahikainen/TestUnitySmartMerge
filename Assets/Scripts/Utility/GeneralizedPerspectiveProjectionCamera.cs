using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GeneralizedPerspectiveProjectionCamera : MonoBehaviour {

	public Transform screenBottomLeft;
	public Transform screenBottomRight;
	public Transform screenTopLeft;
	public Transform screenTopRight;
	public bool adjustNear = false;
	public float explicitNear = 0.3f;

	public void XChange( float x )
	{
		Vector3 newPos = transform.position;
		newPos.x = x;
		transform.position = newPos;
	}

	public void YChange( float y )
	{
		Vector3 newPos = transform.position;
		newPos.y = y;
		transform.position = newPos;
	}

	public void ZChange( float z )
	{
		Vector3 newPos = transform.position;
		newPos.z = z;
		transform.position = newPos;
	}

	Matrix4x4 PerspectiveOffCenter( float left, float right, float bottom, float top, float near, float far ) {
		float x = 2.0F * near / (right - left);
		float y = 2.0F * near / (top - bottom);
		float a = (right + left) / (right - left);
		float b = (top + bottom) / (top - bottom);
		float c = -(far + near) / (far - near);
		float d = -(2.0F * far * near) / (far - near);
		float e = -1.0F;
		Matrix4x4 m = new Matrix4x4();
		m[0, 0] = x;
		m[0, 1] = 0;
		m[0, 2] = a;
		m[0, 3] = 0;
		m[1, 0] = 0;
		m[1, 1] = y;
		m[1, 2] = b;
		m[1, 3] = 0;
		m[2, 0] = 0;
		m[2, 1] = 0;
		m[2, 2] = c;
		m[2, 3] = d;
		m[3, 0] = 0;
		m[3, 1] = 0;
		m[3, 2] = e;
		m[3, 3] = 0;
		return m;
	}

	Matrix4x4 GeneralizedPerspectiveProjection(Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pe, float near, float far)
	{
		Vector3 va, vb, vc;
		Vector3 vr;
		Vector3 vu;
		Vector3 vn;
		
		float left, right, bottom, top, eyedistance;

		Matrix4x4 projectionM;
		
		///Calculate the orthonormal for the screen (the screen coordinate system
		vr = pb - pa;
		vr.Normalize();
		vu = pc - pa;
		vu.Normalize();
		vn = -Vector3.Cross(vr, vu);
		vn.Normalize();
		
		//Calculate the vector from eye (pe) to screen corners (pa, pb, pc)
		va = pa-pe;
		vb = pb-pe;
		vc = pc-pe;
		
		//Get the distance;; from the eye to the screen plane
		eyedistance = -(Vector3.Dot(va, vn));
		
		//Get the varaibles for the off center projection
		left = (Vector3.Dot(vr, va)*near)/eyedistance;
		right  = (Vector3.Dot(vr, vb)*near)/eyedistance;
		bottom  = (Vector3.Dot(vu, va)*near)/eyedistance;
		top = (Vector3.Dot(vu, vc)*near)/eyedistance;
		
		//Get this projection
		projectionM = PerspectiveOffCenter(left, right, bottom, top, near, far);
		
		//finally return
		return projectionM;
	}

	void Update( )
	{
		Camera cam = GetComponent<Camera>( );
		if( ( screenBottomLeft == null ) || ( screenBottomRight == null ) ||
		    ( screenTopLeft == null ) || ( screenTopRight == null ) ||
		    ( cam == null ) ) {
			return;
		}

		if( adjustNear ) {
			Vector3 screenNormal = screenBottomLeft.transform.forward; // assuming the screenBottomLeft has no local rotation
			cam.nearClipPlane = Vector3.Project( ( screenBottomLeft.transform.position - cam.transform.position ), screenNormal ).magnitude;
		} else {
			cam.nearClipPlane = explicitNear;
		}

		//calculate projection
		cam.projectionMatrix = GeneralizedPerspectiveProjection(
			screenBottomLeft.position, screenBottomRight.position,
			screenTopLeft.position, cam.transform.position,
			cam.nearClipPlane, cam.farClipPlane);
		cam.transform.rotation = screenBottomLeft.rotation; // rotation needs to be the same for this to work correctly
	}
}
