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
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO.SaveUtility
{
	public abstract class AsyncDataSerializer : IAsyncDataSerializer
	{
		private bool _isDone;
		private string _error;
		private Thread _thread;
		private object _handle = new object();
		
		public bool IsDone 
		{
			get 
			{
				bool temp;
				lock(_handle) 
				{
					temp = _isDone;
				}
				return temp;
			}
			private set 
			{
				lock(_handle)
				{
					_isDone = value;
				}
			}
		}
		
		public string Error
		{
			get 
			{
				string temp;
				lock(_handle) 
				{
					temp = _error;
				}
				return temp;
			}
			set 
			{
				lock(_handle)
				{
					_error = value;
				}
			}
		}
		
		public void Serialize(ReadOnlyDictionary<string, object> data)
		{
			_thread = new Thread(Run);
			_thread.Start(data);
		}
		
		public void Serialize(ReadOnlyDictionary<string, object> data, ReadOnlyDictionary<string, object> metadata)
		{
			_thread = new Thread(Run);
			_thread.Start(new object[] { data, metadata });
		}
	
		private void Run(object data)
		{
			if(data is Array)
			{
				object[] array = (object[])data;
				
				ThreadFunction((ReadOnlyDictionary<string, object>)array[0], (ReadOnlyDictionary<string, object>)array[1]);
				IsDone = true;
			}
			else
			{
				ThreadFunction((ReadOnlyDictionary<string, object>)data);
				IsDone = true;
			}
		}
		
		protected abstract void ThreadFunction(ReadOnlyDictionary<string, object> data);
		protected abstract void ThreadFunction(ReadOnlyDictionary<string, object> data, ReadOnlyDictionary<string, object> metadata);
	}
}
