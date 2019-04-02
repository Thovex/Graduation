using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using static Thovex.Utility;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

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

        EditorGUILayout.BeginHorizontal("Button");

        value = EditorGUILayout.IntField("Pattern Index: ", value);
        
        if (GUILayout.Button("Spawn Pattern"))
        {
            patternTest.SpawnPattern(value);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal("Button");
        
        EditorGUILayout.BeginVertical("Button");

        if (GUILayout.Button("MoveUp", GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.UP);
        }
        
        if (GUILayout.Button("MoveDown",  GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.DOWN);
        }
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Button");

        if (GUILayout.Button("MoveLeft",  GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.LEFT);
        }
        
        if (GUILayout.Button("MoveRight", GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.RIGHT);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Button");

        if (GUILayout.Button("MoveForward", GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.FORWARD);
        }
        
        if (GUILayout.Button("MoveBack", GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.BACK);
        }
        EditorGUILayout.EndVertical();
      
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Collapse coordinate",  GUILayout.Height(100)))
        {
            patternTest.CollapseCoordinate();
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
        
        For3(_wave, (x, y, z) => { _wave.MatrixData[x, y, z] = true; });
        
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

        For3(selectedPattern, (x, y, z) => {
            GameObject patternData = Instantiate(selectedPattern.MatrixData[x, y, z].Prefab, new Vector3(x, y, z), Quaternion.Euler(selectedPattern.MatrixData[x, y, z].RotationEuler), newPattern.transform);
            patternData.transform.localPosition = new Vector3(_outputSize.x / 3 + x,  y, _outputSize.z/ 3 + z);
            _wave.MatrixData[_outputSize.x / 3 +x, y, _outputSize.z / 3+z] = false;
        });
        
        _coordinateMatrix = new Matrix < Vector3Int >(new Vector3Int(_training.N, _training.N, _training.N));
        
        For3(_coordinateMatrix, (x, y, z) => {
            _coordinateMatrix.MatrixData[x,y,z] = new Vector3Int(x,y,z);
        });        
    }

    public void Move(EOrientations orientation){
        
        
        For3(_coordinateMatrix, (x, y, z) => {
            switch ( orientation ){
                case EOrientations.FORWARD:
                    _coordinateMatrix.MatrixData[x,y,z] += V3ToV3I(Vector3.forward);
                    break;
                case EOrientations.BACK:
                    _coordinateMatrix.MatrixData[x,y,z] += V3ToV3I(Vector3.back);
                    break;
                case EOrientations.RIGHT:
                    _coordinateMatrix.MatrixData[x,y,z] += Vector3Int.right;
                    break;
                case EOrientations.LEFT: 
                    _coordinateMatrix.MatrixData[x,y,z] += Vector3Int.left;
                    break;
                case EOrientations.UP:
                    _coordinateMatrix.MatrixData[x,y,z] += Vector3Int.up;
                    break;
                case EOrientations.DOWN:
                    _coordinateMatrix.MatrixData[x,y,z] += Vector3Int.down;
                    break;
                case EOrientations.NULL:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
            }
        });
    }

    public void CollapseCoordinate(){

    }

    private void OnDrawGizmos()
    {
        try{
            For3(_outputSize, (x, y, z) => {
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
            
            
            For3( _coordinateMatrix, (x, y, z) => {
                Gizmos.DrawSphere(transform.position + _coordinateMatrix.MatrixData[x,y,z], 0.25F);
            });
            
        }
        catch ( Exception e ){
            InitWave();
        }
    }
}


