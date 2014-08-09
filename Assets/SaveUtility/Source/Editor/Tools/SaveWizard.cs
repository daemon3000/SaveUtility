#region [Copyright (c) 2013-2014 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2013-2014 Cristian Alexandru Geambasu
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
//	and associated documentation files (the "Software"), to deal in the Software without restriction, 
//	including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using TeamUtility.IO.SaveUtility;
using SaveUtilityClass = TeamUtility.IO.SaveUtility.SaveUtility;

namespace TeamUtility.Editor.IO.SaveUtility
{
	public sealed class SaveWizard : EditorWindow
	{
		[SerializeField] private SaveUtilityClass _saveUtility;
		[SerializeField] private GameObject _selection;
		[SerializeField] private GUIStyle _bigLabelStyle;
		[SerializeField] private GUIStyle _mediumLabelStyle;
		private Color removeButtonColor = new Color(0.85f, 0.5f, 0.5f, 1.0f);
		
		private void OnEnable()
		{
			if(_saveUtility == null) 
			{
				_saveUtility = SaveUtilityClass.GetInstance(false);
			}
			if(_bigLabelStyle == null) 
			{
				_bigLabelStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
				_bigLabelStyle.alignment = TextAnchor.MiddleCenter;
				_bigLabelStyle.fontStyle = FontStyle.Bold;
				_bigLabelStyle.fontSize = 14;
				_bigLabelStyle.normal.textColor = Color.grey;
			}
			if(_mediumLabelStyle == null) 
			{
				_mediumLabelStyle = new GUIStyle(EditorStyles.whiteLabel);
				_mediumLabelStyle.alignment = TextAnchor.MiddleCenter;
			}
			if(_selection == null)
			{
				if(Selection.activeGameObject != null && !EditorUtility.IsPersistent(Selection.activeGameObject))
				{
					_selection = Selection.activeGameObject;
				}
			}
		}
		
		private void OnSelectionChange()
		{
			if(Selection.activeGameObject != null && !EditorUtility.IsPersistent(Selection.activeGameObject))
			{
				_selection = Selection.activeGameObject;
				Repaint();
			}
		}
		
		private void OnGUI()
		{
			if(_saveUtility == null) 
			{
				DisplayNoSaveUtilityWarning();
			}
			else if(_selection == null)
			{
				DisplayNoSelectionWarning();
			}
			else
			{
				DisplayMainGUI();
			}
		}
		
		private void DisplayNoSaveUtilityWarning()
		{
			Rect position = new Rect(0.0f, 0.0f, Mathf.Max(this.position.width, 250.0f), Mathf.Max(this.position.height, 150.0f));
			Rect groupPos = new Rect(0.0f, position.center.y - 50.0f, position.width, 100.0f);
			
			GUILayout.BeginArea(groupPos);
			EditorGUILayout.LabelField("Could not find a <i>SaveUtility</i> instance!", _bigLabelStyle, GUILayout.Height(30.0f));
			Rect buttonPos = GUILayoutUtility.GetRect(0.0f, 20.0f, GUILayout.ExpandWidth(true));
			buttonPos.x = buttonPos.center.x - 75.0f;
			buttonPos.width = 150.0f;
			if(GUI.Button(buttonPos, "Create SaveUtility"))
			{
				_saveUtility = SaveUtilityClass.GetInstance(true);
			}
			
			GUILayout.EndArea();
		}
		
		private void DisplayNoSelectionWarning()
		{
			EditorGUI.LabelField(new Rect(0.0f, 0.0f, position.width, position.height), "No game object selected!", _bigLabelStyle);
		}
		
