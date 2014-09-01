using UnityEngine;
using System;
using System.Collections;

namespace TeamUtility.IO.SaveUtility
{
	public static class PathHelper
	{
		public static string Combine(string pathA, string pathB)
		{
#if UNITY_STANDALONE || UNITY_EDITOR
			return System.IO.Path.Combine(pathA, pathB);
#else
			bool pathAEndsWithSlash = pathA[pathA.Length - 1] == '/' || pathA[pathA.Length - 1] == '\\';
			bool pathBStartsWithSlash = pathB[0] == '/' || pathB[0] == '\\';

			if(pathAEndsWithSlash && pathBStartsWithSlash)
			{
				return pathA + pathB.Substring(1);
			}
			else if(pathAEndsWithSlash || pathBStartsWithSlash)
			{
				return pathA + pathB;
			}
			else
			{
				return pathA + "\\" + pathB;
			}
#endif
		}

		public static string ChangeExtension(string path, string extension)
		{
			if(path == null)
				throw new ArgumentNullException("path");
			if(extension == null)
				throw new ArgumentNullException("extension");

#if UNITY_STANDALONE || UNITY_EDITOR
			return System.IO.Path.ChangeExtension(path, extension);
#else
			int lastIndex = path.LastIndexOf('.');
			if(lastIndex != -1)
			{
				if(extension[0] == '.')
				{
					return path.Substring(0, lastIndex) + extension;
				}
				else
				{
					if(lastIndex == path.Length - 1)
					{
						return path.Substring(0, lastIndex) + "." + extension;
					}
					else
					{
						return path.Substring(0, lastIndex + 1) + extension;
					}

				}
			}
			else
			{
				if(path.Length > 0 && extension.Length > 0)
				{
					if(extension[0] == '.')
					{
						return path + extension;
					}
					else
					{
						return path + "." + extension;
					}
				}
				else
				{
					return path;
				}
			}
#endif
		}
	}
}