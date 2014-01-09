using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public sealed class AddForceWithMouse : MonoBehaviour 
{
	public float forceMultiplier;
	private Vector3 startPos;
	
	private void OnGUI()
	{
		GUILayout.Label("Hold LMB and drag to move the ball");
	}
	
	private void Update()
	{
		if(Input.GetMouseButtonDown(0))
		{
			startPos = Input.mousePosition;
		}
		else if(Input.GetMouseButtonUp(0))
		{
			Vector3 direction = Input.mousePosition - startPos;
			rigidbody.AddForce(direction.normalized * (forceMultiplier * direction.magnitude), ForceMode.VelocityChange);
		}
	}
}
