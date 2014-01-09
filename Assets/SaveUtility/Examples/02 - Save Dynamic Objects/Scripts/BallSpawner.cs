using UnityEngine;
using System;
using System.Collections;
using TeamUtility.IO.SaveUtility;
using UnityRandom = UnityEngine.Random;

public sealed class BallSpawner : MonoBehaviour 
{
	//	The template needs to be registered with the SaveUtility instance.
	public GameObject template;
	public Transform[] spawnLocations;
	
	private void OnGUI()
	{
		GUILayout.Label("Press LMB to spawn a ball");
	}
	
	private void Update()
	{
		if(Input.GetMouseButtonDown(0) && spawnLocations.Length > 0)
		{
			Transform location = spawnLocations[UnityRandom.Range(0, spawnLocations.Length)];
			SaveUtility.InstantiateSavable(template, location.position, Quaternion.identity);
		}
	}
}
