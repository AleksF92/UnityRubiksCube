using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RubiksCube))]
public class RubiksCubeEditor : Editor
{
    private int selectedIndex;
    private RubiksCube cube;

    public override void OnInspectorGUI()
    {
        cube = (RubiksCube)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        DrawDefaultInspector();
        
        if (GUILayout.Button("Generate"))
        {
            cube.GenerateCube();
        }
        if (GUILayout.Button("Shuffle"))
        {
            cube.Shuffle();
        }
        if (GUILayout.Button("Undo Last"))
        {
            cube.UndoLastMove();
        }
        if (GUILayout.Button("Undo All"))
        {
            cube.UndoAllMoves();
        }
        if (GUILayout.Button("Reset"))
        {
            cube.ResetCube();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rotate", EditorStyles.boldLabel);
        if (cube.GeneratedCubeSize <= 0)
        {
            EditorGUILayout.HelpBox("Cube is not generated yet!", MessageType.Warning);
            if (cube.transform.childCount > 0)
            {
                cube.ClearCube();
            }
        }
        else
        {
            IndexSelectButtons();
            SideButtons("X Axis", new RubiksCube.Move(new Vector3Int(1, 0, 0), selectedIndex));
            SideButtons("Y Axis", new RubiksCube.Move(new Vector3Int(0, 1, 0), selectedIndex));
            SideButtons("Z Axis", new RubiksCube.Move(new Vector3Int(0, 0, 1), selectedIndex));
        }
    }

    private void IndexSelectButtons()
    {
        EditorGUILayout.LabelField("Select layer index", EditorStyles.centeredGreyMiniLabel);

        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < cube.GeneratedCubeSize; i++)
        {
            if (i == selectedIndex)
            {
                if (GUILayout.Button(i.ToString(), EditorStyles.toolbarButton))
                {
                    selectedIndex = i;
                }
            }
            else
            {
                if (GUILayout.Button(i.ToString()))
                {
                    selectedIndex = i;
                }
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void SideButtons(string label, RubiksCube.Move move)
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("<---"))
        {
            move.FlipDirection();
            cube.ApplyCubeMove(move);
        }

        EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel);

        if (GUILayout.Button("--->"))
        {
            cube.ApplyCubeMove(move);
        }

        EditorGUILayout.EndHorizontal();
    }
}