using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using static Thovex.Utility;

[CustomEditor(typeof(PatternAllowanceTest))]
public class PatternAllowanceTestInspector : OdinEditor
{
    int value = 1;    
    
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
        
        
        if (GUILayout.Button("Collapse Random coordinate"))
        {
            patternTest.CollapseRandomCoordinate();
        }
        

    }
}

public class PatternAllowanceTest : SerializedMonoBehaviour
{
    [SerializeField] private TrainingScript _training;
    [SerializeField] private List<Pattern> _patterns;

    [SerializeField] private Vector3Int _outputSize = new Vector3Int(6, 6, 6);
    private Matrix < bool > _wave = new Matrix < bool >(Vector3Int.zero);
    private Matrix <Vector3Int> _coordinateMatrix = new Matrix < Vector3Int >(Vector3Int.zero);

    private Vector3Int _collapsingCoordinate = Vector3Int.zero;
    private string _bitToSpawn = "";

    [SerializeField] private Pattern _patternToSpawn;
    

    public void SetPatterns(){
        _patterns = _training.Patterns.ToList();
        InitWave();
    }

    private void InitWave(){
        _wave = new Matrix < bool >(_outputSize);
        
        Nested3(_wave, (x, y, z) => { _wave.MatrixData[x, y, z] = true; });
        
        for (int i = this.transform.childCount; i > 0; --i)
        {
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }
    }

    public void SpawnPattern(int index){
        InitWave();

        if (!(index >= 0 && index < _patterns.Count)) index = 0;

        Pattern selectedPattern = _patterns[index];

        GameObject newPattern = new GameObject("Pattern");
        newPattern.transform.parent = this.transform;

        newPattern.transform.localPosition = Vector3.zero;

        Nested3(selectedPattern, (x, y, z) => {
            GameObject patternData = Instantiate(selectedPattern.MatrixData[x, y, z].Prefab, new Vector3(x, y, z), Quaternion.Euler(selectedPattern.MatrixData[x, y, z].RotationEuler), newPattern.transform);
            patternData.transform.localPosition = new Vector3(_outputSize.x / 3 + x,  y, _outputSize.z/ 3 + z);
            _wave.MatrixData[_outputSize.x / 3 +x, y, _outputSize.z / 3+z] = false;
        });

        
    }

    public void CollapseRandomCoordinate(){
        List < Vector3Int > possibleCoordinates = new List < Vector3Int >();

        Nested3(_wave, (x, y, z) => {
            if ( !_wave.MatrixData[x, y, z] ){
                if ( y == 1 ){
                    possibleCoordinates.Add(new Vector3Int(x, y, z));
                }
            }
        });


        if ( possibleCoordinates.Count > 0 ){
            int tries = 0;
            
            while( true ){
                Vector3Int coordinate = possibleCoordinates[UnityEngine.Random.Range(0, possibleCoordinates.Count - 1)];

                _coordinateMatrix = new Matrix < Vector3Int >(new Vector3Int(_training.N, _training.N, _training.N));
                _coordinateMatrix.MatrixData[0, 0, 0] = coordinate;

                Nested3(_coordinateMatrix, (x, y, z) => {
                    bool randDir = UnityEngine.Random.Range(0, .5F) > 0.5;

                    if ( randDir ){
                        _coordinateMatrix.MatrixData[x, y, z] = coordinate + new Vector3Int(x, y, z);
                    }
                    else{
                        _coordinateMatrix.MatrixData[x, y, z] = coordinate - new Vector3Int(x, y, z);
 
                    }
                });

                int overlapCount = 0;

                List < Vector3Int > closedCoordinates = new List < Vector3Int >();
                
                Nested3(_wave, (x, y, z) => {
                    if ( !_wave.MatrixData[x, y, z] ){
                        closedCoordinates.Add(new Vector3Int(x,y,z));
                    }
                });

                Nested3(_coordinateMatrix, (x, y, z) => {
                    if ( closedCoordinates.Contains(_coordinateMatrix.MatrixData[x,y,z]) ){
                        overlapCount++;
                    }
                });
                
                if ( overlapCount == 2 ){
                    break;
                }

                tries++;

                if ( tries > 100 ){
                    Debug.LogError("Blyad");
                    break;
                }
            }
        }

    }

    private void OnDrawGizmos()
    {
        try{
            Nested3(_outputSize, (x, y, z) => {
                Color redColor = Color.red;
                redColor.a = 0.75F;

                Color greenColor = Color.green;
                greenColor.a = 0.15F;

                Gizmos.color = _wave.MatrixData[x, y, z] ? greenColor : redColor;
                Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), 0.25F);
            });

            Color magentaColor = Color.magenta;
            magentaColor.a = 0.75F;
            
            Gizmos.color = magentaColor;
            
            
            Nested3( _coordinateMatrix, (x, y, z) => {
                Gizmos.DrawSphere(transform.position + _coordinateMatrix.MatrixData[x,y,z], 0.25F);
               // Handles.Label(transform.position + new Vector3(x,y,z), _bitToSpawn);
            });
            
        }
        catch ( Exception e ){
            InitWave();
        }
    }
}


