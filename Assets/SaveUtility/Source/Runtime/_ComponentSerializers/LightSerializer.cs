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
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	[CustomSerializer(typeof(Light))]
	public sealed class LightSerializer : IComponentSerializer
	{
		public Dictionary<string, object> Serialize(object value)
		{
			Light light = value as Light;
			Dictionary<string, object> dic = new Dictionary<string, object>();
			dic.Add("type", light.type);
			dic.Add("renderMode", light.renderMode);
			dic.Add("alreadyLightmapped", light.alreadyLightmapped);
			dic.Add("intensity", light.intensity);
			dic.Add("range", light.range);
			dic.Add("color", Convert.FromColor(light.color));
			dic.Add("spotAngle", light.spotAngle);
			dic.Add("shadows", light.shadows);
			dic.Add("shadowSoftness", light.shadowSoftness);
			dic.Add("shadowSoftnessFade", light.shadowSoftnessFade);
			dic.Add("shadowStrength", light.shadowStrength);
			
			return dic;
		}
		
		public void Deserialize(object instance, Dictionary<string, object> data)
		{
			Light light = instance as Light;
			light.type = Convert.ToEnum<LightType>(data["type"].ToString());
			light.renderMode = Convert.ToEnum<LightRenderMode>(data["renderMode"].ToString());
			light.alreadyLightmapped = (bool)data["alreadyLightmapped"];
			light.intensity = System.Convert.ToSingle(data["intensity"]);
			light.range = System.Convert.ToSingle(data["range"]);
			light.color = Convert.ToColor((Dictionary<string, object>)data["color"]);
			light.spotAngle = System.Convert.ToSingle(data["spotAngle"]);
			light.shadows = Convert.ToEnum<LightShadows>(data["shadows"].ToString());
			light.shadowSoftness = System.Convert.ToSingle(data["shadowSoftness"]);
			light.shadowSoftnessFade = System.Convert.ToSingle(data["shadowSoftnessFade"]);
			light.shadowStrength = System.Convert.ToSingle(data["shadowStrength"]);
		}
	}
}
