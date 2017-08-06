using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactable : MonoBehaviour {

	public Transform cameraPos;

	void OnMouseDown()
	{
		controller.CT.inspect_object(cameraPos);
	}
}
