using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public sealed class AddForceWithMouse : MonoBehaviour 
{
	public float forceMultiplier;
	private Vector3 _startPos;
	private Rigidbody _rigidbody;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
	}
	
	private void OnGUI()
	{
		GUILayout.Label("Hold LMB and drag to move the ball");
	}
	
	private void Update()
	{
		if(Input.GetMouseButtonDown(0))
		{
			_startPos = Input.mousePosition;
		}
		else if(Input.GetMouseButtonUp(0))
		{
			Vector3 direction = Input.mousePosition - _startPos;
			_rigidbody.AddForce(direction.normalized * (forceMultiplier * direction.magnitude), ForceMode.VelocityChange);
		}
	}
}
