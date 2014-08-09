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
using System;
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	[CustomSerializer(typeof(UnityEngine.Object))]
	public sealed class ObjectReferenceConverter : ITypeConverter
	{
		public bool CanConvert(Type type)
		{
			return (type == typeof(UnityEngine.Object));
		}
		
		public object ConvertFrom(object value)
		{
			SaveUtility saveUtility = SaveUtility.GetInstance(false);
			return saveUtility.GetAssetID((UnityEngine.Object)value);
		}
		
		public object ConvertTo(object data)
		{
			if(data == null) {
				return null;
			}
			
			SaveUtility saveUtility = SaveUtility.GetInstance(false);
			return saveUtility.GetAssetByID((string)data);
		}
	}
	
	[CustomSerializer(typeof(GameObject))]
	public sealed class GameObjectReferenceConverter : ITypeConverter
	{
		public bool CanConvert(Type type)
		{
			return (type == typeof(GameObject));
		}
		
		public object ConvertFrom(object value)
		{
			SaveUtility saveUtility = SaveUtility.GetInstance(false);
			GameObject gameObject = value as GameObject;
			if(gameObject != null)
			{
				UniqueIdentifier uid = gameObject.GetComponent<UniqueIdentifier>();
				if(uid != null && !string.IsNullOrEmpty(uid.ID))
				{
					return uid.ID;
				}
				else
				{
					return saveUtility.GetAssetID(gameObject);
				}
			}
			
			return null;
		}
		
		public object ConvertTo(object data)
		{
			if(data == null) {
				return null;
			}
			
			SaveUtility saveUtility = SaveUtility.GetInstance(false);
			GameObject gameObject = saveUtility.GetStoredGameObjectByID((string)data);
			if(gameObject == null)
			{
				gameObject = saveUtility.GetAssetByID((string)data) as GameObject;
			}
			
			return gameObject;
		}
	}
	
	[CustomSerializer(typeof(Component))]
	public sealed class ComponentReferenceConverter : ITypeConverter
	{
		public bool CanConvert(Type type)
		{
			return (type == typeof(Component));
		}
		
		public object ConvertFrom(object value)
		{
			Component component = value as Component;
			if(component != null)
			{
				UniqueIdentifier uid = component.GetComponent<UniqueIdentifier>();
				if(uid != null && !string.IsNullOrEmpty(uid.ID))
				{
					Dictionary<string, object> data = new Dictionary<string, object>
					{
						{"id", uid.ID},
						{"type", component.GetType().Name}
					};
					
					return data;
				}
			}
			
			return null;
		}
		
		public object ConvertTo(object data)
		{
			if(data == null) {
				return null;
			}
			
			SaveUtility saveUtility = SaveUtility.GetInstance(false);
			Dictionary<string, object> dic = data as Dictionary<string, object>;
			
			return saveUtility.GetStoredComponentByID((string)dic["id"], (string)dic["type"]);
		}
	}
}
