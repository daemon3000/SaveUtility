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
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	[ExecuteInEditMode]
	public sealed class GameObjectSerializer : UniqueIdentifier
	{
		#region [ComponentStatusPair]
		[Serializable]
		public class ComponentStatusPair
		{
			public bool serialize;
			public Component component;
			
			public ComponentStatusPair() { }
			public ComponentStatusPair(bool serialize, Component component)
			{
				this.serialize = serialize;
				this.component = component;
			}
		}
		#endregion
		
		[SerializeField] private bool _storeAllComponents = true;
		[SerializeField] private List<ComponentStatusPair> _serializableComponents;
		private static Dictionary<Type, Type> _customSerializerTable;
		
		private void Awake()
		{
			SaveUtility saveUtility = SaveUtility.GetInstance(true);
			saveUtility.AddGameObjectSerializer(this);
			
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			if(_customSerializerTable == null) {
				BuildCustomSerializerTable();
			}
		}
		
		protected override void OnEnable()
		{
			base.OnEnable();
			
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			if(_serializableComponents == null) {
				_serializableComponents = new List<ComponentStatusPair>();
			}
		}
		
		private void OnDestroy()
		{
			SaveUtility saveUtility = SaveUtility.GetInstance(false);
			if(saveUtility != null) {
				saveUtility.RemoveGameObjectSerializer(this);
			}
		}
		
		private void BuildCustomSerializerTable()
		{
			if(_customSerializerTable == null) {
				_customSerializerTable = new Dictionary<Type, Type>();
			}
			else {
				_customSerializerTable.Clear();
			}
			
			foreach(Type typeOfSerializer in GetTypesWithCustomSerializerAttribute())
			{
				object[] attributes = typeOfSerializer.GetCustomAttributes(typeof(CustomSerializerAttribute), false);
				CustomSerializerAttribute attribute = attributes[0] as CustomSerializerAttribute;
				
				if(!_customSerializerTable.ContainsKey(attribute.typeToSerialize)) {
					_customSerializerTable.Add(attribute.typeToSerialize, typeOfSerializer);
				}
			}
		}
		
		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> data = new Dictionary<string, object>();
			data.Add("name", gameObject.name);
			data.Add("tag", gameObject.tag);
			data.Add("layer", gameObject.layer);
			data.Add("activeSelf", gameObject.activeSelf);
			
			foreach(ComponentStatusPair pair in _serializableComponents)
			{
				if(!_storeAllComponents && !pair.serialize)
					continue;
				
				Component component = pair.component;
				Type componentType = component.GetType();
				
				if(component is ISerializableMonoBehaviour)
				{
					data.Add(componentType.FullName, ((ISerializableMonoBehaviour)component).Serialize());
				}
				else if(_customSerializerTable.ContainsKey(componentType))
				{
					Type serializerType = _customSerializerTable[componentType];
					MethodInfo mi = serializerType.GetMethod("Serialize");
					if(mi != null)
					{
						object instance = Activator.CreateInstance(serializerType);
						object[] parameters = new object[] { component };
						object componentData = mi.Invoke(instance, parameters);
						
						data.Add(componentType.FullName, componentData);
					}
				}
			}
			
			return data;
		}
		
		public void Deserialize(Dictionary<string, object> data)
		{
			foreach(ComponentStatusPair pair in _serializableComponents)
			{
				if(!data.ContainsKey(pair.component.GetType().FullName))
					continue;
				
				Component component = pair.component;
				Type componentType = component.GetType();
				Dictionary<string, object> componentData = (Dictionary<string, object>)data[componentType.FullName];
				
				if(component is ISerializableMonoBehaviour)
				{
					((ISerializableMonoBehaviour)component).Deserialize(componentData);
				}
				else if(_customSerializerTable.ContainsKey(componentType))
				{
					Type serializerType = _customSerializerTable[componentType];
					MethodInfo mi = serializerType.GetMethod("Deserialize");
					if(mi != null)
					{
						object instance = Activator.CreateInstance(serializerType);
						object[] parameters = new object[] { component, componentData };
						mi.Invoke(instance, parameters);
					}
				}
			}
			gameObject.name = data["name"].ToString();
			gameObject.tag = data["tag"].ToString();
			gameObject.layer = System.Convert.ToInt32(data["layer"]);
			
			bool activeSelf = (bool)data["activeSelf"];
			if(activeSelf != gameObject.activeSelf) {
				gameObject.SetActive(activeSelf);
			}
		}
		
#if UNITY_EDITOR
		public List<ComponentStatusPair> GetComponentList()
		{
			return _serializableComponents;
		}
#endif
		
		#region [Static Methods]
		public static IEnumerable<Type> GetTypesWithCustomSerializerAttribute()
		{
			return GetTypesWithCustomSerializerAttribute(Assembly.GetExecutingAssembly());
		}
		
		public static IEnumerable<Type> GetTypesWithCustomSerializerAttribute(Assembly assembly) 
		{
			foreach(Type type in assembly.GetTypes()) 
			{
				if(type.GetCustomAttributes(typeof(CustomSerializerAttribute), false).Length > 0 &&
					type.GetInterfaces().Contains(typeof(IComponentSerializer))) 
				{
					yield return type;
				}
			}
		}
		
		public static bool IsComponentSerializable(Component component)
		{
			return (component is ISerializableMonoBehaviour) || HasCustomSerializer(component);
		}
		
		private static bool HasCustomSerializer(Component component)
		{
			Type componentType = component.GetType();
			foreach(Type typeOfSerializer in GetTypesWithCustomSerializerAttribute())
			{
				object[] attributes = typeOfSerializer.GetCustomAttributes(typeof(CustomSerializerAttribute), false);
				CustomSerializerAttribute attribute = attributes[0] as CustomSerializerAttribute;
				
				if(attribute.typeToSerialize == componentType)
					return true;
			}
			
			return false;
		}
		#endregion
	}
}
