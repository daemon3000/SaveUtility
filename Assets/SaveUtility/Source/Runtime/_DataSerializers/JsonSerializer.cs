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
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	public sealed class JsonSerializer : IDataSerializer
	{
		private string _outputFilename;
		private string _metadataFilename;
		
		public JsonSerializer(string outputFilename)
		{
#if UNITY_STANDALONE || UNITY_METRO || UNITY_METRO_8_1 || UNITY_EDITOR
			_outputFilename = outputFilename;
			_metadataFilename = PathHelper.ChangeExtension(_outputFilename, "meta");
#else
			_outputFilename = null;
			_metadataFilename = null;
			Debug.LogError("You cannot use JsonSerializer on the current platform");
#endif
		}
		
		public void Serialize(ReadOnlyDictionary<string, object> data)
		{
			if(data == null)
				throw new ArgumentNullException("data");

			if(_outputFilename == null)
				return;

#if UNITY_EDITOR || SAVEUTILITY_DEVBUILD
			string output = MiniJson.Serialize(data, true);
#else
			string output = MiniJson.Serialize(data, false);
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
			using(System.IO.StreamWriter sw = System.IO.File.CreateText(_outputFilename))
			{
				sw.Write(output);
			}
#elif UNITY_METRO || UNITY_METRO_8_1
			UnityEngine.Windows.File.WriteAllBytes(_outputFilename, System.Text.Encoding.Unicode.GetBytes(output));
#endif
		}
		
		public void Serialize(ReadOnlyDictionary<string, object> data, ReadOnlyDictionary<string, object> metadata)
		{
			if(data == null)
				throw new ArgumentNullException("data");
			if(metadata == null)
				throw new ArgumentNullException("metadata");

#if UNITY_EDITOR || SAVEUTILITY_DEVBUILD
			string output = MiniJson.Serialize(data, true);
			string metaoutput = MiniJson.Serialize(metadata, true);
#else
			string output = MiniJson.Serialize(data, false);
			string metaoutput = MiniJson.Serialize(metadata, false);
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
			using(System.IO.StreamWriter sw = System.IO.File.CreateText(_outputFilename))
			{
				sw.Write(output);
			}
			using(System.IO.StreamWriter sw = System.IO.File.CreateText(_metadataFilename))
			{
				sw.Write(metaoutput);
			}
#elif UNITY_METRO || UNITY_METRO_8_1
			UnityEngine.Windows.File.WriteAllBytes(_outputFilename, System.Text.Encoding.Unicode.GetBytes(output));
			UnityEngine.Windows.File.WriteAllBytes(_metadataFilename, System.Text.Encoding.Unicode.GetBytes(metaoutput));
#endif
		}
	}
}
