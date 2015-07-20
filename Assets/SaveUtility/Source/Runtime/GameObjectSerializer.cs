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
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
#if UNITY_WINRT && !UNITY_EDITOR
using _Type = System.Reflection.TypeInfo;
#else
using _Type = System.Type;
#endif

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
		
		private static Dictionary<_Type, IComponentSerializer> _customSerializerTable;
		private static Dictionary<_Type, ITypeConverter> _typeConverters;

		[SerializeField] private List<ComponentStatusPair> _serializableComponents = new List<ComponentStatusPair>();
		private SaveUtility _saveUtility = null;
		
		private void Awake()
		{
			//	If a GameObjectSerializer is instantiated at runtime, it will not be registered with the SaveUtility.
			//	The task of managing the GameObjectSerializer should fall to a RuntimeObjectSerializer, if one is available.
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
			{
				SaveUtility saveUtility = SaveUtility.GetInstance();
				if(saveUtility == null)
					saveUtility = SaveUtility.CreateInstance();

				GameObjectSerializer result = saveUtility.GetGameObjectSerializerByID(_id);
				if(result == null)
				{
					var prefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject) as GameObject;
					if(prefab != null)
					{
						GameObjectSerializer serializer = prefab.GetComponent<GameObjectSerializer>();
						if(serializer != null)
						{
							if(!string.IsNullOrEmpty(serializer.ID))
							{
								_id = GetUniqueID();
#if SAVEUTILITY_DEVBUILD
								Debug.LogWarning("GameObjectSerializer: " + GetInstanceID() + " has the same ID as the prefab. " + "New ID: " + _id);
#endif
							}
						}
					}
					saveUtility.AddGameObjectSerializer(this);
				}
				else if(result != this)
				{
					_id = GetUniqueID();
#if SAVEUTILITY_DEVBUILD
					Debug.Log("GameObjectSerializer: " + GetInstanceID() + " has a duplicate ID. " + "New ID: " + _id);
#endif

					saveUtility.AddGameObjectSerializer(this);
				}

				return;
			}
