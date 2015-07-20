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
using SystemConvert = System.Convert;
#if UNITY_WINRT
using System.Reflection;
#endif

namespace TeamUtility.IO.SaveUtility
{
	public static class Convert
	{
		public static Dictionary<string, object> FromVector2(Vector2 value)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict.Add("x", value.x);
			dict.Add("y", value.y);
			
			return dict;
		}
		
		public static Vector2 ToVector2(Dictionary<string, object> value)
		{
			return new Vector2(SystemConvert.ToSingle(value["x"]), SystemConvert.ToSingle(value["y"]));
		}
		
		public static Dictionary<string, object> FromVector3(Vector3 value)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict.Add("x", value.x);
			dict.Add("y", value.y);
			dict.Add("z", value.z);
			
			return dict;
		}
		
		public static Vector3 ToVector3(Dictionary<string, object> value)
		{
			return new Vector3(SystemConvert.ToSingle(value["x"]), SystemConvert.ToSingle(value["y"]),
								SystemConvert.ToSingle(value["z"]));
		}
		
		public static Dictionary<string, object> FromVector4(Vector4 value)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict.Add("x", value.x);
			dict.Add("y", value.y);
			dict.Add("z", value.z);
			dict.Add("w", value.w);
			
			return dict;
		}
		
		public static Vector4 ToVector4(Dictionary<string, object> value)
		{
			return new Vector4(SystemConvert.ToSingle(value["x"]), SystemConvert.ToSingle(value["y"]),
								SystemConvert.ToSingle(value["z"]), SystemConvert.ToSingle(value["w"]));
		}
		
		public static Dictionary<string, object> FromQuaternion(Quaternion value)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict.Add("x", value.x);
			dict.Add("y", value.y);
			dict.Add("z", value.z);
			dict.Add("w", value.w);
			
			return dict;
		}
		
		public static Quaternion ToQuaternion(Dictionary<string, object> value)
		{
			return new Quaternion(SystemConvert.ToSingle(value["x"]), SystemConvert.ToSingle(value["y"]),
								SystemConvert.ToSingle(value["z"]), SystemConvert.ToSingle(value["w"]));
		}
		
		public static Dictionary<string, object> FromColor(Color value)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict.Add("r", value.r);
			dict.Add("g", value.g);
			dict.Add("b", value.b);
			dict.Add("a", value.a);
			
			return dict;
		}
		
		public static Color ToColor(Dictionary<string, object> value)
		{
			return new Color(SystemConvert.ToSingle(value["r"]), SystemConvert.ToSingle(value["g"]),
							SystemConvert.ToSingle(value["b"]), SystemConvert.ToSingle(value["a"]));
		}
		
		public static Dictionary<string, object> FromColor32(Color32 value)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict.Add("r", value.r);
			dict.Add("g", value.g);
			dict.Add("b", value.b);
			dict.Add("a", value.a);
			
			return dict;
		}
		
		public static Color32 ToColor32(Dictionary<string, object> value)
		{
			return new Color32(SystemConvert.ToByte(value["r"]), SystemConvert.ToByte(value["g"]),
							SystemConvert.ToByte(value["b"]), SystemConvert.ToByte(value["a"]));
		}
		
		public static Dictionary<string, object> FromRect(Rect value)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict.Add("x", value.x);
			dict.Add("y", value.y);
			dict.Add("width", value.width);
			dict.Add("height", value.height);
			
			return dict;
		}
		
		public static Rect ToRect(Dictionary<string, object> value)
		{
			return new Rect(SystemConvert.ToSingle(value["x"]), SystemConvert.ToSingle(value["y"]),
							SystemConvert.ToSingle(value["width"]), SystemConvert.ToSingle(value["height"]));
		}
		
		public static T ToEnum<T>(object value) where T : struct
		{
			return (T)ToEnum(value.ToString(), typeof(T));
		}
		
		public static T ToEnum<T>(string value) where T : struct
		{
			return (T)ToEnum(value, typeof(T));
		}
		
		public static object ToEnum(object value, Type enumType)
		{
			return ToEnum(value.ToString(), enumType);
		}
		
		public static object ToEnum(string value, Type enumType)
		{
			if(!IsEnum(enumType))
				throw new ArgumentException("The type you are trying to cast to is not an enumeration.", "value");

			if(string.IsNullOrEmpty(value)) {
				return 0;
			}
			try {
				return Enum.Parse(enumType, value, true);
			}
			catch {
				string message = string.Format("Cannot cast string to {0}.", enumType.Name);
				throw new InvalidCastException(message);
			}
		}

		private static bool IsEnum(Type type)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			TypeInfo typeInfo = type.GetTypeInfo();
			return typeInfo.IsEnum;
#else
			return type.IsEnum;
#endif
		}
	}
}
