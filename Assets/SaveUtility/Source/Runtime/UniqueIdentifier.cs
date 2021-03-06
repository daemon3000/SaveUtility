﻿#region [Copyright (c) 2015 Cristian Alexandru Geambasu]
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
using System;
using System.Collections;

namespace TeamUtility.IO.SaveUtility
{
	[ExecuteInEditMode]
	public class UniqueIdentifier : MonoBehaviour
	{
		[SerializeField] protected string _id;
#if UNITY_EDITOR
		private string _chachedID = null;
		private bool _isIDChached = false;
#endif

		public string ID 
		{ 
			get { return _id; } 
		}
		
		protected virtual void OnEnable()
		{
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
			{
				if(string.IsNullOrEmpty(_id)) 
				{
					if(_isIDChached)
					{
						_id = _chachedID;
						_isIDChached = false;
					}
					else
					{
						_id = GetUniqueID();
#if SAVEUTILITY_DEVBUILD
						Debug.Log(string.Format("No ID on {0}: {1}. New ID: {2}", GetType().Name, GetInstanceID(), _id));
#endif
					}
				}
			}
#endif
		}

#if UNITY_EDITOR
		public void ClearID()
		{
			_id = null;
		}

		public void ChacheID()
		{
			if(!_isIDChached)
			{
				_chachedID = _id;
				_isIDChached = true;
			}
		}

		public void GenerateNewID()
		{
			_id = GetUniqueID();
		}

		[ContextMenu("Copy ID")]
		private void CopyIDToSystemBuffer()
		{
			UnityEditor.EditorGUIUtility.systemCopyBuffer = _id;
		}
#endif
		
		public static string GetUniqueID()
		{
			return Guid.NewGuid().ToString();
		}
	}
}
