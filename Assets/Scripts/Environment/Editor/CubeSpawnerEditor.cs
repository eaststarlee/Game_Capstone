// CubeSpawner의 인스펙터에 Generate/ Clear 버튼을 추가하는 커스텀 에디터
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubeSpawner))]
public class CubeSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CubeSpawner spawner = (CubeSpawner)target;

        if (GUILayout.Button("Generate Cubes (큐브 생성)"))
        {
            spawner.GenerateCubes();
        }

        if (GUILayout.Button("Clear Cubes (모두 삭제)"))
        {
            spawner.ClearCubes();
        }
    }
}