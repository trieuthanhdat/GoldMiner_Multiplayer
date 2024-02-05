using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Asteroids.HostAdvanced
{
	// Utility class to set up the Level Boundaries values
	[CustomEditor(typeof(LevelBoundaries))]
	public class LevelBoundariesEditor : Editor
	{
		BoxBoundsHandle _boundaryHandle = new BoxBoundsHandle();

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			LevelBoundaries levelBoundaries = (LevelBoundaries) target;
			if (GUILayout.Button("Set Boundaries based on Camera"))
			{
				levelBoundaries.SetBoundariesBasedOnCamera();
			}
		}

		protected virtual void OnSceneGUI()
		{
			LevelBoundaries levelBoundaries = (LevelBoundaries) target;

			// Create the handle based on the current information available in the LevelBoundary

			_boundaryHandle.center = levelBoundaries.transform.position;
			_boundaryHandle.size = new Vector3(levelBoundaries.LevelBoundaryX, levelBoundaries.LevelBoundaryY);
			_boundaryHandle.SetColor(Color.yellow);

			// draw the handle
			EditorGUI.BeginChangeCheck();
			_boundaryHandle.DrawHandle();

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(levelBoundaries, "Changed Level Boundaries");

				Bounds newBounds = new Bounds
				{
					center = _boundaryHandle.center,
					size = _boundaryHandle.size
				};

				levelBoundaries.SetBoundariesBasedOnBounds(newBounds);
			}
		}
	}
}