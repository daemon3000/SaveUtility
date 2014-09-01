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
using System.Collections.Generic;
using TeamUtility.IO.SaveUtility;

namespace TeamUtility.Editor.IO.SaveUtility
{
	[CustomEditor(typeof(UniqueIdentifier))]
	public class UniqueIdentifierEditor : UnityEditor.Editor
	{
		protected SerializedProperty _id;
		protected bool _hasPrefab = false;
		protected bool _isPeristent = false;

		protected virtual void OnEnable()
		{
			_id = serializedObject.FindProperty("_id");

			var targetRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(((UniqueIdentifier)target).gameObject);
			_hasPrefab = PrefabUtility.GetPrefabParent(targetRoot) != null;
			_isPeristent = EditorUtility.IsPersistent(target);
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

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
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

			serializedObject.ApplyModifiedProperties();
		}

		protected void Duplicate()
		{
			GameObject targetGameObject = ((UniqueIdentifier)target).gameObject;
			GameObject targetRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(targetGameObject);
			GameObject duplicate = null;
			
			if(targetRoot != null && targetRoot == targetGameObject)
			{
				GameObject prefab = PrefabUtility.GetPrefabParent(targetRoot) as GameObject;
				duplicate = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

				UniqueIdentifier[] identifiers = duplicate.GetComponentsInChildren<UniqueIdentifier>();
				for(int i = 0; i < identifiers.Length; i++)
				{
					identifiers[i].ChacheID();
				}

				PropertyModification[] pm = RemoveIDModifications(PrefabUtility.GetPropertyModifications(targetRoot));
				PrefabUtility.SetPropertyModifications(duplicate, pm);
			}
			else
			{
				duplicate = GameObject.Instantiate(targetGameObject) as GameObject;
				
				UniqueIdentifier[] identifiers = duplicate.GetComponentsInChildren<UniqueIdentifier>();
				for(int i = 0; i < identifiers.Length; i++)
				{
					if(!(identifiers[i] is GameObjectSerializer))
					{
						identifiers[i].GenerateNewID();
					}
				}
			}

			duplicate.transform.parent = targetGameObject.transform.parent;
			duplicate.name = targetGameObject.name;
			Selection.activeGameObject = duplicate;
		}

		private PropertyModification[] RemoveIDModifications(PropertyModification[] pm)
		{
			List<PropertyModification> modif = new List<PropertyModification>(pm.Length + 1);
			for(int i = 0; i < pm.Length; i++)
			{
				Type targetType = pm[i].target.GetType();
				if((targetType != typeof(UniqueIdentifier) && targetType != typeof(GameObjectSerializer)) || pm[i].propertyPath != "_id")
				{
					modif.Add(pm[i]);
				}
			}

			return modif.ToArray();
		}

		protected void RevertInstanceChanges()
		{
			var targetRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(((UniqueIdentifier)target).gameObject);
			
			UniqueIdentifier[] identifiers = targetRoot.GetComponentsInChildren<UniqueIdentifier>();
			for(int i = 0; i < identifiers.Length; i++)
			{
				identifiers[i].ChacheID();
			}
			
			PrefabUtility.RevertPrefabInstance(targetRoot);
		}

		protected void ApplyChangesToPrefab()
		{
			var targetRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(((UniqueIdentifier)target).gameObject);
			
			UniqueIdentifier[] identifiers = targetRoot.GetComponentsInChildren<UniqueIdentifier>();
			for(int i = 0; i < identifiers.Length; i++)
			{
				identifiers[i].ChacheID();
			}
			
			GameObject prefab = PrefabUtility.GetPrefabParent(targetRoot) as GameObject;
			prefab = PrefabUtility.ReplacePrefab(targetRoot, prefab, ReplacePrefabOptions.ConnectToPrefab);
			
			UniqueIdentifier[] prefabIdentifiers = prefab.GetComponentsInChildren<UniqueIdentifier>(true);
			for(int i = 0; i < prefabIdentifiers.Length; i++)
			{
				prefabIdentifiers[i].ClearID();
			}
			
			AssetDatabase.SaveAssets();
		}
	}
}
