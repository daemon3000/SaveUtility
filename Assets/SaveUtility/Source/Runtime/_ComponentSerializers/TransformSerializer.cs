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
using System;
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	[CustomSerializer(typeof(Transform))]
	public sealed class TransformSerializer : IComponentSerializer
	{
		private const string invalidParentWarning = "You are trying to save a transform whose parent has no unique identifier. This can result in a wrong position and/or rotation when loading the save.";
		
		public Dictionary<string, object> Serialize(object value)
		{
			Transform transform = value as Transform;
			Dictionary<string, object> data = new Dictionary<string, object>();
			data.Add("localPosition", Convert.FromVector3(transform.localPosition));
			data.Add("localRotation", Convert.FromQuaternion(transform.localRotation));
			data.Add("localScale", Convert.FromVector3(transform.localScale));
			if(transform.parent == null) 
			{
				data.Add("parent", null);
			}
			else
			{
				UniqueIdentifier uid = transform.parent.GetComponent<UniqueIdentifier>();
				if(uid != null) {
					data.Add("parent", uid.ID);
				}
				else {
#if UNITY_EDITOR || SAVEUTILITY_DEVBUILD
					Debug.LogWarning(invalidParentWarning);
#endif
					data.Add("parent", null);
				}
			}
			
			return data;
		}
		
		public void Deserialize(object instance, Dictionary<string, object> data)
		{
			Transform transform = instance as Transform;
			
			if(data["parent"] != null) {
				SaveUtility saveUtility = SaveUtility.GetInstance(false);
				transform.parent = saveUtility.GetStoredComponentByID<Transform>((string)data["parent"]);
			}
			else {
				transform.parent = null;
			}
			
			transform.localRotation = Convert.ToQuaternion((Dictionary<string, object>)data["localRotation"]);
			transform.localPosition = Convert.ToVector3((Dictionary<string, object>)data["localPosition"]);
			transform.localScale = Convert.ToVector3((Dictionary<string, object>)data["localScale"]);
		}
	}
}
