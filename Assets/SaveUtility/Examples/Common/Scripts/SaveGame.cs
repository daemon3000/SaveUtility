using UnityEngine;
using System;
using System.IO;
using System.Collections;
using TeamUtility.IO.SaveUtility;

public sealed class SaveGame : MonoBehaviour 
{
	private enum SaveFormat
	{
		Binary, Json, PlayerPrefs
	}
	
	public int exampleCount;
	private string _saveLocationBinary;
	private string _saveLocationJson;
	private string _saveKeyPlayerPrefs;
	private SaveFormat _saveFormat;
	
	private void Awake()
	{
		_saveFormat = SaveFormat.Json;
		_saveLocationBinary = PathHelper.Combine(GetSaveFolder(), "example_" + exampleCount + ".bin");
		_saveLocationJson = PathHelper.Combine(GetSaveFolder(), "example_" + exampleCount + ".json");
		_saveKeyPlayerPrefs = "example_" + exampleCount + "_save";
	}

	private string GetSaveFolder()
	{
#if UNITY_WINRT && !UNITY_EDITOR
		string saveFolder = PathHelper.Combine(UnityEngine.Windows.Directory.localFolder, "Saves");
		if(!UnityEngine.Windows.Directory.Exists(saveFolder))
		{
			UnityEngine.Windows.Directory.CreateDirectory(saveFolder);
		}

		return saveFolder;
#else
		string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		string saveFolder = PathHelper.Combine(documentsFolder, @"SaveUtility\Saves");
		if(!Directory.Exists(saveFolder)) 
		{
			Directory.CreateDirectory(saveFolder);
		}
		
		return saveFolder;
#endif
	}
	
	private void OnGUI()
	{

		Rect area = new Rect(0.0f, Screen.height - 100.0f, Screen.width, 100.0f);
		GUILayout.BeginArea(area);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Current Save Format:", GUILayout.Width(160));
		if(GUILayout.Button(_saveFormat.ToString(), GUILayout.Width(200)))
		{
			CycleSaveFormat();
		}
		GUILayout.EndHorizontal();
		GUI.color = Color.red;
		GUILayout.Label("Press F5 to save the game");
		GUILayout.Label("Press F9 to load the game");
		if(_saveFormat == SaveFormat.Binary)
			GUILayout.Label("Save File: " + _saveLocationBinary);
		else if(_saveFormat == SaveFormat.Json)
			GUILayout.Label("Save File: " + _saveLocationJson);
		else
			GUILayout.Label("Save Key: " + _saveKeyPlayerPrefs);
		GUI.color = Color.white;
		GUILayout.EndArea();
	}

	private void CycleSaveFormat()
	{
		if(_saveFormat == SaveFormat.Binary)
			_saveFormat = SaveFormat.Json;
		else if(_saveFormat == SaveFormat.Json)
			_saveFormat = SaveFormat.PlayerPrefs;
		else
			_saveFormat = SaveFormat.Binary;
	}
	
	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.F5))
		{
			IDataSerializer serializer;
			if(_saveFormat == SaveFormat.Binary)
			{
				serializer = new BinarySerializer(_saveLocationBinary);
			}
			else if(_saveFormat == SaveFormat.Json)
			{
				serializer = new JsonSerializer(_saveLocationJson);
			}
			else
			{
				serializer = new PlayerPrefsSerializer(_saveKeyPlayerPrefs);
			}
			
			SaveUtility.Save(serializer);
		}
		else if(Input.GetKeyDown(KeyCode.F9))
		{
			IDataDeserializer deserializer;
			if(_saveFormat == SaveFormat.Binary)
			{
				deserializer = new BinaryDeserializer(_saveLocationBinary);
			}
			else if(_saveFormat == SaveFormat.Json)
			{
				deserializer = new JsonDeserializer(_saveLocationJson);
			}
			else
			{
				deserializer = new PlayerPrefsDeserializer(_saveKeyPlayerPrefs);
			}
			
			SaveUtility.Load(deserializer);
		}
	}
}
