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

namespace TeamUtility.Editor.IO.SaveUtility
{
	[CustomEditor(typeof(TeamUtility.IO.SaveUtility.SaveUtility))]
	public sealed class SaveUtilityEditor : UnityEditor.Editor 
	{
		private TeamUtility.IO.SaveUtility.SaveUtility _saveUtility;
		private SerializedProperty _requiredAssets;
		private SerializedProperty _serializers;
		private int _toolbarSelection = 0;
		private string[] _toolbarOptions = new string[] { "Tracked Objects", "Required Assets" };
		
		private void OnEnable()
		{
			_requiredAssets = serializedObject.FindProperty("_requiredAssets");
			_serializers = serializedObject.FindProperty("_serializers");
			_saveUtility = target as TeamUtility.IO.SaveUtility.SaveUtility;
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.Space();
			_toolbarSelection = GUILayout.Toolbar(_toolbarSelection, _toolbarOptions, GUILayout.ExpandWidth(true));
			if(_toolbarSelection == 0)
			{
				DisplayTrackedObjects();
			}
			else
			{
				DisplayRequiredPrefabs();
			}
			
			EditorGUILayout.Space();
			serializedObject.ApplyModifiedProperties();
		}
		
		private void DisplayTrackedObjects()
		{
			EditorGUILayout.Space();
			for(int i = 0; i < _serializers.arraySize; i++)
			{
				SerializedProperty prop = _serializers.GetArrayElementAtIndex(i);
				GameObjectSerializer serializer = prop.objectReferenceValue as GameObjectSerializer;
				if(serializer == null) {
					EditorGUILayout.LabelField("Object reference is not a GameObjectSerializer");
					GUI.color = Color.red;
					if(GUILayout.Button("Remove"))
					{
						_serializers.DeleteArrayElementAtIndex(i);
					}
					GUI.color = Color.white;
					continue;
				}
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(string.Format("{0} - {1}", serializer.name, serializer.ID));
				if(GUILayout.Button("Select"))
				{
					Selection.activeGameObject = serializer.gameObject;
				}
				if(GUILayout.Button("Up") && i > 0)
				{
					_serializers.MoveArrayElement(i, i - 1);
				}
				if(GUILayout.Button("Down") && i < _serializers.arraySize - 1)
				{
					_serializers.MoveArrayElement(i, i + 1);
				}
				if(GUILayout.Button("Top") && i > 0)
				{
					_serializers.MoveArrayElement(i, 0);
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		
		private void DisplayRequiredPrefabs()
		{
			EditorGUILayout.Space();
			for(int i = 0; i < _requiredAssets.arraySize; i++)
			{
				SerializedProperty asset = _requiredAssets.GetArrayElementAtIndex(i).FindPropertyRelative("asset");
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.ObjectField(asset.objectReferenceValue, typeof(UnityEngine.Object), false);
				if(GUILayout.Button("Remove", GUILayout.Width(24.0f)))
				{
					_requiredAssets.DeleteArrayElementAtIndex(i);
					i--;
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.Space();
			
			Rect dragAndDropArea = GUILayoutUtility.GetRect(0.0f, 0.0f, GUILayout.ExpandWidth(true), GUILayout.Height(50.0f));
			dragAndDropArea.x += 5.0f;
			dragAndDropArea.width -= 10.0f;
			EditorToolbox.DragAndDropArea(dragAndDropArea, "Drop assets here", ProcessDragAndDrop);
		}
		
		private void ProcessDragAndDrop(UnityEngine.Object obj)
		{
			if(_saveUtility.IsAssetValidForRegistration(obj))
			{
				_requiredAssets.arraySize++;
				SerializedProperty lastItem = _requiredAssets.GetArrayElementAtIndex(_requiredAssets.arraySize - 1);
				
				SerializedProperty id = lastItem.FindPropertyRelative("id");
				id.stringValue = UniqueIdentifier.GetUniqueID();
				
				SerializedProperty asset = lastItem.FindPropertyRelative("asset");
				asset.objectReferenceValue = obj;
			}
		}
	}
}
