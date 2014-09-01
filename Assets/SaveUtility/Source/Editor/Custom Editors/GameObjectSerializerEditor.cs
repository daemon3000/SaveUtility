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
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO.SaveUtility;

namespace TeamUtility.Editor.IO.SaveUtility
{
	[CustomEditor(typeof(GameObjectSerializer))]
	public sealed class GameObjectSerializerEditor : UniqueIdentifierEditor
	{
		private SerializedProperty _serializableComponents;
		
		protected override void OnEnable()
		{
			base.OnEnable();
			_serializableComponents = serializedObject.FindProperty("_serializableComponents");
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.Space();
			if(!_isPeristent && _hasPrefab)
			{
				EditorGUILayout.HelpBox("Do not use the Revert and Apply buttons located at the top of the inspector. Use the buttons located below.", MessageType.Warning);
				EditorGUILayout.Space();
			}
			GUI.enabled = false;
			EditorGUILayout.PropertyField(_id);
			GUI.enabled = true;
			DisplayComponentList();
			serializedObject.ApplyModifiedProperties();
		}
		
		private void DisplayComponentList()
		{
			GameObjectSerializer target = this.target as GameObjectSerializer;
			List<GameObjectSerializer.ComponentStatusPair> currentComponents = target.GetComponentList();
			IEnumerable<Component> components = from c in target.GetComponents<Component>()
												where GameObjectSerializer.IsComponentSerializable(c) select c;
			
			int newSize = components.Count();
			if(newSize > currentComponents.Count)
			{
				foreach(Component c in components)
				{
					if(!Contains(currentComponents, c)) {
						currentComponents.Add(new GameObjectSerializer.ComponentStatusPair(true, c));
					}
				}
			}
			else if(newSize < currentComponents.Count)
			{
				for(int i = 0; i < currentComponents.Count; i++)
				{
					if(!components.Contains(currentComponents[i].component)) {
						currentComponents.RemoveAt(i);
						i--;
					}
				}
			}

			_serializableComponents.isExpanded = EditorGUILayout.Foldout(_serializableComponents.isExpanded, "Serializable Components");
			if(_serializableComponents.isExpanded)
			{
				EditorGUI.indentLevel++;
				foreach(var pair in currentComponents)
				{
					pair.serialize = EditorGUILayout.Toggle(pair.component.GetType().Name, pair.serialize);
				}
				EditorGUI.indentLevel--;
			}

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Select\nAll", GUILayout.Height(32)))
			{
				foreach(var pair in currentComponents)
				{
					pair.serialize = true;
				}
			}
			if(GUILayout.Button("Deselect\nAll", GUILayout.Height(32)))
			{
				foreach(var pair in currentComponents)
				{
					pair.serialize = false;
				}
			}
			GUI.enabled = !EditorApplication.isPlaying && !_isPeristent;
			if(GUILayout.Button("Duplicate", GUILayout.Height(32)))
			{
				Duplicate();
			}
			GUI.enabled = _hasPrefab && !EditorApplication.isPlaying && !_isPeristent;
			if(GUILayout.Button("Revert\nChanges", GUILayout.Height(32)))
			{
				RevertInstanceChanges();
			}
			if(GUILayout.Button("Apply\nChanges", GUILayout.Height(32)))
			{
				ApplyChangesToPrefab();
			}
			GUI.enabled = false;
			EditorGUILayout.EndHorizontal();
		}

		private bool Contains(List<GameObjectSerializer.ComponentStatusPair> list, Component c)
		{
			for(int i = 0; i < list.Count; i++)
			{
				if(list[i].component == c)
					return true;
			}
			
			return false;
		}
	}
}
