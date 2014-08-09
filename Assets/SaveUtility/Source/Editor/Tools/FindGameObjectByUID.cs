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
	public sealed class FindGameObjectByUID : EditorWindow
	{
		[SerializeField] private string _uid = string.Empty;
		
		private void OnGUI()
		{
			_uid = EditorGUILayout.TextField("UID", _uid);
			EditorGUILayout.Space();
			
			Rect position = GUILayoutUtility.GetRect(0.0f, 20.0f, GUILayout.ExpandWidth(true));
			position.x = position.center.x - 75.0f;
			position.width = 150.0f;
			
			GUI.enabled = (_uid.Length > 0);
			if(GUI.Button(position, "Find"))
			{
				UniqueIdentifier result = FindObject();
				if(result != null)
				{
					Selection.activeGameObject = result.gameObject;
				}
				else
				{
					EditorUtility.DisplayDialog("No match", "Unable to find an object with the requested ID.", "OK");
				}
			}
			GUI.enabled = true;
		}
		
		private UniqueIdentifier FindObject()
		{
			UniqueIdentifier[] ids = UnityEngine.Object.FindObjectsOfType(typeof(UniqueIdentifier)) as UniqueIdentifier[];
			foreach(var id in ids)
			{
				if(String.Compare(_uid, id.ID, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					return id;
				}
			}
			
			return null;
		}
		
		[MenuItem("Team Utility/Save Utility/Tools/Find Object By UID", false, 90)]
		public static void Init()
		{
			EditorWindow.GetWindow<FindGameObjectByUID>("Find");
		}
	}
}
