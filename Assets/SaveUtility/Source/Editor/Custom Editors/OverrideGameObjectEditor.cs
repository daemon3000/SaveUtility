using UnityEditor;
using UnityEngineInternal;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;
using TeamUtility.IO.SaveUtility;

namespace TeamUtility.Editor.IO.SaveUtility
{
	[System.AttributeUsage(System.AttributeTargets.Class)]
	internal class OverrideInternalEditorTypeMarkAttribute:System.Attribute
	{
		public System.Type type;
		public OverrideInternalEditorTypeMarkAttribute(string typeName)
		{
			var types = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetTypes();
			foreach(var type in types)
			{
				if(type.Name != typeName)
					continue;
				
				this.type = type;
				break;
			}
			if(this.type == null)
				Debug.LogError("Type not found:"  + typeName);
		}
	}

	public class OverrideInternalEditor : UnityEditor.Editor
	{
		public UnityEditor.Editor _baseEditor;
		public UnityEditor.Editor baseEditor
		{
			get
			{
				if(_baseEditor == null)
				{
					var atr = System.Attribute.GetCustomAttribute(this.GetType(), typeof(OverrideInternalEditorTypeMarkAttribute)) as OverrideInternalEditorTypeMarkAttribute;
					if(atr != null)
					{
						if (this.targets != null && this.targets.Length > 0)
							_baseEditor = CreateEditor(this.targets, atr.type);
					}
				}
				return _baseEditor;
			}
		}
		
		private void OnEnable()
		{
		}
		
		private void OnDisable()
		{
			DestroyImmediate(_baseEditor);
		}
		
		protected override void OnHeaderGUI()
		{
			var method = baseEditor.GetType().GetMethod("OnHeaderGUI",  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			method.Invoke(baseEditor, new object[0]);
		}
		
		public override GUIContent GetPreviewTitle()
		{
			if(target == null)
				return null;
			return baseEditor.GetPreviewTitle();
		}
		
		public override string GetInfoString()
		{
			if(target == null)
				return "";
			return baseEditor.GetInfoString();
		}
		
		public override bool HasPreviewGUI()
		{
			if(target == null)
				return false;
			return baseEditor.HasPreviewGUI();
		}
		
		public override void OnInspectorGUI()
		{
			if(target == null)
				return;
			baseEditor.OnInspectorGUI();
		}
		
		public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
		{
			if(target == null)
				return;
			baseEditor.OnInteractivePreviewGUI(r, background);
		}
		
		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			if(target == null)
				return;
			baseEditor.OnPreviewGUI(r, background);
		}
		
		public override void OnPreviewSettings()
		{
			if(target == null)
				return;
			baseEditor.OnPreviewSettings();
		}
		
		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			if(target == null)
				return null;
			
			return baseEditor.RenderStaticPreview(assetPath, subAssets, width, height);
		}
	}

	[CustomEditor(typeof(GameObject))]
	[CanEditMultipleObjects]
	[OverrideInternalEditorTypeMark("GameObjectInspector")]
	public class GameObjectEditorOverride : OverrideInternalEditor
	{
		private bool _isPersistent;
		private bool _hasPrefab;

		private void OnEnable()
		{
			_isPersistent = EditorUtility.IsPersistent(target);

			var targetRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab((GameObject)target);
			_hasPrefab = PrefabUtility.GetPrefabParent(targetRoot) != null;
		}

		protected override void OnHeaderGUI()
		{
			base.OnHeaderGUI();

			if(!_isPersistent)
			{
				if(_hasPrefab)
				{
					UniqueIdentifier uid = ((GameObject)target).GetComponent<UniqueIdentifier>();
					if(uid != null)
					{
						EditorGUILayout.HelpBox(string.Format("Do not use the Revert and Apply buttons. Use the buttons provided by the {0} component.", uid.GetType().Name), MessageType.Warning);
					}
				}
			}
		}
	}
}