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
	public sealed class JsonDeserializer : IDataDeserializer
	{
		private string _inputFilename;
		
		public JsonDeserializer(string inputFilename)
		{
			_inputFilename = inputFilename;
		}
		
		public ReadOnlyDictionary<string, object> Deserialize()
		{
			if(File.Exists(_inputFilename))
			{
				Dictionary<string, object> data;
				using(StreamReader sr = File.OpenText(_inputFilename))
				{
					data = MiniJson.Deserialize(sr.ReadToEnd()) as Dictionary<string, object>;
				}
				
				return new ReadOnlyDictionary<string, object>(data);
			}

			return null;
		}
		
		public ReadOnlyDictionary<string, object> GetCustomMetadata()
		{
			string metafile = Path.ChangeExtension(_inputFilename, "meta");
			if(File.Exists(metafile))
			{
				Dictionary<string, object> data;
				using(StreamReader sr = File.OpenText(metafile))
				{
					data = MiniJson.Deserialize(sr.ReadToEnd()) as Dictionary<string, object>;
				}
				
				return new ReadOnlyDictionary<string, object>(data);
			}
			
			return null;
		}
	}
}
