using System;
using Assets.Scripts.MapGenerator.MapGeneratorBehaviours;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.MapGenerator
{
	[CustomEditor(typeof(UltraMapGenerator))]
	public class UltraMapGeneratorEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			var myTarget = (UltraMapGenerator)target;

			GUILayout.Space(10);

			if (GUILayout.Button("Generate landscape"))
			{
				try
				{
					foreach (var s in myTarget.Generate())
						EditorUtility.DisplayProgressBar("Progress: ", s.content, s.time);
				}
				finally
				{
					EditorUtility.ClearProgressBar();
				}
			}
		}
	}
}
