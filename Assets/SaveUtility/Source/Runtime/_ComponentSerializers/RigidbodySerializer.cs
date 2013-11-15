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
	[CustomSerializer(typeof(Rigidbody))]
	public sealed class RigidbodySerializer : IComponentSerializer
	{
		public Dictionary<string, object> Serialize (object value)
		{
			Rigidbody body = value as Rigidbody;
			Dictionary<string, object> dic = new Dictionary<string, object>();
			dic.Add("isKinematic", body.isKinematic);
			dic.Add("freezeRotation", body.freezeRotation);
			dic.Add("useGravity", body.useGravity);
			dic.Add("detectCollisions", body.detectCollisions);
			dic.Add("useConeFriction", body.useConeFriction);
			dic.Add("drag", body.drag);
			dic.Add("angularDrag", body.angularDrag);
			dic.Add("mass", body.mass);
			dic.Add("rotation", Convert.FromQuaternion(body.rotation));
			dic.Add("sleepVelocity", body.sleepVelocity);
			dic.Add("sleepAngularVelocity", body.sleepAngularVelocity);
			dic.Add("maxAngularVelocity", body.maxAngularVelocity);
			dic.Add("constraints", body.constraints);
			dic.Add("collisionDetectionMode", body.collisionDetectionMode);
			dic.Add("interpolation", body.interpolation);
			dic.Add("solverIterationCount", body.solverIterationCount);
			dic.Add("velocity", Convert.FromVector3(body.velocity));
			dic.Add("angularVelocity", Convert.FromVector3(body.angularVelocity));
			dic.Add("inertiaTensor", Convert.FromVector3(body.inertiaTensor));
			dic.Add("inertiaTensorRotation", Convert.FromQuaternion(body.inertiaTensorRotation));
			
			return dic;
		}
		
		public void Deserialize (object instance, Dictionary<string, object> data)
		{
			Rigidbody body = instance as Rigidbody;
			body.isKinematic = true;
			body.freezeRotation = (bool)data["freezeRotation"];
			body.useGravity = (bool)data["useGravity"];
			body.detectCollisions = (bool)data["detectCollisions"];
			body.useConeFriction = (bool)data["useConeFriction"];
			body.drag = System.Convert.ToSingle(data["drag"]);
			body.angularDrag = System.Convert.ToSingle(data["angularDrag"]);
			body.mass = System.Convert.ToSingle(data["mass"]);
			body.rotation = Convert.ToQuaternion((Dictionary<string, object>)data["rotation"]);
			body.sleepVelocity = System.Convert.ToSingle(data["sleepVelocity"]);
			body.sleepAngularVelocity = System.Convert.ToSingle(data["sleepAngularVelocity"]);
			body.maxAngularVelocity = System.Convert.ToSingle(data["maxAngularVelocity"]);
			body.constraints = Convert.ToEnum<RigidbodyConstraints>(data["constraints"].ToString());
			body.collisionDetectionMode = Convert.ToEnum<CollisionDetectionMode>(data["collisionDetectionMode"].ToString());
			body.interpolation = Convert.ToEnum<RigidbodyInterpolation>(data["interpolation"].ToString());
			body.solverIterationCount = System.Convert.ToInt32(data["solverIterationCount"]);
			body.isKinematic = (bool)data["isKinematic"];
			if(!body.isKinematic)
			{
				body.velocity = Convert.ToVector3((Dictionary<string, object>)data["velocity"]);
				body.angularVelocity = Convert.ToVector3((Dictionary<string, object>)data["angularVelocity"]);
				body.inertiaTensor = Convert.ToVector3((Dictionary<string, object>)data["inertiaTensor"]);
				body.inertiaTensorRotation = Convert.ToQuaternion((Dictionary<string, object>)data["inertiaTensorRotation"]);
			}
		}
	}
}
