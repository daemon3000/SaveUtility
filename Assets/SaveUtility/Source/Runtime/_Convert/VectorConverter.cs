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
	[CustomSerializer(typeof(Vector2))]
	public sealed class Vector2Converter : ITypeConverter
	{
		public bool CanConvert(Type type)
		{
			return (type == typeof(Vector2));
		}
		
		public object ConvertFrom(object value)
		{
			return Convert.FromVector2((Vector2)value);
		}
		
		public object ConvertTo(object data)
		{
			return Convert.ToVector2((Dictionary<string, object>)data);
		}
	}
	
	[CustomSerializer(typeof(Vector3))]
	public sealed class Vector3Converter : ITypeConverter
	{
		public bool CanConvert(Type type)
		{
			return (type == typeof(Vector3));
		}
		
		public object ConvertFrom(object value)
		{
			return Convert.FromVector3((Vector3)value);
		}
		
		public object ConvertTo(object data)
		{
			return Convert.ToVector3((Dictionary<string, object>)data);
		}
	}
	
	[CustomSerializer(typeof(Vector4))]
	public sealed class Vector4Converter : ITypeConverter
	{
		public bool CanConvert(Type type)
		{
			return (type == typeof(Vector4));
		}
		
		public object ConvertFrom(object value)
		{
			return Convert.FromVector4((Vector4)value);
		}
		
		public object ConvertTo(object data)
		{
			return Convert.ToVector4((Dictionary<string, object>)data);
		}
	}
}
