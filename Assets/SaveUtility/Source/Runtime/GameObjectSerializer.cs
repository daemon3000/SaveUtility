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
		
		private static Dictionary<Type, IComponentSerializer> _customSerializerTable;
		private static Dictionary<Type, ITypeConverter> _typeConverters;
		
		[SerializeField] private bool _storeAllComponents = true;
		[SerializeField] private List<ComponentStatusPair> _serializableComponents;
		private SaveUtility _saveUtility;
		
		private void Awake()
		{
			//	If a GameObjectSerializer is instantiated at runtime, it will not be registered with the SaveUtility.
			//	The task of managing the GameObjectSerializer should fall to a RuntimeObjectSerializer, if one is available.
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
			{
				SaveUtility saveUtility = SaveUtility.GetInstance(true);
				saveUtility.AddGameObjectSerializer(this);
				return;
			}
#endif
			if(_customSerializerTable == null) {
				BuildCustomSerializerTable();
			}
			if(_typeConverters == null) {
				SetTypeConverters();
			}
			_saveUtility = SaveUtility.GetInstance(false);
		}
		
		protected override void OnEnable()
		{
			base.OnEnable();

			if(_serializableComponents == null) {
				_serializableComponents = new List<ComponentStatusPair>();
			}
		}
		
		private void OnDestroy()
		{
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
			{
				SaveUtility saveUtility = SaveUtility.GetInstance(false);
				if(saveUtility != null) 
				{
					saveUtility.RemoveGameObjectSerializer(this);
				}
				return;
			}
#endif
			if(_saveUtility != null) 
			{
				_saveUtility.RemoveGameObjectSerializer(this);
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
				
				if(component is MonoBehaviour)
				{
					if(component is ISerializableMonoBehaviour)
					{
						data.Add(componentType.FullName, ((ISerializableMonoBehaviour)component).Serialize());
					}
					else
					{
						data.Add(componentType.FullName, SerializeComponent(component, componentType));
					}
				}
				else if(_customSerializerTable.ContainsKey(componentType))
				{
					IComponentSerializer serializer = _customSerializerTable[componentType];
					data.Add(componentType.FullName, serializer.Serialize(component));
				}
			}
			gameObject.SendMessage("OnSerialized", SendMessageOptions.DontRequireReceiver);
			
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
				
				if(component is MonoBehaviour)
				{
					if(component is ISerializableMonoBehaviour)
					{
						((ISerializableMonoBehaviour)component).Deserialize(componentData);
					}
					else
					{
						DeserializeComponent(component, componentType, componentData);
					}
				}
				else if(_customSerializerTable.ContainsKey(componentType))
				{
					IComponentSerializer serializer = _customSerializerTable[componentType];
					serializer.Deserialize(component, componentData);
				}
			}
			gameObject.name = data["name"].ToString();
			gameObject.tag = data["tag"].ToString();
			gameObject.layer = System.Convert.ToInt32(data["layer"]);
			if(gameObject.activeSelf)
			{
				gameObject.SendMessage("OnDeserialized", SendMessageOptions.DontRequireReceiver);
			}
			
			bool activeSelf = (bool)data["activeSelf"];
			if(activeSelf != gameObject.activeSelf) 
			{
				gameObject.SetActive(activeSelf);
				if(gameObject.activeSelf)
				{
					gameObject.SendMessage("OnDeserialized", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		
		#region [Component Serialization]
		private Dictionary<string, object> SerializeComponent(Component instance, Type componentType)
		{
			Dictionary<string, object> data = new Dictionary<string, object>();
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | 
										BindingFlags.Default | BindingFlags.Instance;
			IEnumerable<FieldInfo> fields = from fi in componentType.GetFields(bindingFlags)
											where fi.IsDefined(typeof(SaveFieldAttribute), false) select fi;
			
			foreach(FieldInfo fi in fields)
			{
				object value = GetFieldValue(instance, fi);
				data.Add(fi.Name, value);
			}
			return data;
		}
		
		private object GetFieldValue(Component instance, FieldInfo fieldInfo)
		{
			Type fieldType = fieldInfo.FieldType;
			object rawValue = fieldInfo.GetValue(instance);
			object value = null;
			
			if(rawValue == null || fieldType.IsEnum || IsPrimitive(fieldType))
			{
				value = rawValue;
			}
			else if(_typeConverters.ContainsKey(fieldType))
			{
				value = _typeConverters[fieldType].ConvertFrom(rawValue);
			}
			else if(typeof(Component).IsAssignableFrom(fieldType))
			{
				value = _typeConverters[typeof(Component)].ConvertFrom(rawValue);
			}
			else if(typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
			{
				value = _typeConverters[typeof(UnityEngine.Object)].ConvertFrom(rawValue);
			}
			else if(typeof(IList).IsAssignableFrom(fieldType))
			{
				value = GetListValue(instance, fieldInfo, fieldType);
			}
			else if(typeof(IDictionary).IsAssignableFrom(fieldType))
			{
				value = GetDictionaryValue(instance, fieldInfo);
			}
			
			return value;
		}
		
		private bool IsPrimitive(Type fieldType)
		{
			return (fieldType == typeof(bool) || fieldType == typeof(int) || fieldType == typeof(float) ||
					fieldType == typeof(char) || fieldType == typeof(string) || fieldType == typeof(double) ||
					fieldType == typeof(long) || fieldType == typeof(byte));
		}
		
		private object GetListValue(Component instance, FieldInfo fieldInfo, Type fieldType)
		{
			Type elementType = typeof(System.Array).IsAssignableFrom(fieldType) ? fieldType.GetElementType() :
																				  fieldType.GetProperty("Item").PropertyType;
			if(elementType.IsEnum || IsPrimitive(elementType))
			{
				return fieldInfo.GetValue(instance);
			}
			else
			{
				ITypeConverter converter = GetTypeConverter(elementType);
				if(converter == null) {
					return null;
				}
				
				IList rawValue = fieldInfo.GetValue(instance) as IList;
				List<object> value = new List<object>();
				foreach(object item in rawValue)
				{
					value.Add(converter.ConvertFrom(item));
				}
				
				return value;
			}
		}
		
		private ITypeConverter GetTypeConverter(Type type)
		{
			if(_typeConverters.ContainsKey(type))
			{
				return _typeConverters[type];
			}
			else if(typeof(Component).IsAssignableFrom(type))
			{
				return _typeConverters[typeof(Component)];
			}
			else if(typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				return _typeConverters[typeof(UnityEngine.Object)];
			}
			
			return null;
		}
		
		private object GetDictionaryValue(Component instance, FieldInfo fieldInfo)
		{
			Type[] genericArguments = fieldInfo.FieldType.GetGenericArguments();
			if(genericArguments[0] != typeof(string)) {
				return null;
//				throw new NotSupportedException(string.Format("Unable to save dictionary. Expected key of type string, got key of type {0}", entry.Key.GetType().Name));
			}
			
			Dictionary<string, object> value = new Dictionary<string, object>();
			IDictionary rawValue = fieldInfo.GetValue(instance) as IDictionary;
			
			foreach(DictionaryEntry entry in rawValue)
			{
				Type valueType = entry.Value.GetType();
				if(valueType.IsEnum || IsPrimitive(valueType))
				{
					value.Add((string)entry.Key, entry.Value);
				}
				else
				{
					ITypeConverter converter = GetTypeConverter(valueType);
					if(converter != null) 
					{
						value.Add((string)entry.Key, converter.ConvertFrom(entry.Value));
					}
					else
					{
						value.Add((string)entry.Key, null);
					}
				}
			}
			
			return value;
		}
		
		private void DeserializeComponent(Component instance, Type componentType, Dictionary<string, object> data)
		{
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | 
										BindingFlags.Default | BindingFlags.Instance;
			IEnumerable<FieldInfo> fields = from fi in componentType.GetFields(bindingFlags)
											where fi.IsDefined(typeof(SaveFieldAttribute), false) select fi;
			
			object value = null;
			foreach(FieldInfo fi in fields)
			{
				if(data.TryGetValue(fi.Name, out value))
				{
					SetFieldValue(instance, fi, value);
				}
			}
		}
		
		private void SetFieldValue(Component instance, FieldInfo fieldInfo, object value)
		{
			Type fieldType = fieldInfo.FieldType;
			if(fieldType.IsEnum)
			{
				fieldInfo.SetValue(instance, Convert.ToEnum(value, fieldType));
			}
			else if(IsPrimitive(fieldType))
			{
				fieldInfo.SetValue(instance, System.Convert.ChangeType(value, fieldType));
			}
			else if(_typeConverters.ContainsKey(fieldType))
			{
				ITypeConverter converter = _typeConverters[fieldType];
				fieldInfo.SetValue(instance, converter.ConvertTo(value));
			}
			else if(typeof(Component).IsAssignableFrom(fieldType))
			{
				ITypeConverter converter = _typeConverters[typeof(Component)];
				fieldInfo.SetValue(instance, converter.ConvertTo(value));
			}
			else if(typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
			{
				ITypeConverter converter = _typeConverters[typeof(UnityEngine.Object)];
				fieldInfo.SetValue(instance, converter.ConvertTo(value));
			}
			else if(typeof(IList).IsAssignableFrom(fieldType))
			{
				SetListValue(instance, fieldInfo, fieldType, value);
			}
			else if(typeof(IDictionary).IsAssignableFrom(fieldType))
			{
				SetDictionaryValue(instance, fieldInfo, fieldType, value);
			}
		}
		
		private void SetListValue(Component instance, FieldInfo fieldInfo, Type fieldType, object value)
		{
			bool isArray = typeof(System.Array).IsAssignableFrom(fieldType);
			Type elementType = isArray ? fieldType.GetElementType() :
										 fieldType.GetProperty("Item").PropertyType;
			IList<object> rawList = value as IList<object>;
			
			if(elementType == typeof(object))
			{
				if(isArray) {
					fieldInfo.SetValue(instance, rawList.ToArray());
				}
				else {
					fieldInfo.SetValue(instance, rawList);
				}
			}
			if(elementType.IsEnum || IsPrimitive(elementType))
			{
				if(isArray) {
					fieldInfo.SetValue(instance, PopulateArray(elementType, rawList, null));
				}
				else {
					fieldInfo.SetValue(instance, PopulateList(elementType, rawList, null));
				}
			}
			else
			{
				ITypeConverter converter = GetTypeConverter(elementType);
				if(converter != null) 
				{
					if(isArray) {
						fieldInfo.SetValue(instance, PopulateArray(elementType, rawList, converter));
					}
					else {
						fieldInfo.SetValue(instance, PopulateList(elementType, rawList, converter));
					}
				}
			}
		}
		
		private Array PopulateArray(Type elementType, IList<object> rawValues, ITypeConverter converter)
		{
			Array array = Array.CreateInstance(elementType, rawValues.Count);
			for(int i = 0; i < rawValues.Count; i++)
			{
				if(converter != null)
				{
					array.SetValue(converter.ConvertTo(rawValues[i]), i);
				}
				else
				{
					if(elementType.IsEnum) {
						array.SetValue(Convert.ToEnum(rawValues[i], elementType), i);
					}
					else {
						array.SetValue(System.Convert.ChangeType(rawValues[i], elementType), i);
					}
				}
			}
			
			return array;
		}
		
		private IList PopulateList(Type elementType, IList<object> rawValues, ITypeConverter converter)
		{
			Type listType = typeof(List<>).MakeGenericType(elementType);
			object list = Activator.CreateInstance(listType);
			
			foreach(object value in rawValues)
			{
				if(converter != null)
				{
					((IList)list).Add(converter.ConvertTo(value));
				}
				else
				{
					if(elementType.IsEnum) {
						((IList)list).Add(Convert.ToEnum(value, elementType));
					}
					else {
						((IList)list).Add(System.Convert.ChangeType(value, elementType));
					}
				}
			}
			
			return (IList)list;
		}
		
		private void SetDictionaryValue(Component instance, FieldInfo fieldInfo, Type fieldType, object value)
		{
			Type[] genericArguments = fieldType.GetGenericArguments();
			if(genericArguments[0] != typeof(string)) {
				return;
			}
			
			Dictionary<string, object> rawDictionary = value as Dictionary<string, object>;
			Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(genericArguments[0], genericArguments[1]);
			object dictionary = Activator.CreateInstance(dictionaryType);
			
			foreach(KeyValuePair<string, object> entry in rawDictionary)
			{
				if(genericArguments[1] == typeof(object))
				{
					((IDictionary)dictionary).Add(entry.Key, entry.Value);
				}
				else if(genericArguments[1].IsEnum)
				{
					((IDictionary)dictionary).Add(entry.Key, Convert.ToEnum((string)entry.Value, genericArguments[1]));
				}
				else if(IsPrimitive(genericArguments[1]))
				{
					((IDictionary)dictionary).Add(entry.Key, entry.Value);
				}
				else
				{
					ITypeConverter converter = GetTypeConverter(genericArguments[1]);
					if(converter != null)
					{
						((IDictionary)dictionary).Add(entry.Key, converter.ConvertTo(entry.Value));
					}
					else
					{
						((IDictionary)dictionary).Add(entry.Key, null);
					}
				}
			}
			
			fieldInfo.SetValue(instance, dictionary);
		}
		#endregion
		
#if UNITY_EDITOR
		public List<ComponentStatusPair> GetComponentList()
		{
			return _serializableComponents;
		}
#endif
		
		#region [Static Methods]
		public static IEnumerable<Type> GetTypeConverters()
		{
			return GetTypeConverters(Assembly.GetExecutingAssembly());
		}
		
		public static IEnumerable<Type> GetTypeConverters(Assembly assembly)
		{
			foreach(Type type in assembly.GetTypes()) 
			{
				if(type.GetCustomAttributes(typeof(CustomSerializerAttribute), false).Length > 0 &&
					type.GetInterfaces().Contains(typeof(ITypeConverter))) 
				{
					yield return type;
				}
			}
		}
		
		public static IEnumerable<Type> GetComponentSerializerTypes()
		{
			return GetComponentSerializerTypes(Assembly.GetExecutingAssembly());
		}
		
		public static IEnumerable<Type> GetComponentSerializerTypes(Assembly assembly) 
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
			if(component is MonoBehaviour)
			{
				return (component is ISerializableMonoBehaviour) || CanBeAutoSerialized(component.GetType());
			}
			
			return HasCustomSerializer(component);
		}
		
		private static bool CanBeAutoSerialized(Type monoBehaviourType)
		{
			return monoBehaviourType.IsDefined(typeof(SaveComponentAttribute), false);
		}
		
		private static bool HasCustomSerializer(Component component)
		{
			Type componentType = component.GetType();
			foreach(Type typeOfSerializer in GetComponentSerializerTypes())
			{
				object[] attributes = typeOfSerializer.GetCustomAttributes(typeof(CustomSerializerAttribute), false);
				CustomSerializerAttribute attribute = attributes[0] as CustomSerializerAttribute;
				
				if(attribute.typeToSerialize == componentType)
					return true;
			}
			
			return false;
		}
		
		private static void BuildCustomSerializerTable()
		{
			if(_customSerializerTable == null) {
				_customSerializerTable = new Dictionary<Type, IComponentSerializer>();
			}
			else {
				_customSerializerTable.Clear();
			}
			
			foreach(Type typeOfSerializer in GetComponentSerializerTypes())
			{
				object[] attributes = typeOfSerializer.GetCustomAttributes(typeof(CustomSerializerAttribute), false);
				CustomSerializerAttribute attribute = attributes[0] as CustomSerializerAttribute;
				
				if(!_customSerializerTable.ContainsKey(attribute.typeToSerialize)) 
				{
					IComponentSerializer serializer = Activator.CreateInstance(typeOfSerializer) as IComponentSerializer;
					if(serializer != null)
					{
						_customSerializerTable.Add(attribute.typeToSerialize, serializer);
					}
				}
			}
		}
		
		private static void SetTypeConverters()
		{
			if(_typeConverters == null) {
				_typeConverters = new Dictionary<Type, ITypeConverter>();
			}
			else {
				_typeConverters.Clear();
			}
			
			foreach(Type typeOfConverter in GetTypeConverters())
			{
				object[] attributes = typeOfConverter.GetCustomAttributes(typeof(CustomSerializerAttribute), false);
				CustomSerializerAttribute attribute = attributes[0] as CustomSerializerAttribute;
				
				if(!_typeConverters.ContainsKey(attribute.typeToSerialize)) 
				{
					ITypeConverter converter = Activator.CreateInstance(typeOfConverter) as ITypeConverter;
					if(converter != null)
					{
						_typeConverters.Add(attribute.typeToSerialize, converter);
					}
				}
			}
		}
		#endregion
	}
}
