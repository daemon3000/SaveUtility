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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
#if UNITY_WINRT
using System.Reflection;
#endif

namespace TeamUtility.IO
{
	public class BinaryFormatter
	{
		#region [Type Headers]
		private const byte NULL_BYTE = 0x00;
		private const byte BOOL_HEADER = 0x10;
		private const byte CHAR_HEADER = 0x20;
		private const byte INT32_HEADER = 0x30;
		private const byte INT64_HEADER = 0x40;
		private const byte SINGLE_HEADER = 0x50;
		private const byte DOUBLE_HEADER = 0x60;
		private const byte STRING_HEADER = 0x70;
		private const byte BYTE_ARRAY_HEADER = 0x80;
		private const byte LIST_HEADER = 0x90;
		private const byte DICTIONARY_HEADER = 0xa0;
		#endregion
		public const byte VERSION = 0x01;
		//	235 T(eam) U(tility) B I N \r \n 26 \n
		//	235 - Non ASCII character
		//	26 - EOF of some systems
		private readonly byte[] ID = { 0xeb, 0x54, 0x55, 0x42, 0x49, 0x4e, 0x0d, 0x0a, 0x1a, 0x0a };

		public object Deserialize(string filename)
		{
#if UNITY_STANDALONE || UNITY_EDITOR
			using(Stream stream = File.OpenRead(filename))
			{
				return Deserialize(stream);
			}
#elif UNITY_METRO
			byte[] data = UnityEngine.Windows.File.ReadAllBytes(filename);
			if(data != null)
			{
				return Deserialize(data);
			}
			else
			{
				return null;
			}
#else
			return null;
#endif
		}

		public object Deserialize(byte[] input)
		{
			using(MemoryStream stream = new MemoryStream(input))
			{
				return Deserialize(stream);
			}
		}

		public object Deserialize(Stream inputStream)
		{
			using(BinaryReader reader = new BinaryReader(inputStream))
			{
				byte[] id;
				byte version;

				ReadHeader(reader, out id, out version);
				if(!DoesIDMatch(id)) {
					throw new FormatException("File format ID does not match.");
				}
				else if(version != VERSION) {
					throw new FormatException("File format version does not match");
				}

				return ReadValue(reader);
			}
		}

		public void Serialize(object value, string filename)
		{
#if UNITY_STANDALONE || UNITY_EDITOR
			using(Stream stream = File.OpenWrite(filename))
			{
				Serialize(value, stream);
			}
#elif UNITY_METRO
			byte[] output = Serialize(value);
			UnityEngine.Windows.File.WriteAllBytes(filename, output);
#endif
		}

		public byte[] Serialize(object value)
		{
			using(MemoryStream stream = new MemoryStream())
			{
				Serialize(value, stream);
				return stream.ToArray();
			}
		}

		public void Serialize(object value, Stream outputStream)
		{
			using(BinaryWriter writer = new BinaryWriter(outputStream))
			{
				WriteHeader(writer);
				WriteValue(value, writer);
			}
		}

		#region [Read]
		private void ReadHeader(BinaryReader reader, out byte[] id, out byte version)
		{
			id = reader.ReadBytes(ID.Length);
			EatBytes(reader, 1);
			version = reader.ReadByte();
			EatBytes(reader, 1);
		}

		private object ReadValue(BinaryReader reader)
		{
			byte header = reader.ReadByte();
			if(header == NULL_BYTE)
			{
				return null;
			}
			else if(header == BOOL_HEADER)
			{
				return reader.ReadBoolean();
			}
			else if(header == CHAR_HEADER)
			{
				return reader.ReadChar();
			}
			else if(header == INT32_HEADER)
			{
				return reader.ReadInt32();
			}
			else if(header == INT64_HEADER)
			{
				return reader.ReadInt64();
			}
			else if(header == SINGLE_HEADER)
			{
				return reader.ReadSingle();
			}
			else if(header == DOUBLE_HEADER)
			{
				return reader.ReadDouble();
			}
			else if(header == STRING_HEADER)
			{
				return reader.ReadString();
			}
			else if(header == BYTE_ARRAY_HEADER)
			{
				return ReadByteArray(reader);
			}
			else if(header == LIST_HEADER)
			{
				return ReadList(reader);
			}
			else if(header == DICTIONARY_HEADER)
			{
				return ReadDictionary(reader);
			}
			else
			{
				throw new NotSupportedException(string.Format("The type header {0} is not supported", header.ToString("X2")));
			}
		}

