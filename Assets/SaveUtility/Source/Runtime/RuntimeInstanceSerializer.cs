using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	public sealed class RuntimeInstanceSerializer : MonoBehaviour
	{
		private GameObject _template;
		private GameObject _instance;
		
		public GameObject Template
		{
			get { return _template; }
		}
		
		public void Initialize(GameObject template, GameObject instance)
		{
			_template = template;
			_instance = instance;
		}
		
		public Dictionary<string, object> Serialize()
		{
			if(_template == null || _instance == null) {
				return null;
			}
			
			SaveUtility saveUtility = SaveUtility.GetInstance();
			GameObjectSerializer serializer = _instance.GetComponent<GameObjectSerializer>();
			if(serializer != null)
			{
				Dictionary<string, object> dict = new Dictionary<string, object>();
				dict.Add("prefab", saveUtility.GetAssetID(_template));
				dict.Add("instance", serializer.Serialize());
				
				return dict;
			}
			
			return null;
		}
		
		public bool Deserialize(Dictionary<string, object> data)
		{
			SaveUtility saveUtility = SaveUtility.GetInstance();
			
			_template = saveUtility.GetAssetByID((string)data["prefab"]) as GameObject;
			if(_template != null)
			{
				GameObject instance = GameObject.Instantiate(_template) as GameObject;
				GameObjectSerializer serializer = instance.GetComponent<GameObjectSerializer>();
				if(serializer != null)
				{
					serializer.Deserialize((Dictionary<string, object>)data["instance"]);
					transform.parent = instance.transform;
					_instance = instance;
					return true;
				}
			}
			
			return false;
		}
	}
}
