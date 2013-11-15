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
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	[ExecuteInEditMode]
	public sealed class SaveUtility : MonoBehaviour 
	{
		#region [Asset-ID Pair]
		[Serializable]
		public sealed class AssetIDPair
		{
			public string id;
			public UnityEngine.Object asset;
			
			public AssetIDPair() { }
			public AssetIDPair(UnityEngine.Object asset, string id)
			{
				this.asset = asset;
				this.id = id;
			}
		}
		#endregion
		
		public const string VERSION = "0.1.0.0";
		private const int MAX_FRAMES_TO_GET_DATA = 5;
		
		[SerializeField] 
		private List<GameObjectSerializer> _serializers = new List<GameObjectSerializer>();
		[SerializeField]
		private List<AssetIDPair> _requiredAssets = new List<AssetIDPair>();
		
		private Dictionary<string, GameObject> _referenceTable;
		private Dictionary<string, AssetIDPair> _assetTable;
		private static SaveUtility _instance;
		
		public void AddGameObjectSerializer(GameObjectSerializer serializer)
		{
			if(!_serializers.Contains(serializer)) {
				_serializers.Add(serializer);
			}
		}
		
		public bool RemoveGameObjectSerializer(GameObjectSerializer serializer)
		{
			return _serializers.Remove(serializer);
		}
		
		public int GetGameObjectSerializerCount()
		{
			return _serializers.Count;
		}
		
		private void Awake()
		{
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
			{
				SaveUtility[] obj = UnityEngine.Object.FindObjectsOfType(typeof(SaveUtility)) as SaveUtility[];
				if(obj != null && obj.Length > 1) 
				{
					Debug.LogWarning("You can't have more than one SaveUtility in a scene.");
					UnityEngine.Object.DestroyImmediate(this);
				}
				
				return;
			}
#endif
			
			if(_instance != null)
			{
#if UNITY_EDITOR || SAVEUTILITY_DEVBUILD
				Debug.LogWarning("You can't have more than one SaveUtility in a scene.");
#endif
				UnityEngine.Object.Destroy(this);
				return;
			}
			
			_instance = this;
			BuildReferenceTable();
			BuildAssetTable();
		}
		
		private void BuildReferenceTable()
		{
			_referenceTable = new Dictionary<string, GameObject>();
			
			UniqueIdentifier[] identifiers = UnityEngine.Object.FindObjectsOfType(typeof(UniqueIdentifier)) as UniqueIdentifier[];
			foreach(UniqueIdentifier uid in identifiers)
			{
				_referenceTable.Add(uid.ID, uid.gameObject);
			}
		}
		
		public GameObject GetStoredGameObjectByID(string id)
		{
			GameObject gameObject;
			_referenceTable.TryGetValue(id, out gameObject);
			
			return gameObject;
		}
		
		public string GetStoredGameObjectID(GameObject gameObject)
		{
			foreach(var item in _referenceTable)
			{
				if(item.Value == gameObject) {
					return item.Key;
				}
			}
			
			return null;
		}
		
		public T GetStoredComponentByID<T>(string id) where T : Component
		{
			GameObject gameObject = GetStoredGameObjectByID(id);
			if(gameObject != null)
			{
				return gameObject.GetComponent<T>();
			}
			else
			{
				return null;
			}
		}
		
		private void BuildAssetTable()
		{
			_assetTable = new Dictionary<string, AssetIDPair>();
			
			foreach(AssetIDPair pair in _requiredAssets)
			{
				_assetTable.Add(pair.id, pair);
			}
		}
		
		public UnityEngine.Object GetAssetByID(string id)
		{
			AssetIDPair result = null;
			_assetTable.TryGetValue(id, out result);
			
			return (result != null) ? result.asset : null;
		}
		
		public string GetAssetID(UnityEngine.Object asset)
		{
			foreach(AssetIDPair pair in _requiredAssets)
			{
				if(pair.asset == asset) {
					return pair.id;
				}
			}
			
			return null;
		}
		
#if UNITY_EDITOR
		public void AddRequiredAsset(UnityEngine.Object asset)
		{
			if(!UnityEditor.EditorApplication.isPlaying && IsAssetValidForRegistration(asset))
			{
				_requiredAssets.Add(new AssetIDPair(asset, UniqueIdentifier.GetUniqueID()));
			}
		}
		
		public bool IsAssetValidForRegistration(UnityEngine.Object asset)
		{
			if(asset == null || !UnityEditor.EditorUtility.IsPersistent(asset)) {
				return false;
			}
			foreach(AssetIDPair pair in _requiredAssets)
			{
				if(pair.asset == asset) {
					return false;
				}
			}
			
			return true;
		}
#endif
		
		#region [Set/Get Save Table]
		public void SetSaveTable(Dictionary<string, object> saveTable)
		{
			Queue<GameObjectSerializer> waitingForDestroy = new Queue<GameObjectSerializer>();
			foreach(GameObjectSerializer serializer in _serializers)
			{
				if(saveTable.ContainsKey(serializer.ID))
				{
					serializer.Deserialize((Dictionary<string, object>)saveTable[serializer.ID]);
				}
				else
				{
					waitingForDestroy.Enqueue(serializer);
				}
			}
			
			while(waitingForDestroy.Count > 0)
			{
				GameObjectSerializer serializer = waitingForDestroy.Dequeue();
				if(serializer != null) {
					UnityEngine.Object.Destroy(serializer.gameObject);
				}
			}
		}
		
		public void GetSaveTable(Action<Dictionary<string, object>> completedCallback)
		{
			StartCoroutine(GetSaveTableOverTime(completedCallback));
		}
		
		private IEnumerator GetSaveTableOverTime(Action<Dictionary<string, object>> completedCallback)
		{
			Dictionary<string, object> saveTable = new Dictionary<string, object>();
			int count = 0;
			int batchSize = (_serializers.Count < MAX_FRAMES_TO_GET_DATA) ? 1 : _serializers.Count / MAX_FRAMES_TO_GET_DATA;
			
			while(count < _serializers.Count)
			{
				for(int i = 0; i < batchSize && count < _serializers.Count; i++, count++)
				{
					saveTable.Add(_serializers[count].ID, _serializers[count].Serialize());
				}
				yield return null;
			}
			
			saveTable.Add("metadata", GetMetadata());
			yield return null;
			
			completedCallback(saveTable);
		}
		
		private Dictionary<string, object> GetMetadata()
		{
			Dictionary<string, object> metadata = new Dictionary<string, object>();
			metadata.Add("sceneName", Application.loadedLevelName);
			metadata.Add("sceneIndex", Application.loadedLevel);
			metadata.Add("version", VERSION);
			
			return metadata;
		}
		#endregion
		
		#region [Static Methods]
		public static SaveUtility GetInstance(bool createIfNull)
		{
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
			{
				SaveUtility[] obj = UnityEngine.Object.FindObjectsOfType(typeof(SaveUtility)) as SaveUtility[];
				if(obj != null && obj.Length > 0) {
					return obj[0];
				}
				else 
				{
					return createIfNull ? CreateInstance() : null;
				}
			}
#endif
			if(_instance == null && createIfNull) {
				return CreateInstance();
			}
			return _instance;
		}
		
		private static SaveUtility CreateInstance()
		{
			if(_instance != null) {
				return _instance;
			}
			
			GameObject gameObject = new GameObject("SaveUtility");
			SaveUtility instance = gameObject.AddComponent<SaveUtility>();
			
			return instance;
		}
		#endregion
	}
}