#endif
			if(_customSerializerTable == null) {
				BuildCustomSerializerTable();
			}
			if(_typeConverters == null) {
				SetTypeConverters();
			}
			_saveUtility = SaveUtility.GetInstance();
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
				SaveUtility saveUtility = SaveUtility.GetInstance();
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
				if(!pair.serialize)
					continue;
				
				Component component = pair.component;
				_Type componentType = GetTypeOf(component);
				
				if(component is MonoBehaviour)
				{
					if(component is ISavableMonoBehaviour)
					{
						data.Add(componentType.FullName, ((ISavableMonoBehaviour)component).Serialize());
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
				_Type componentType = GetTypeOf(component);
				Dictionary<string, object> componentData = (Dictionary<string, object>)data[componentType.FullName];
				
				if(component is MonoBehaviour)
				{
					if(component is ISavableMonoBehaviour)
					{
						((ISavableMonoBehaviour)component).Deserialize(componentData);
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
		private Dictionary<string, object> SerializeComponent(Component instance, _Type componentType)
		{
			Dictionary<string, object> data = new Dictionary<string, object>();
#if UNITY_WINRT && !UNITY_EDITOR
			IEnumerable<FieldInfo> fields = from fi in componentType.DeclaredFields
											where fi.IsDefined(typeof(SaveFieldAttribute), false) select fi;
#else
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | 
										BindingFlags.Default | BindingFlags.Instance;
			IEnumerable<FieldInfo> fields = from fi in componentType.GetFields(bindingFlags)
											where fi.IsDefined(typeof(SaveFieldAttribute), false) select fi;
#endif
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
			
			if(rawValue == null || IsEnum(fieldType) || IsPrimitive(fieldType))
			{
				value = rawValue;
			}
			else if(HasTypeConverter(fieldType))
			{
#if UNITY_WINRT && !UNITY_EDITOR
				value = _typeConverters[fieldType.GetTypeInfo()].ConvertFrom(rawValue);
#else
				value = _typeConverters[fieldType].ConvertFrom(rawValue);
#endif
			}
			else if(IsAssignableFrom(typeof(Component), fieldType))
			{
#if UNITY_WINRT && !UNITY_EDITOR
				value = _typeConverters[typeof(Component).GetTypeInfo()].ConvertFrom(rawValue);
#else
				value = _typeConverters[typeof(Component)].ConvertFrom(rawValue);
#endif
			}
			else if(IsAssignableFrom(typeof(UnityEngine.Object), fieldType))
			{
#if UNITY_WINRT && !UNITY_EDITOR
				value = _typeConverters[typeof(UnityEngine.Object).GetTypeInfo()].ConvertFrom(rawValue);
#else
				value = _typeConverters[typeof(UnityEngine.Object)].ConvertFrom(rawValue);
#endif
			}
			else if(IsAssignableFrom(typeof(IList), fieldType))
			{
				value = GetListValue(instance, fieldInfo, fieldType);
			}
			else if(IsAssignableFrom(typeof(IDictionary), fieldType))
			{
				value = GetDictionaryValue(instance, fieldInfo, fieldType);
			}
			
			return value;
		}

		private bool IsEnum(Type type)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			return type.GetTypeInfo().IsEnum;
#else
			return type.IsEnum;
#endif
		}

		private bool IsPrimitive(Type fieldType)
		{
			return (fieldType == typeof(bool) || fieldType == typeof(int) || fieldType == typeof(float) ||
					fieldType == typeof(char) || fieldType == typeof(string) || fieldType == typeof(double) ||
					fieldType == typeof(long) || fieldType == typeof(byte));
		}

		private bool IsAssignableFrom(Type sourceType, Type destType)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			return sourceType.GetTypeInfo().IsAssignableFrom(destType.GetTypeInfo());
#else
			return sourceType.IsAssignableFrom(destType);
#endif
		}

		private bool HasTypeConverter(Type fieldType)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			return _typeConverters.ContainsKey(fieldType.GetTypeInfo());
#else
			return _typeConverters.ContainsKey(fieldType);
#endif
		}

		private object GetListValue(Component instance, FieldInfo fieldInfo, Type fieldType)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			Type elementType = IsAssignableFrom(typeof(System.Array), fieldType) ? fieldType.GetTypeInfo().GetElementType() :
																					fieldType.GetTypeInfo().GetDeclaredProperty("Item").PropertyType;
#else
			Type elementType = IsAssignableFrom(typeof(System.Array), fieldType) ? fieldType.GetElementType() :
																				   fieldType.GetProperty("Item").PropertyType;
#endif

			if(IsEnum(elementType) || IsPrimitive(elementType))
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
			if(HasTypeConverter(type))
			{
#if UNITY_WINRT && !UNITY_EDITOR
				return _typeConverters[type.GetTypeInfo()];
#else
				return _typeConverters[type];
#endif
			}
			if(IsAssignableFrom(typeof(Component), type))
			{
#if UNITY_WINRT && !UNITY_EDITOR
				return _typeConverters[typeof(Component).GetTypeInfo()];
#else
				return _typeConverters[typeof(Component)];
#endif
			}
			if(IsAssignableFrom(typeof(UnityEngine.Object), type))
			{
#if UNITY_WINRT && !UNITY_EDITOR
				return _typeConverters[typeof(UnityEngine.Object).GetTypeInfo()];
#else
				return _typeConverters[typeof(UnityEngine.Object)];
#endif
			}
			
			return null;
		}
		
		private object GetDictionaryValue(Component instance, FieldInfo fieldInfo, Type fieldType)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			Type[] genericArguments = fieldType.GetTypeInfo().GenericTypeArguments;
#else
			Type[] genericArguments = fieldType.GetGenericArguments();