		private void DisplayMainGUI()
		{
			Rect position = new Rect(0.0f, 0.0f, Mathf.Max(this.position.width, 320.0f), Mathf.Max(this.position.height, 150.0f));
			Rect gameObjectSettingsArea = new Rect(position.center.x - 135.0f, 10.0f, 270.0f, 75.0f);
			Rect gameObjectAndChildrenSettingsArea = new Rect(gameObjectSettingsArea.x, gameObjectSettingsArea.yMax + 25.0f, 270.0f, 75.0f);
			Rect infoArea = new Rect(5.0f, Mathf.Max(position.yMax - 40.0f, gameObjectAndChildrenSettingsArea.yMax + 20.0f), 
									 position.width, 40.0f);
			Color initColor = GUI.color;
			
			
			GUILayout.BeginArea(gameObjectSettingsArea);
			EditorGUILayout.LabelField(_selection.name, _bigLabelStyle, GUILayout.Height(30.0f));
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Unique Identifier"))
			{
				AddUniqueIdentifier(_selection, false);
			}
			if(GUILayout.Button("GameObject Serializer"))
			{
				AddGameObjectSerializer(_selection, false);
			}
			EditorGUILayout.EndHorizontal();
			
			GUI.color = removeButtonColor;
			if(GUILayout.Button("Remove"))
			{
				Remove(_selection, false);
			}
			GUI.color = initColor;
			GUILayout.EndArea();
			
			GUILayout.BeginArea(gameObjectAndChildrenSettingsArea);
			EditorGUILayout.LabelField(_selection.name + " + Children", _bigLabelStyle, GUILayout.Height(30.0f));
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Unique Identifier"))
			{
				AddUniqueIdentifier(_selection, true);
			}
			if(GUILayout.Button("GameObject Serializer"))
			{
				AddGameObjectSerializer(_selection, true);
			}
			EditorGUILayout.EndHorizontal();
			
			GUI.color = removeButtonColor;
			if(GUILayout.Button("Remove"))
			{
				Remove(_selection, true);
			}
			GUI.color = initColor;
			GUILayout.EndArea();
			
			GUILayout.BeginArea(infoArea);
			EditorGUILayout.LabelField("Tracked Objects:             " + _saveUtility.GetGameObjectSerializerCount().ToString());
			HierarchyExtension.DrawIcons = EditorGUILayout.Toggle("Hierarchy Icons", HierarchyExtension.DrawIcons);
			GUILayout.EndArea();
		}
		
		private void AddUniqueIdentifier(GameObject gameObject, bool recursive)
		{
			UniqueIdentifier uid = gameObject.GetComponent<UniqueIdentifier>();
			if(uid == null)
			{
				gameObject.AddComponent<UniqueIdentifier>();
			}
			else
			{
				if(uid is GameObjectSerializer)
				{
					UnityEngine.Object.DestroyImmediate(uid);
					gameObject.AddComponent<UniqueIdentifier>();
				}
			}
			
			if(recursive)
			{
				foreach(Transform child in gameObject.transform)
				{
					AddUniqueIdentifier(child.gameObject, false);
				}
			}
		}
		
		private void AddGameObjectSerializer(GameObject gameObject, bool recursive)
		{
			UniqueIdentifier uid = gameObject.GetComponent<UniqueIdentifier>();
			if(uid == null)
			{
				gameObject.AddComponent<GameObjectSerializer>();
			}
			else
			{
				if(!(uid is GameObjectSerializer))
				{
					UnityEngine.Object.DestroyImmediate(uid);
					gameObject.AddComponent<GameObjectSerializer>();
				}
			}
			
			if(recursive)
			{
				foreach(Transform child in gameObject.transform)
				{
					AddGameObjectSerializer(child.gameObject, false);
				}
			}
		}
		
		private void Remove(GameObject gameObject, bool recursive)
		{
			UniqueIdentifier uid = gameObject.GetComponent<UniqueIdentifier>();
			if(uid != null)
			{
				UnityEngine.Object.DestroyImmediate(uid);
			}
			
			if(recursive)
			{
				foreach(Transform child in gameObject.transform)
				{
					Remove(child.gameObject, false);
				}
			}
		}
		
		[MenuItem("Team Utility/Save Utility/Tools/Save Wizard", false, 90)]
		private static void Open()
		{
			EditorWindow.GetWindow<SaveWizard>("Save Wizard");
		}
	}
}
