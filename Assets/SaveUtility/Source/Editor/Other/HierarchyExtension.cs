#region [Copyright (c) 2013 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2013 Cristian Alexandru Geambasu
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
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO.SaveUtility;

namespace TeamUtility.Editor.IO.SaveUtility
{
	[InitializeOnLoad]
	public sealed class HierarchyExtension : MonoBehaviour 
	{
		private static bool _drawIcons;
		private static Texture _uidIcon;
		private static Texture _gameObejctIcon;
		private static Texture _saveUtilityIcon;
		
		public static bool DrawIcons
		{
			get { return _drawIcons; }
			set
			{
				if(value != _drawIcons)
				{
					_drawIcons = value;
					EditorPrefs.SetBool("HierarchyExtension.drawIcons", _drawIcons);
					
					if(_drawIcons)
					{
						EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyItemGUI;
					}
					else
					{
						EditorApplication.hierarchyWindowItemOnGUI -= HandleHierarchyItemGUI;
					}
				}
			}
		}
		
		static HierarchyExtension()
		{
			_uidIcon = EditorGUIUtility.Load("SaveUtility/Icons/uid.png") as Texture;
			_gameObejctIcon = EditorGUIUtility.Load("SaveUtility/Icons/game_object.png") as Texture;
			_saveUtilityIcon = EditorGUIUtility.Load("SaveUtility/Icons/save_utility.png") as Texture;
			
			_drawIcons = EditorPrefs.GetBool("HierarchyExtension.drawIcons", false);
			if(_drawIcons)
			{
				EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyItemGUI;
			}
		}
		
		private static void HandleHierarchyItemGUI(int instanceID, Rect selectionRect)
		{
			Rect drawRect = selectionRect;
			drawRect.x = drawRect.xMax - (drawRect.height + 5.0f);
			
			GameObject gameObejct = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if(gameObejct != null)
			{
				if(gameObejct.GetComponent<TeamUtility.IO.SaveUtility.SaveUtility>() != null)
				{
					GUI.Label(drawRect, _saveUtilityIcon);
				}
				else if(gameObejct.GetComponent<GameObjectSerializer>() != null)
				{
					GUI.Label(drawRect, _gameObejctIcon);
				}
				else if(gameObejct.GetComponent<UniqueIdentifier>() != null)
				{
					GUI.Label(drawRect, _uidIcon);
				}
			}
		}
	}
}
