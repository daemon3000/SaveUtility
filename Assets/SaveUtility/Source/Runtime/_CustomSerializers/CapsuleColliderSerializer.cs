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
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	[CustomSerializer(typeof(CapsuleCollider))]
	public sealed class CapsuleColliderSerializer : IComponentSerializer
	{
		public Dictionary<string, object> Serialize(object value)
		{
			CapsuleCollider collider = value as CapsuleCollider;
			Dictionary<string, object> dic = new Dictionary<string, object>();
			dic.Add("enabled", collider.enabled);
			dic.Add("isTrigger", collider.isTrigger);
			dic.Add("center", Convert.FromVector3(collider.center));
			dic.Add("radius", collider.radius);
			dic.Add("height", collider.height);
			dic.Add("direction", collider.direction);
			
			return dic;
		}
		
		public void Deserialize(object instance, Dictionary<string, object> data)
		{
			CapsuleCollider collider = instance as CapsuleCollider;
			collider.isTrigger = (bool)data["isTrigger"];
			collider.center = Convert.ToVector3((Dictionary<string, object>)data["center"]);
			collider.radius = System.Convert.ToSingle(data["radius"]);
			collider.height = System.Convert.ToSingle(data["height"]);
			collider.direction = System.Convert.ToInt32(data["direction"]);
			collider.enabled = (bool)data["enabled"];
		}
	}
}
