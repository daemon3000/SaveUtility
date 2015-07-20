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
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	public sealed class JsonDeserializer : IDataDeserializer
	{
		private string _inputFilename;
		private string _metadataFilename;
		
		public JsonDeserializer(string inputFilename)
		{
#if UNITY_STANDALONE || UNITY_METRO || UNITY_METRO_8_1 || UNITY_EDITOR
			_inputFilename = inputFilename;
			_metadataFilename = PathHelper.ChangeExtension(_inputFilename, "meta");
#else
			_inputFilename = null;
			_metadataFilename = null;
			Debug.LogError("You cannot use JsonDeserializer on the current platform");
#endif
		}
		
		public ReadOnlyDictionary<string, object> Deserialize()
		{
#if UNITY_STANDALONE || UNITY_EDITOR
			bool isFileValid = _inputFilename != null && System.IO.File.Exists(_inputFilename);
#elif UNITY_METRO || UNITY_METRO_8_1
			bool isFileValid = _inputFilename != null && UnityEngine.Windows.File.Exists(_inputFilename);
#else
			bool isFileValid = false;
#endif

			if(isFileValid)
			{
#if UNITY_STANDALONE || UNITY_EDITOR
				Dictionary<string, object> data;
				using(StreamReader sr = File.OpenText(_inputFilename))
				{
					data = MiniJson.Deserialize(sr.ReadToEnd()) as Dictionary<string, object>;
				}

				if(data != null)
					return new ReadOnlyDictionary<string, object>(data);
#else
				byte[] rawInput = UnityEngine.Windows.File.ReadAllBytes(_inputFilename);
				string input = System.Text.Encoding.Unicode.GetString(rawInput, 0, rawInput.Length);

				Dictionary<string, object> data = MiniJson.Deserialize(input) as Dictionary<string, object>;
				if(data != null)
					return new ReadOnlyDictionary<string, object>(data);
#endif
			}

			return null;
		}
		
		public ReadOnlyDictionary<string, object> GetCustomMetadata()
		{
#if UNITY_STANDALONE || UNITY_EDITOR
			bool isFileValid = _metadataFilename != null && System.IO.File.Exists(_metadataFilename);
#elif UNITY_METRO || UNITY_METRO_8_1
			bool isFileValid = _metadataFilename != null && UnityEngine.Windows.File.Exists(_metadataFilename);
#else
			bool isFileValid = false;
#endif
			
			if(isFileValid)
			{
#if UNITY_STANDALONE || UNITY_EDITOR
				Dictionary<string, object> data;
				using(StreamReader sr = File.OpenText(_metadataFilename))
				{
					data = MiniJson.Deserialize(sr.ReadToEnd()) as Dictionary<string, object>;
				}
				
				if(data != null)
					return new ReadOnlyDictionary<string, object>(data);
#else
				byte[] rawInput = UnityEngine.Windows.File.ReadAllBytes(_metadataFilename);
				string input = System.Text.Encoding.Unicode.GetString(rawInput, 0, rawInput.Length);
				
				Dictionary<string, object> data = MiniJson.Deserialize(input) as Dictionary<string, object>;
				if(data != null)
					return new ReadOnlyDictionary<string, object>(data);
#endif
			}
			
			return null;
		}
	}
}
