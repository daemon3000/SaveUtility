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
	[CustomSerializer(typeof(Animation))]
	public sealed class AnimationSerializer : IComponentSerializer
	{
		public Dictionary<string, object> Serialize(object value)
		{
			Animation animation = value as Animation;
			Dictionary<string, object> data = new Dictionary<string, object>();
			
			data.Add("enabled", animation.enabled);
			data.Add("isPlaying", animation.isPlaying);
			foreach(AnimationState animState in animation)
			{
				Dictionary<string, object> stateData = new Dictionary<string, object>();
				stateData.Add("blendMode", animState.blendMode);
				stateData.Add("layer", animState.layer);
				stateData.Add("speed", animState.speed);
				stateData.Add("time", animState.time);
				stateData.Add("weight", animState.weight);
				stateData.Add("wrapMode", animState.wrapMode);
				stateData.Add("enabled", animState.enabled);
				
				data.Add(animState.name, stateData);
			}
			
			return data;
		}
		
		public void Deserialize(object instance, Dictionary<string, object> data)
		{
			Animation animation = instance as Animation;
			foreach(AnimationState animState in animation)
			{
				Dictionary<string, object> stateData = (Dictionary<string, object>)data[animState.name];
				animState.blendMode = Convert.ToEnum<AnimationBlendMode>(stateData["blendMode"]);
				animState.layer = System.Convert.ToInt32(stateData["layer"]);
				animState.speed = System.Convert.ToSingle(stateData["speed"]);
				animState.time = System.Convert.ToSingle(stateData["time"]);
				animState.weight = System.Convert.ToSingle(stateData["weight"]);
				animState.wrapMode = Convert.ToEnum<WrapMode>(stateData["wrapMode"]);
				animState.enabled = (bool)stateData["enabled"];
			}
			
			bool isPlaying = (bool)data["isPlaying"];
			bool enabled = (bool)data["enabled"];
			if(isPlaying) 
			{
				animation.Play();
			}
			if(animation.enabled != enabled)
			{
				animation.enabled = enabled;
			}
		}
	}
}