#endif
			if(genericArguments[0] != typeof(string))
				return null;
			
			Dictionary<string, object> value = new Dictionary<string, object>();
			IDictionary rawValue = fieldInfo.GetValue(instance) as IDictionary;
			
			foreach(DictionaryEntry entry in rawValue)
			{
				Type valueType = entry.Value.GetType();
				if(IsEnum(valueType) || IsPrimitive(valueType))
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
		#endregion

		#region [Component Deserialization]
		private void DeserializeComponent(Component instance, _Type componentType, Dictionary<string, object> data)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			IEnumerable<FieldInfo> fields = from fi in componentType.DeclaredFields
											where fi.IsDefined(typeof(SaveFieldAttribute), false) select fi;
#else
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | 
										BindingFlags.Default | BindingFlags.Instance;
			IEnumerable<FieldInfo> fields = from fi in componentType.GetFields(bindingFlags)
											where fi.IsDefined(typeof(SaveFieldAttribute), false) select fi;
#endif
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
			if(IsEnum(fieldType))
			{
				fieldInfo.SetValue(instance, Convert.ToEnum(value, fieldType));
			}
			else if(IsPrimitive(fieldType))
			{
				fieldInfo.SetValue(instance, System.Convert.ChangeType(value, fieldType));
			}
			else if(HasTypeConverter(fieldType))
			{
#if UNITY_WINRT && !UNITY_EDITOR
				ITypeConverter converter = _typeConverters[fieldType.GetTypeInfo()];
				fieldInfo.SetValue(instance, converter.ConvertTo(value));
#else
				ITypeConverter converter = _typeConverters[fieldType];
				fieldInfo.SetValue(instance, converter.ConvertTo(value));
#endif
			}
			else if(IsAssignableFrom(typeof(Component), fieldType))
			{
#if UNITY_WINRT && !UNITY_EDITOR
				ITypeConverter converter = _typeConverters[typeof(Component).GetTypeInfo()];
				fieldInfo.SetValue(instance, converter.ConvertTo(value));
#else
				ITypeConverter converter = _typeConverters[typeof(Component)];
				fieldInfo.SetValue(instance, converter.ConvertTo(value));
#endif
			}
			else if(IsAssignableFrom(typeof(UnityEngine.Object), fieldType))
			{
#if UNITY_WINRT && !UNITY_EDITOR
				ITypeConverter converter = _typeConverters[typeof(UnityEngine.Object).GetTypeInfo()];
				fieldInfo.SetValue(instance, converter.ConvertTo(value));
#else
				ITypeConverter converter = _typeConverters[typeof(UnityEngine.Object)];
				fieldInfo.SetValue(instance, converter.ConvertTo(value));
#endif
			}
			else if(IsAssignableFrom(typeof(IList), fieldType))
			{
				SetListValue(instance, fieldInfo, fieldType, value);
			}
			else if(IsAssignableFrom(typeof(IDictionary), fieldType))
			{
				SetDictionaryValue(instance, fieldInfo, fieldType, value);
			}
		}
		
		private void SetListValue(Component instance, FieldInfo fieldInfo, Type fieldType, object value)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			bool isArray = IsAssignableFrom(typeof(System.Array), fieldType);
			Type elementType = isArray ? fieldType.GetTypeInfo().GetElementType() :
										 fieldType.GetTypeInfo().GetDeclaredProperty("Item").PropertyType;
#else
			bool isArray = typeof(System.Array).IsAssignableFrom(fieldType);
			Type elementType = isArray ? fieldType.GetElementType() :
										 fieldType.GetProperty("Item").PropertyType;