		private byte[] ReadByteArray(BinaryReader reader)
		{
			int length = reader.ReadInt32();
			byte[] array = new byte[length];

			for(int i = 0; i < length; i++)
			{
				array[i] = reader.ReadByte();
			}

			return array;
		}

		private List<object> ReadList(BinaryReader reader)
		{
			int count = reader.ReadInt32();
			List<object> list = new List<object>(count + 1);

			for(int i = 0; i < count; i++)
			{
				list.Add(ReadValue(reader));
			}

			return list;
		}

		private IDictionary<string, object> ReadDictionary(BinaryReader reader)
		{
			int count = reader.ReadInt32();
			Dictionary<string, object> dict = new Dictionary<string, object>();

			for(int i = 0; i < count; i++)
			{
				dict.Add(reader.ReadString(), ReadValue(reader));
			}

			return dict;
		}

		private void EatBytes(BinaryReader reader, int count)
		{
			reader.ReadBytes(count);
		}

		private bool DoesIDMatch(byte[] id)
		{
			if(id.Length != ID.Length) {
				return false;
			}

			for(int i = 0; i < id.Length; i++)
			{
				if(id[i] != ID[i]) {
					return false;
				}
			}

			return true;
		}
		#endregion

		#region [Write]
		private void WriteHeader(BinaryWriter writer)
		{
			writer.Write(ID);
			writer.Write(NULL_BYTE);
			writer.Write(VERSION);
			writer.Write(NULL_BYTE);
		}

		private void WriteValue(object value, BinaryWriter writer)
		{
			if(value == null)
			{
				writer.Write(NULL_BYTE);
			}
			else if(value is bool)
			{
				WriteBool((bool)value, writer);
			}
			else if(value is char)
			{
				WriteChar((char)value, writer);
			}
			else if(value is int)
			{
				WriteInt32((int)value, writer);
			}
			else if(value is long)
			{
				WriteInt64((long)value, writer);
			}
			else if(value is float)
			{
				WriteSingle((float)value, writer);
			}
			else if(value is double)
			{
				WriteDouble((double)value, writer);
			}
			else if(value is string)
			{
				WriteString((string)value, writer);
			}
			else if(IsEnum(value))
			{
				WriteString(value.ToString(), writer);
			}
			else if(value is byte[])
			{
				WriteByteArray((byte[])value, writer);
			}
			else if(value is IList)
			{
				WriteList((IList)value, writer);
			}
			else if(value is IDictionary<string, object>)
			{
				WriteDictionary((IDictionary<string, object>)value, writer);
			}
			else
			{
				throw new NotSupportedException(string.Format("Object of type {0} is not supported", value.GetType().FullName));
			}
		}

		private void WriteBool(bool value, BinaryWriter writer)
		{
			writer.Write(BOOL_HEADER);
			writer.Write(value);
		}

		private void WriteChar(char value, BinaryWriter writer)
		{
			writer.Write(CHAR_HEADER);
			writer.Write(value);
		}

		private void WriteInt32(int value, BinaryWriter writer)
		{
			writer.Write(INT32_HEADER);
			writer.Write(value);
		}

		private void WriteInt64(long value, BinaryWriter writer)
		{
			writer.Write(INT64_HEADER);
			writer.Write(value);
		}

		private void WriteSingle(float value, BinaryWriter writer)
		{
			writer.Write(SINGLE_HEADER);
			writer.Write(value);
		}

		private void WriteDouble(double value, BinaryWriter writer)
		{
			writer.Write(DOUBLE_HEADER);
			writer.Write(value);
		}

		private void WriteString(string value, BinaryWriter writer)
		{
			writer.Write(STRING_HEADER);
			writer.Write(value);
		}

		private void WriteByteArray(byte[] value, BinaryWriter writer)
		{
			writer.Write(BYTE_ARRAY_HEADER);
			writer.Write(value.Length);
			writer.Write(value);
		}

		private void WriteList(IList value, BinaryWriter writer)
		{
			writer.Write(LIST_HEADER);
			writer.Write(value.Count);

			foreach(object item in value)
			{
				WriteValue(item, writer);
			}
		}

		private void WriteDictionary(IDictionary<string, object> value, BinaryWriter writer)
		{
			writer.Write(DICTIONARY_HEADER);
			writer.Write(value.Count);

			foreach(KeyValuePair<string, object> pair in value)
			{
				writer.Write(pair.Key);
				WriteValue(pair.Value, writer);
			}
		}

		private bool IsEnum(object value)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			TypeInfo typeInfo = value.GetType().GetTypeInfo();
			return typeInfo.IsEnum;
#else
			return value.GetType().IsEnum;
#endif
		}
		#endregion
	}
}

