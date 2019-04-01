using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

/*
[CustomEditor(typeof(PatternAllowanceTest))]
public class PatternAllowanceTestInspector : OdinEditor
{
    int value = 1;
    private Vector3Int coordinate = Vector3Int.zero;
    
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PatternAllowanceTest patternTest = (PatternAllowanceTest) target;

        if (GUILayout.Button("Set Patterns"))
        {
            patternTest.SetPatterns();
        }

        value = EditorGUILayout.IntField("Pattern Index: ", value);
        
        if (GUILayout.Button("Spawn Pattern"))
        {
            patternTest.SpawnPattern(value);
        }
        
        coordinate = EditorGUILayout.Vector3IntField("Coordinate ", coordinate);
        
        if (GUILayout.Button("Collapse coordinate"))
        {
            patternTest.CollapseCoordinate(coordinate);
        }
        

    }
}

public class PatternAllowanceTest : SerializedMonoBehaviour
{
    [SerializeField] private TrainingScript training;
    [SerializeField] private List<Pattern> patterns;

    [SerializeField] private Vector3Int outputSize = new Vector3Int(6, 6, 6);
    private bool[,,] wave;
    

    public void SetPatterns()
    {
        patterns = training.Patterns;
    }

    public void SpawnPattern(int index)
    {
        wave = new bool[outputSize.x,outputSize.y,outputSize.z];

        for (int x = 0; x < wave.GetLength(0); x++)
        {
            for (int y = 0; y < wave.GetLength(1); y++)
            {
                for (int z = 0; z < wave.GetLength(2); z++)
                {
                    wave[x, y, z] = true;
                }
            }
        }
        

        if (!(index >= 0 && index < patterns.Count)) index = 0;

        Pattern selectedPattern = patterns[index];


        for (int i = this.transform.childCount; i > 0; --i)
        {
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }

        GameObject newPattern = new GameObject("Pattern");
        newPattern.transform.parent = this.transform;

        newPattern.transform.localPosition = Vector3.zero;

        TrainingData[,,] data = selectedPattern.MatrixData;

        for (int x = 0; x < data.GetLength(0); x++)
        {
            for (int y = 0; y < data.GetLength(1); y++)
            {
                for (int z = 0; z < data.GetLength(2); z++)
                {
                    if (data[x, y, z].GameObjectIndex != training.nullIndex)
                    {
                        GameObject patternData = Instantiate(training.IdToPrefab(data[x, y, z].GameObjectIndex), new Vector3(x,y,z),
                            Quaternion.Euler(data[x, y, z].LocalRotation), newPattern.transform);
                        patternData.transform.localPosition = new Vector3(outputSize.x / 3 + x, outputSize.y/ 3 + y, outputSize.z/ 3 + z);
                        wave[outputSize.x / 3 +x, outputSize.y / 3+y, outputSize.z / 3+z] = false;

                    }
                }
            }
        }
        
    }

    public void CollapseCoordinate(Vector3Int coordinate)
    {
        
    }

    private void OnDrawGizmos()
    {
        if (!(wave.GetLength(0) > 0 && wave.GetLength(1) > 0 && wave.GetLength(2) > 0)) return;
        
        for (int x = 0; x < wave.GetLength(0); x++)
        {
            for (int y = 0; y < wave.GetLength(1); y++)
            {
                for (int z = 0; z < wave.GetLength(2); z++)
                {
                    Color redColor = Color.red;
                    redColor.a = 0.75F;

                    Color greenColor = Color.green;
                    greenColor.a = 0.15F;
                    
                    Gizmos.color = wave[x, y, z] ? greenColor : redColor;
                    Gizmos.DrawSphere(transform.position + new Vector3(x,y,z), 0.25F);
                }
            }
        }
    }
}
*/

