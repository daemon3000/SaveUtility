using UnityEngine;
using System;
using System.Collections;
using TeamUtility.IO.SaveUtility;

[SaveComponent]
public sealed class AutomaticSaving : MonoBehaviour 
{
	[SaveField] private int _gold;
	[SaveField] private int _hp;
	[SaveField] private int _mana;
	private Vector2 _scrollPos;
	
	private void OnGUI()
	{
		Rect area = new Rect(Screen.width / 2, 0.0f, Screen.width / 2, Screen.height / 2);
		GUILayout.BeginArea(area);
		_scrollPos = GUILayout.BeginScrollView(_scrollPos);
		GUILayout.Label("Automatic Saving");
		GUILayout.BeginHorizontal();
		GUILayout.Label("Gold: " + _gold);
		if(GUILayout.Button("+"))
		{
			_gold++;
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("HP: " + _hp);
		if(GUILayout.Button("+"))
		{
			_hp++;
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Mana: " + _mana);
		if(GUILayout.Button("+"))
		{
			_mana++;
		}
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
	
	//	Called when all the components of this game object have been loaded.
	private void OnDeserialized()
	{
//		Debug.Log("Script loaded");
	}
}
