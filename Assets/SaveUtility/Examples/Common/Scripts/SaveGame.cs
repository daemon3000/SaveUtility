using UnityEngine;
using System;
using System.IO;
using System.Collections;
using TeamUtility.IO.SaveUtility;

public sealed class SaveGame : MonoBehaviour 
{
	public enum SaveFormat
	{
		Binary, Json, PlayerPrefs
	}
	
	public SaveFormat saveFormat;
	public int exampleCount;
	private string _saveLocation;
	
	private void Awake()
	{
		if(saveFormat == SaveFormat.Binary)
		{
			_saveLocation = Path.Combine(GetSaveFolder(), "example_" + exampleCount + ".bin");
		}
		else if(saveFormat == SaveFormat.Json)
		{
			_saveLocation = Path.Combine(GetSaveFolder(), "example_" + exampleCount + ".json");
		}
		else
		{
			_saveLocation = "example_" + exampleCount + "_save";
		}
	}
	
	private string GetSaveFolder()
	{
		string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		string saveFolder = Path.Combine(documentsFolder, @"SaveUtility\Saves");
		
		if(!Directory.Exists(saveFolder)) 
		{
			Directory.CreateDirectory(saveFolder);
		}
		
		return saveFolder;
	}
	
	private void OnGUI()
	{
		GUI.color = Color.red;
		Rect area = new Rect(0.0f, Screen.height - 80.0f, Screen.width, 80.0f);
		GUILayout.BeginArea(area);
		GUILayout.Label("Press F5 to save the game");
		GUILayout.Label("Press F9 to load the game");
		if(saveFormat == SaveFormat.Binary || saveFormat == SaveFormat.Json)
			GUILayout.Label("Save File: " + _saveLocation);
		GUILayout.EndArea();
		GUI.color = Color.white;
	}
	
	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.F5))
		{
			IDataSerializer serializer;
			if(saveFormat == SaveFormat.Binary)
			{
				serializer = new BinarySerializer(_saveLocation);
			}
			else if(saveFormat == SaveFormat.Json)
			{
				serializer = new JsonSerializer(_saveLocation);
			}
			else
			{
				serializer = new PlayerPrefsSerializer(_saveLocation);
			}
			
			SaveGameManager.Save(serializer);
		}
		else if(Input.GetKeyDown(KeyCode.F9))
		{
			IDataDeserializer deserializer;
			if(saveFormat == SaveFormat.Binary)
			{
				deserializer = new BinaryDeserializer(_saveLocation);
			}
			else if(saveFormat == SaveFormat.Json)
			{
				deserializer = new JsonDeserializer(_saveLocation);
			}
			else
			{
				deserializer = new PlayerPrefsDeserializer(_saveLocation);
			}
			
			SaveGameManager.Load(deserializer);
		}
	}
}