#endif
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
			if(IsEnum(elementType) || IsPrimitive(elementType))
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
					if(IsEnum(elementType)) {
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
#if UNITY_WINRT && !UNITY_EDITOR
			Type listType = typeof(List<>).GetTypeInfo().MakeGenericType(elementType);
			object list = Activator.CreateInstance(listType);
#else
			Type listType = typeof(List<>).MakeGenericType(elementType);
			object list = Activator.CreateInstance(listType);
#endif
			
			foreach(object value in rawValues)
			{
				if(converter != null)
				{
					((IList)list).Add(converter.ConvertTo(value));
				}
				else
				{
					if(IsEnum(elementType)) {
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
#if UNITY_WINRT && !UNITY_EDITOR
			Type[] genericArguments = fieldType.GetTypeInfo().GenericTypeArguments;
#else
			Type[] genericArguments = fieldType.GetGenericArguments();
#endif
			if(genericArguments[0] != typeof(string))
				return;
			
			Dictionary<string, object> rawDictionary = value as Dictionary<string, object>;
#if UNITY_WINRT && !UNITY_EDITOR
			Type dictionaryType = typeof(Dictionary<,>).GetTypeInfo().MakeGenericType(genericArguments[0], genericArguments[1]);
			object dictionary = Activator.CreateInstance(dictionaryType);
#else
			Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(genericArguments[0], genericArguments[1]);
			object dictionary = Activator.CreateInstance(dictionaryType);
#endif
			
			foreach(KeyValuePair<string, object> entry in rawDictionary)
			{
				if(genericArguments[1] == typeof(object))
				{
					((IDictionary)dictionary).Add(entry.Key, entry.Value);
				}
				else if(IsEnum(genericArguments[1]))
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

		[ContextMenu("Copy ID")]
		private void CopyIDToSystemBuffer()
		{
			UnityEditor.EditorGUIUtility.systemCopyBuffer = _id;
		}
#endif
		public static bool IsComponentSerializable(Component component)
		{
			if(component is MonoBehaviour)
			{
				return (component is ISavableMonoBehaviour) || CanBeAutoSerialized(GetTypeOf(component));
			}
			
			return HasCustomSerializer(component);
		}
		
		private static bool CanBeAutoSerialized(_Type monoBehaviourType)
		{
			return monoBehaviourType.IsDefined(typeof(SaveComponentAttribute), false);
		}
		
		private static bool HasCustomSerializer(Component component)
		{
			_Type componentType = GetTypeOf(component);
			foreach(_Type typeOfSerializer in GetComponentSerializerTypes())
			{
				CustomSerializerAttribute attribute = GetCustomSerializerAttribute(typeOfSerializer);
#if UNITY_WINRT && !UNITY_EDITOR
				if(attribute.typeToSerialize.GetTypeInfo() == componentType)
					return true;
#else
				if(attribute.typeToSerialize == componentType)
					return true;
#endif
			}
			
			return false;
		}

		private static IEnumerable<_Type> GetTypeConverters()
		{
			foreach(_Type type in GetTypesInExecutingAssembly()) 
			{
#if UNITY_WINRT && !UNITY_EDITOR
				if(type.GetCustomAttributes(typeof(CustomSerializerAttribute), false).Count() > 0 &&
				   type.ImplementedInterfaces.Contains(typeof(ITypeConverter))) 
				{
					yield return type;
				}
#else
				if(type.GetCustomAttributes(typeof(CustomSerializerAttribute), false).Length > 0 &&
					type.GetInterfaces().Contains(typeof(ITypeConverter))) 
				{
					yield return type;
				}
#endif
			}
		}
		
		private static IEnumerable<_Type> GetComponentSerializerTypes() 
		{
			foreach(_Type type in GetTypesInExecutingAssembly()) 
			{
#if UNITY_WINRT && !UNITY_EDITOR
				if(type.GetCustomAttributes(typeof(CustomSerializerAttribute), false).Count() > 0 &&
				   type.ImplementedInterfaces.Contains(typeof(IComponentSerializer))) 
				{
					yield return type;
				}
#else
				if(type.GetCustomAttributes(typeof(CustomSerializerAttribute), false).Length > 0 &&
					type.GetInterfaces().Contains(typeof(IComponentSerializer))) 
				{
					yield return type;
				}
#endif
			}
		}
		
		private static void BuildCustomSerializerTable()
		{
			if(_customSerializerTable == null) {
				_customSerializerTable = new Dictionary<_Type, IComponentSerializer>();
			}
			else {
				_customSerializerTable.Clear();
			}
			
			foreach(_Type typeOfSerializer in GetComponentSerializerTypes())
			{
				CustomSerializerAttribute attribute = GetCustomSerializerAttribute(typeOfSerializer);
#if UNITY_WINRT && !UNITY_EDITOR
				if(!_customSerializerTable.ContainsKey(attribute.typeToSerialize.GetTypeInfo())) 
				{
					IComponentSerializer serializer = Activator.CreateInstance(typeOfSerializer.AsType()) as IComponentSerializer;
					if(serializer != null)
					{
						_customSerializerTable.Add(attribute.typeToSerialize.GetTypeInfo(), serializer);
					}
				}
#else
				if(!_customSerializerTable.ContainsKey(attribute.typeToSerialize)) 
				{
					IComponentSerializer serializer = Activator.CreateInstance(typeOfSerializer) as IComponentSerializer;
					if(serializer != null)
					{
						_customSerializerTable.Add(attribute.typeToSerialize, serializer);
					}
				}
#endif
			}
		}
		
		private static void SetTypeConverters()
		{
			if(_typeConverters == null)
				_typeConverters = new Dictionary<_Type, ITypeConverter>();
			else
				_typeConverters.Clear();
			
			foreach(_Type typeOfConverter in GetTypeConverters())
			{
				CustomSerializerAttribute attribute = GetCustomSerializerAttribute(typeOfConverter);
#if UNITY_WINRT && !UNITY_EDITOR
				if(!_typeConverters.ContainsKey(attribute.typeToSerialize.GetTypeInfo())) 
				{
					ITypeConverter converter = Activator.CreateInstance(typeOfConverter.AsType()) as ITypeConverter;
					if(converter != null)
					{
						_typeConverters.Add(attribute.typeToSerialize.GetTypeInfo(), converter);
					}
				}
#else
				if(!_typeConverters.ContainsKey(attribute.typeToSerialize)) 
				{
					ITypeConverter converter = Activator.CreateInstance(typeOfConverter) as ITypeConverter;
					if(converter != null)
					{
						_typeConverters.Add(attribute.typeToSerialize, converter);
					}
				}
#endif
			}
		}

		private static _Type GetTypeOf(object value)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			return value.GetType().GetTypeInfo();
#else
			return value.GetType();
#endif
		}

		private static IEnumerable<_Type> GetTypesInExecutingAssembly()
		{
#if UNITY_WINRT && !UNITY_EDITOR
			return typeof(GameObjectSerializer).GetTypeInfo().Assembly.DefinedTypes;
#else
			return Assembly.GetExecutingAssembly().GetTypes();
#endif
		}

		private static CustomSerializerAttribute GetCustomSerializerAttribute(_Type type)
		{
			var attributes = type.GetCustomAttributes(typeof(CustomSerializerAttribute), false);
			return attributes.First() as CustomSerializerAttribute;
		}
	}
}
