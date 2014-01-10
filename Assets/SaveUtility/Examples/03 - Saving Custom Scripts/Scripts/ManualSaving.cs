using UnityEngine;
using System;
using System.Collections.Generic;
using TeamUtility.IO.SaveUtility;

[SaveComponent]
public sealed class ManualSaving : MonoBehaviour, ISerializableMonoBehaviour
{
	private int _gold;
	private int _hp;
	private int _mana;
	private Vector2 _scrollPos;
	
	private void OnGUI()
	{
		Rect area = new Rect(0.0f, 0.0f, Screen.width / 2, Screen.height / 2);
		GUILayout.BeginArea(area);
		_scrollPos = GUILayout.BeginScrollView(_scrollPos);
		GUILayout.Label("Manual Saving");
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
	
	public Dictionary<string, object> Serialize()
	{
		Dictionary<string, object> dict = new Dictionary<string, object>();
		dict.Add("_gold", _gold);
		dict.Add("_hp", _hp);
		dict.Add("_mana", _mana);
		
		return dict;
	}
	
	public void Deserialize(Dictionary<string, object> data)
	{
		_gold = System.Convert.ToInt32(data["_gold"]);
		_hp = System.Convert.ToInt32(data["_hp"]);
		_mana = System.Convert.ToInt32(data["_mana"]);
	}
}
