/*
 * Copyright (c) 2013-2014 Calvin Rien
 *
 * Based on the JSON parser by Patrick van Bergen
 * http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
 *
 * Simplified it so that it doesn't throw exceptions
 * and can be used in Unity iPhone with maximum code stripping.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO.SaveUtility;

namespace TeamUtility.IO
{
	/// <summary>
	/// This class encodes and decodes JSON strings.
	/// Spec. details, see http://www.json.org/
	///
	/// JSON uses Arrays and Objects. These correspond here to the datatypes IList and IDictionary.
	/// All numbers are parsed to doubles.
	/// </summary>
	public static class MiniJson
	{
		/// <summary>
		/// Parses the string json into a value
		/// </summary>
		/// <param name="json">A JSON string.</param>
		/// <returns>An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
		public static object Deserialize(string json)
		{
			return string.IsNullOrEmpty(json) ? null : Parser.Parse(json);
		}
		
		/// <summary>
		/// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
		/// </summary>
		/// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
		public static string Serialize(object obj, bool prettyPrint)
		{
			return Serializer.Serialize(obj, prettyPrint);
		}
		
		private sealed class Parser : IDisposable
		{
			private enum TOKEN
			{
				NONE,
				CURLY_OPEN,
				CURLY_CLOSE,
				SQUARED_OPEN,
				SQUARED_CLOSE,
				COLON,
				COMMA,
				STRING,
				NUMBER,
				TRUE,
				FALSE,
				NULL
			};
			
			private const string WORD_BREAK = "{}[],:\"";
			private StringReader json;
			
			#region [Properties]
			private char PeekChar 
			{
				get 
				{
					return System.Convert.ToChar(json.Peek());
				}
			}

			private char NextChar 
			{
				get 
				{
					return System.Convert.ToChar(json.Read());
				}
			}

			private string NextWord 
			{
				get 
				{
					StringBuilder word = new StringBuilder();
					while(!IsWordBreak(PeekChar)) 
					{
						word.Append(NextChar);
						if(json.Peek() == -1) {
							break;
						}
					}

					return word.ToString();
				}
			}

			private TOKEN NextToken 
			{
				get 
				{
					EatWhitespace();
					if(json.Peek() == -1) {
						return TOKEN.NONE;
					}

					switch(PeekChar) 
					{
					case '{':
						return TOKEN.CURLY_OPEN;
					case '}':
						json.Read();
						return TOKEN.CURLY_CLOSE;
					case '[':
						return TOKEN.SQUARED_OPEN;
					case ']':
						json.Read();
						return TOKEN.SQUARED_CLOSE;
					case ',':
						json.Read();
						return TOKEN.COMMA;
					case '"':
						return TOKEN.STRING;
					case ':':
						return TOKEN.COLON;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					case '-':
						return TOKEN.NUMBER;
					}

					switch(NextWord) 
					{
					case "false":
						return TOKEN.FALSE;
					case "true":
						return TOKEN.TRUE;
					case "null":
						return TOKEN.NULL;
					}

					return TOKEN.NONE;
				}
			}
			#endregion
			
			#region [Init]
			public Parser(string jsonString)
			{
				json = new StringReader(jsonString);
			}

			public static object Parse(string jsonString)
			{
				using(var instance = new Parser(jsonString)) 
				{
					return instance.ParseValue();
				}
			}
			#endregion
			
			public void Dispose()
			{
				json.Dispose();
				json = null;
			}
			
			private Dictionary<string, object> ParseObject()
			{
				Dictionary<string, object> table = new Dictionary<string, object>();
				json.Read();			// ditch opening bracket
				
				while(true) 
				{
					switch(NextToken) 
					{
					case TOKEN.NONE:
						return null;
					case TOKEN.COMMA:
						continue;
					case TOKEN.CURLY_CLOSE:
						return table;
					default:
						string name = ParseString();
						if(name == null) {
							return null;
						}
						if(NextToken != TOKEN.COLON) {
							return null;
						}
						json.Read();	// ditch the colon
						
						table[name] = ParseValue();
						break;
					}
				}
			}
			
			private List<object> ParseArray()
			{
				List<object> array = new List<object>();
				json.Read();			// ditch opening bracket

				bool parsing = true;
				while(parsing) 
				{
					TOKEN nextToken = NextToken;
					switch(nextToken) 
					{
					case TOKEN.NONE:
						return null;
					case TOKEN.COMMA:
						continue;
					case TOKEN.SQUARED_CLOSE:
						parsing = false;
						break;
					default:
						object value = ParseByToken(nextToken);
						array.Add(value);
						break;
					}
				}

				return array;
			}
			
			private object ParseValue()
			{
				TOKEN nextToken = NextToken;
				return ParseByToken(nextToken);
			}
			
			private object ParseByToken(TOKEN token)
			{
				switch(token) 
				{
				case TOKEN.STRING:
					return ParseString();
				case TOKEN.NUMBER:
					return ParseNumber();
				case TOKEN.CURLY_OPEN:
					return ParseObject();
				case TOKEN.SQUARED_OPEN:
					return ParseArray();
				case TOKEN.TRUE:
					return true;
				case TOKEN.FALSE:
					return false;
				case TOKEN.NULL:
					return null;
				default:
					return null;
				}
			}
			
			private string ParseString()
			{
				StringBuilder builder = new StringBuilder();
				char c;
				bool parsing = true;
				
				json.Read ();		// ditch opening quote
				while (parsing) 
				{
					if(json.Peek() == -1) 
					{
						parsing = false;
						break;
					}

					c = NextChar;
					switch(c) 
					{
					case '"':
						parsing = false;
						break;
					case '\\':
						if (json.Peek() == -1) 
						{
							parsing = false;
							break;
						}

						c = NextChar;
						switch(c) 
						{
						case '"':
						case '\\':
						case '/':
							builder.Append(c);
							break;
						case 'b':
							builder.Append('\b');
							break;
						case 'f':
							builder.Append('\f');
							break;
						case 'n':
							builder.Append('\n');
							break;
						case 'r':
							builder.Append('\r');
							break;
						case 't':
							builder.Append('\t');
							break;
						case 'u':
							var hex = new char[4];

							for (int i=0; i< 4; i++) {
								hex [i] = NextChar;
							}

							builder.Append((char)System.Convert.ToInt32(new string(hex), 16));
							break;
						}
						break;
					default:
						builder.Append(c);
						break;
					}
				}

				return builder.ToString();
			}
			
			private object ParseNumber()
			{
				string number = NextWord;
				if (number.IndexOf('.') == -1) 
				{
					long parsedInt;
					Int64.TryParse(number, out parsedInt);
					return parsedInt;
				}
				else
				{
					double parsedDouble;
					Double.TryParse(number, out parsedDouble);
					return parsedDouble;
				}
			}
			
			private void EatWhitespace()
			{
				while(Char.IsWhiteSpace(PeekChar)) 
				{
					json.Read();
					if (json.Peek() == -1) {
						break;
					}
				}
			}
			
			private bool IsWordBreak(char c)
			{
				return Char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
			}
		}
		
		private sealed class Serializer
		{
			private StringBuilder builder;
			private int depth;
			private bool prettyPrint;
			
			public string Json
			{
				get
				{
					return builder.ToString();
				}
			}
			
			#region [Init]
			public Serializer(bool prettyPrint)
			{
				depth = 0;
				builder = new StringBuilder();
				this.prettyPrint = prettyPrint;
			}

			public static string Serialize(object obj, bool prettyPrint)
			{
				Serializer serializer = new Serializer(prettyPrint);
				serializer.SerializeValue(obj);

				return serializer.Json;
			}
			#endregion

			private void SerializeValue(object value)
			{
				if(value == null) 
				{
					builder.Append("null");
				} 
				else if(value is char) 
				{
					SerializeString(new string((char)value, 1));
				}
				else if(value is string) 
				{
					SerializeString((string)value);
				} 
				else if(value is bool) 
				{
					builder.Append((bool)value ? "true" : "false");
				} 
				else if(value is IList<object>) 
				{
					SerializeArray((IList<object>)value);
				} 
				else if(value is IDictionary<string, object>) 
				{
					SerializeObject((IDictionary<string, object>)value);
				} 
				else 
				{
					SerializeOther(value);
				}
			}

			private void SerializeObject(IDictionary<string, object> obj)
			{
				bool first = true;
				
				if(prettyPrint)
				{
					if(depth > 0) 
					{
						WriteIndentation();
					}
					depth++;
				}
				
				builder.Append('{');
				if(prettyPrint) 
				{
					WriteIndentation();
				}
				foreach(KeyValuePair<string, object> pair in obj)
				{
					if(!first)
					{
						builder.Append(", ");
						if(prettyPrint) 
						{
							WriteIndentation();
						}
					}
					
					SerializeString(pair.Key);
					builder.Append(": ");
					SerializeValue(pair.Value);
					first = false;
				}
				
				if(prettyPrint) 
				{
					depth--;
					WriteIndentation();
				}
				builder.Append ('}');
			}

			private void SerializeArray(IList<object> anArray)
			{
				bool first = true;
				
				if(prettyPrint) 
				{
					depth++;
				}
				
				builder.Append ('[');
				if(prettyPrint) 
				{
					WriteIndentation();
				}
				foreach (object obj in anArray) 
				{
					if (!first) 
					{
						builder.Append (", ");
						if(prettyPrint) 
						{
							WriteIndentation();
						}
					}

					SerializeValue(obj);
					first = false;
				}
				
				if(prettyPrint) 
				{
					depth--;
					WriteIndentation();
				}
				builder.Append (']');
			}

			private void SerializeString(string str)
			{
				builder.Append('\"');

				char[] charArray = str.ToCharArray();
				foreach(var c in charArray) 
				{
					switch(c) 
					{
					case '"':
						builder.Append("\\\"");
						break;
					case '\\':
						builder.Append("\\\\");
						break;
					case '\b':
						builder.Append("\\b");
						break;
					case '\f':
						builder.Append("\\f");
						break;
					case '\n':
						builder.Append("\\n");
						break;
					case '\r':
						builder.Append("\\r");
						break;
					case '\t':
						builder.Append("\\t");
						break;
					default:
						int codepoint = System.Convert.ToInt32(c);
						if((codepoint >= 32) && (codepoint <= 126)) 
						{
							builder.Append(c);
						} 
						else 
						{
							builder.Append("\\u");
							builder.Append(codepoint.ToString("x4"));
						}
						break;
					}
				}

				builder.Append('\"');
			}

			private void SerializeOther(object value)
			{
				// NOTE: decimals lose precision during serialization.
				// They always have, I'm just letting you know.
				// Previously floats and doubles lost precision too.
				if(value is float) 
				{
					builder.Append(((float)value).ToString ("R"));
				} 
				else if(value is int || value is uint || value is long || value is sbyte || 
						 value is byte || value is short || value is ushort || value is ulong) 
				{
					builder.Append(value);
				} 
				else if(value is double || value is decimal) 
				{
					builder.Append(System.Convert.ToDouble(value).ToString("R"));
				} 
				else
				{
					SerializeString(value.ToString());
				}
			}
			
			private void WriteIndentation()
			{
				builder.Append('\n');
				for(int i=0; i< depth; i++)
				{
					builder.Append('\t');
				}
			}
		}
	}
}
