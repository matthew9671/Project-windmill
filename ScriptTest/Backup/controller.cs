using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controller : MonoBehaviour {

	bool mouseDown = false;
	Vector3 mousePos;
	float baseAngle;
	const float acc = 1f;
	const float maxV = 4f;
	float v;
	float dragSpeed = 1f;
	float returnSpeed = 3f;
	Transform currObj;
	[SerializeField] GameObject table;

	void Update () 
	{
		if (mouseDown)
		{
			float start = table.transform.rotation.eulerAngles.y;
			float target = start + Input.mousePosition.x - mousePos.x;
			table.transform.rotation = Quaternion.Euler(0f, smooth_interpolation(start, target, false), 0f);
		}
		else
		{
			float angle = table.transform.rotation.eulerAngles.y;
			table.transform.rotation = Quaternion.Euler(0f, Mathf.LerpAngle(angle, 0f, Time.deltaTime * returnSpeed), 0f);
		}
		// Use raycast for highlighting objects
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));
		RaycastHit hit;
		Physics.Raycast(ray, out hit, Mathf.Abs(Camera.main.transform.position.z), layerMask:LayerMask.GetMask("Table"));
		if (hit.collider != null)
		{
			Debug.Log("Hit!");
			if (currObj != hit.collider.gameObject.transform)
			{
				if (currObj != null)
				{
					currObj.localScale /= 1.2f;
				}
				currObj = hit.collider.gameObject.transform;
				currObj.localScale *= 1.2f;
			}
		}
		else
		{
			if (currObj != null)
			{
				currObj.localScale /= 1.2f;
				currObj = null;
			}
		}
	}

	float smooth_interpolation (float start, float target, bool inertia)
	{
		float result;
		if (inertia)
		{
			
		}
		else
		{
			
			if (v == 0)
			{
				v = maxV * Mathf.Sign(target - start);
			}
		}
		return start + v;
	}

	float get_angle()
	{
		return 0f;
	}

	void OnMouseDown()
	{
		baseAngle = table.transform.rotation.eulerAngles.y;
		mouseDown = true;
		mousePos = Input.mousePosition;
	}

	void OnMouseUp()
	{
		mouseDown = false;
	}

}
