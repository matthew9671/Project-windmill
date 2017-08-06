using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controller : MonoBehaviour {

	bool mouseDown = false;
	Vector3 mousePos;
	const int sides = 4;
	const float acc = 0.5f;
	const float maxV = 8f;
	const float damp = 0.85f;
	const float sec_damp = 0.02f;
	const float v_threshold = 1f;
	const float diff_threshold = 2f;
	public float v;
	public float diff = 0f;
	float dragSpeed = 1f;
	Transform currObj;
	[SerializeField] GameObject table;
	[SerializeField] GameObject tableCenter;

	// Indicator related
	const int scanSteps = 10;
	const float range = 2.0f;
	Dictionary<GameObject, int> indicatorWeights = new Dictionary<GameObject, int>();
	[SerializeField] Texture2D indicator;
	const float indicatorSize = 30f;

	// Inspection related
	bool zoomIn = false;
	bool inAnimation = false;
	Vector3 camRetPos;
	Quaternion camRetRot;
	const float cameraSpeed = 0.08f;
	const int cameraTime = 70;

	// Black Magic
	public static controller CT;

	void Awake()
	{
		if (CT != null) 
		{
			GameObject.Destroy (CT);
		} 
		else 
		{
			CT = this;
		}
		DontDestroyOnLoad(this);
	}

	void Update () 
	{
		if (inAnimation) return;
		if (zoomIn)
		{
			update_zoom_in();
		}
		else
		{
			update_zoom_out();
		}
	}

	void update_zoom_in()
	{
		if (Input.GetButtonDown("Fire2"))
		{
			StartCoroutine(move_camera(camRetPos, camRetRot, delegate() {
				zoomIn = false;
			}));
		}
	}

	void update_zoom_out()
	{
		// Deal with rotation of the scene
		if (mouseDown)
		{
			diff += (mousePos.x - Input.mousePosition.x) * dragSpeed;
			mousePos = Input.mousePosition;
			smooth_interpolation(diff, false);
			diff -= v;
			table.transform.Rotate(0f, v, 0f);
		}
		else
		{
			smooth_interpolation(diff, true);
			diff -= v;
			table.transform.Rotate(0f, v, 0f);
		}
		// Deal with mouseInput
		if (Input.GetButtonDown("Fire1"))
		{
			mouseDown = true;
			mousePos = Input.mousePosition;
		}
		if (Input.GetButtonUp("Fire1"))
		{
			float currAng = table.transform.rotation.eulerAngles.y;
			diff = Mathf.DeltaAngle(currAng, 0f);
			for (int i = 1; i < sides; i++)
			{
				float ang = 360f / sides * i;
				float angDiff = Mathf.DeltaAngle(currAng, ang);
				if (Mathf.Abs(angDiff) < Mathf.Abs(diff)) diff = angDiff;
			}
			//diff = clamp_angle(diff);
			mouseDown = false;
		}
		// Use raycast for highlighting objects
		Ray ray;
		RaycastHit hit;
		indicatorWeights.Clear();
		for (int i = 1; i < scanSteps + 1; i++)
		{
			for (int dir = 0; dir < 2; dir++)
			{
				Vector3 rayPoint = new Vector3(Mathf.Lerp(Screen.width * dir, Screen.width/2, (float)i / (float)scanSteps), Screen.height / 2, range);
				rayPoint = Camera.main.ScreenToWorldPoint(rayPoint);
				ray = new Ray(rayPoint, tableCenter.transform.position - rayPoint);
				Debug.DrawRay(rayPoint, tableCenter.transform.position - rayPoint);
				Physics.Raycast(ray, out hit, Vector3.Magnitude(tableCenter.transform.position - rayPoint), 
					layerMask:LayerMask.GetMask("Table"));
				if (hit.collider != null)
				{
					GameObject curr = hit.collider.gameObject;
					indicatorWeights[curr] = i;
				}
			}
		}
	}

	void OnGUI()
	{
		foreach(GameObject obj in indicatorWeights.Keys)
		{
			Vector3 center = Camera.main.WorldToScreenPoint(obj.transform.position);
			float size = indicatorSize * indicatorWeights[obj] / scanSteps;
			Rect bounds = new Rect (center.x - size / 2, 
				Screen.height - center.y - size / 2,
									size, size);
			GUI.DrawTexture (bounds, indicator);
		}
	}

	void smooth_interpolation (float diff, bool inertia)
	// Assume that the numbers for start and target wrap around (180 = -180)
	{
		if (inertia)
		{
			float dir = Mathf.Sign(diff);
			v = v * damp - v * Mathf.Abs(v) * sec_damp + dir * acc;
			v = Mathf.Clamp(v, -maxV, maxV);
			if (Mathf.Abs(v) < v_threshold && Mathf.Abs(diff) < diff_threshold)
			{
				v = diff;
			}
		}
		else
		{
			// There is no overshooting when we are dragging by hand
			if (maxV > Mathf.Abs(diff))
			{
				v = diff;
			}
			else
			{
				v = maxV * Mathf.Sign(diff);
			}
		}
	}

	float float_mod(float a, float b)
	// Assuming b > 0
	// Returns m so that 0 <= m < b and a = r * b + m where r is integer
	// Somehow the built-in mod doesn't do this??
	{
		if (a > 0)
		{
			return a % b;
		}
		else
		{
			return a % b + b;
		}
	}

	float clamp_angle(float ang)
	// Return the equivalent of ang in [-180, 180)
	{
		return float_mod(ang + 180f, 360f) - 180f;
	}

	public void inspect_object(Transform target)
	{
		if (zoomIn || inAnimation) return;
		camRetPos = Camera.main.transform.position;
		camRetRot = Camera.main.transform.rotation;
		StartCoroutine(move_camera(target.position, target.rotation, delegate() {
			zoomIn = true;
		}));
	}

	IEnumerator move_camera (Vector3 pos, Quaternion rot, Action callWhenFinished)
	{
		inAnimation = true;
		Transform camTrans = Camera.main.transform;
		for (int i = 0; i < cameraTime; i++)
		{
			camTrans.position = Vector3.Lerp(camTrans.position, pos, cameraSpeed);
			camTrans.rotation = Quaternion.Slerp(camTrans.rotation, rot, cameraSpeed);
			yield return new WaitForFixedUpdate();
		}
		camTrans.position = pos;
		camTrans.rotation = rot;
		inAnimation = false;
		callWhenFinished();
	}
}
