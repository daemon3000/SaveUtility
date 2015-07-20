#region [Copyright (c) 2015 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2015 Cristian Alexandru Geambasu
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
	public static class MenuCommands
	{
		[MenuItem("Team Utility/Save Utility/Create Save Utility", false, 0)]
		public static void CreateSaveUtility()
		{
			var saveUtility = TeamUtility.IO.SaveUtility.SaveUtility.GetInstance();
			if(saveUtility == null)
				saveUtility = TeamUtility.IO.SaveUtility.SaveUtility.CreateInstance();

			Selection.activeObject = saveUtility;
		}
		
		[MenuItem("Team Utility/Save Utility/Add GameObject Serializer %#g", true, 0)]
		public static bool ValidateAddGameObjectSerializer()
		{
			GameObject activeGO = Selection.activeGameObject;
			if(activeGO == null || activeGO.GetComponent<GameObjectSerializer>() != null)  {
				return false;
			}
			else {
				return true;
			}
		}
		
		[MenuItem("Team Utility/Save Utility/Add GameObject Serializer %#g", false, 0)]
		public static void AddGameObjectSerializer()
		{
			GameObject gameObject = Selection.activeGameObject;
			if(gameObject != null)
			{
				UniqueIdentifier uid = gameObject.GetComponent<UniqueIdentifier>();
				if(uid == null)
				{
					gameObject.AddComponent<GameObjectSerializer>();
				}
				else
				{
					if(uid is GameObjectSerializer)
					{
						UnityEngine.Object.DestroyImmediate(uid);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(uid);
						gameObject.AddComponent<GameObjectSerializer>();
					}
				}
			}
		}
		
		[MenuItem("Team Utility/Save Utility/Add Unique Identifier %#u", true, 0)]
		public static bool ValidateAddUniqueIdentifier()
		{
			GameObject activeGO = Selection.activeGameObject;
			if(activeGO == null || activeGO.GetComponent<UniqueIdentifier>() != null)  {
				return false;
			}
			else {
				return true;
			}
		}
		
		[MenuItem("Team Utility/Save Utility/Add Unique Identifier %#u", false, 0)]
		public static void AddUniqueIdentifier()
		{
			GameObject gameObject = Selection.activeGameObject;
			if(gameObject != null)
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
					else
					{
						UnityEngine.Object.DestroyImmediate(uid);
					}
				}
			}
		}
		
		[MenuItem("Team Utility/Save Utility/Add To Required Assets", true, 0)]
		private static bool ValidateRequirePrefab()
		{
			return (TeamUtility.IO.SaveUtility.SaveUtility.GetInstance() != null && Selection.objects.Length > 0);
		}
		
		[MenuItem("Team Utility/Save Utility/Add To Required Assets", false, 0)]
		public static void RequirePrefab()
		{
			TeamUtility.IO.SaveUtility.SaveUtility saveUtility = TeamUtility.IO.SaveUtility.SaveUtility.GetInstance();
			if(saveUtility != null) 
			{
				foreach(UnityEngine.Object asset in Selection.objects)
				{
					saveUtility.AddRequiredAsset(asset);
				}
			}
		}
		
		[MenuItem("Team Utility/Save Utility/About", false, 120)]
		private static void About()
		{
			string about = string.Format("SaveUtility, MIT licensed\nCopyright \u00A9 2015 Cristian Alexandru Geambasu\nhttps://bitbucket.org/daemon3000/saveutility-for-unity3d");
			EditorUtility.DisplayDialog("About", about, "OK");
		}
	}
}
