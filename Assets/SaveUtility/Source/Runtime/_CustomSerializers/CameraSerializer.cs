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
using System;
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	[CustomSerializer(typeof(Camera))]
	public sealed class CameraSerializer : IComponentSerializer
	{
		public Dictionary<string, object> Serialize(object value)
		{
			Camera camera = value as Camera;
			Dictionary<string, object> dic = new Dictionary<string, object>();
			dic.Add("enabled", camera.enabled);
			dic.Add("orthographic", camera.orthographic);
			dic.Add("orthographicSize", camera.orthographicSize);
			dic.Add("fieldOfView", camera.fieldOfView);
			dic.Add("nearClipPlane", camera.nearClipPlane);
			dic.Add("farClipPlane", camera.farClipPlane);
			dic.Add("depth", camera.depth);
			
			return dic;
		}
		
		public void Deserialize(object instance, Dictionary<string, object> data)
		{
			Camera camera = instance as Camera;
			camera.orthographic = (bool)data["orthographic"];
			camera.orthographicSize = System.Convert.ToSingle(data["orthographicSize"]);
			camera.fieldOfView = System.Convert.ToSingle(data["fieldOfView"]);
			camera.nearClipPlane = System.Convert.ToSingle(data["nearClipPlane"]);
			camera.farClipPlane = System.Convert.ToSingle(data["farClipPlane"]);
			camera.depth = System.Convert.ToInt32(data["depth"]);
			camera.enabled = (bool)data["enabled"];
		}
	}
}
