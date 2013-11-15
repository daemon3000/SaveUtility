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
	public abstract class AsyncDataSerializer : IAsyncDataSerializer
	{
		protected Dictionary<string, object> _data;
		private bool _isDone;
		private string _error;
		private object _handle = new object();
		private System.Threading.Thread _thread;
		
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
		
		public void Serialize(Dictionary<string, object> data)
		{
			_data = data;
			_thread = new System.Threading.Thread(Run);
			_thread.Start();
		}
	
		protected abstract void ThreadFunction();
		
		private void Run()
		{
			ThreadFunction();
			IsDone = true;
		}
	}
}
