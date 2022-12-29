using UnityEngine;
using UnityEditor;
using System.Collections;

namespace BBG.WordSearch
{
	[CustomEditor(typeof(WordListLayoutGroup))]
	public class WordListLayoutGroupEditor : Editor
	{
		#region Unity Methods

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Padding"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("spacing"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rows"));

			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();
		}

		#endregion
	}
}
