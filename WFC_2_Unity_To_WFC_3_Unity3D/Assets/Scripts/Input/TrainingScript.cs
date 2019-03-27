using System.Collections.Generic;
using System.Linq.Expressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

// ReSharper disable All

[CustomEditor(typeof(TrainingScript))]
public class TrainingScriptInspector : OdinEditor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        TrainingScript training = (TrainingScript)target;
            
        if (GUILayout.Button("Start WFC")){
            //training.WaveFunctionCollapse();
        }
        
        if (GUILayout.Button("Draw Patterns")){
           // training.DrawPatterns();
        }
    }
}

[System.Serializable]
public struct Module{

    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3Int rotationEuler;
    
    public GameObject Prefab{
        get{ return prefab; }
        set{ prefab = value; }
    }

    public Vector3Int RotationEuler{
        get{ return rotationEuler; }
        set{ rotationEuler = value; }
    }

    public Module(GameObject prefab, Vector3Int rotationEuler){
        this.prefab = prefab;
        this.rotationEuler = rotationEuler;
    }
}

[ExecuteInEditMode]
public class TrainingScript : SerializedMonoBehaviour{

    [SerializeField] private Dictionary<Vector3Int, Module> ChildrenByCoordinate = new Dictionary<Vector3Int, Module>();
    [SerializeField] private Dictionary<int, GameObject> PrefabAndId = new Dictionary<int, GameObject>();
    [SerializeField] private List<Pattern> Patterns = new List< Pattern>();

    [SerializeField] private GameObject DisplayPatternObject;

    private Matrix < Module > ModuleMatrix;

    private InputGriddify Input;

    private void Update(){
        TranslatePrefabsToId();

    }

    public void TranslatePrefabsToId(){
        ClearPreviousData();
        
        GetResources();
        
        Input = GetComponent<InputGriddify>();

        AssignCoordinateToChildren();
        
        InitializeMatrix();

        DefinePatterns();
        
        DisplayPatterns();
    }
    
    private void ClearPreviousData(){
        ChildrenByCoordinate = new Dictionary<Vector3Int, Module>();
        PrefabAndId = new Dictionary < int, GameObject >();
        ModuleMatrix = new Matrix < Module >(Vector3Int.zero);
        Patterns = new List < Pattern >();

    }

    private void GetResources(){
        GameObject[] Prefabs = Resources.LoadAll<GameObject>("Wfc");

        for ( int i = 0; i < Prefabs.Length; i++ ){
            PrefabAndId.Add(i, Prefabs[i]);
        }
    }

    private void AssignCoordinateToChildren(){
        for ( int i = 0; i < transform.childCount; i++ ){
            Transform childTransform = transform.GetChild(i);
            
            ChildrenByCoordinate.Add(
                V3toV3I(childTransform.localPosition), 
                new Module(
                    (GameObject)PrefabUtility.GetPrefabParent(childTransform.gameObject),
                    V3toV3I(childTransform.localEulerAngles)
                )
            );
        }
    }

    static Vector3Int V3toV3I(Vector3 Input){
        return new Vector3Int(Mathf.RoundToInt(Input.x), Mathf.RoundToInt(Input.y), Mathf.RoundToInt(Input.z));
    }

    private void InitializeMatrix(){
        ModuleMatrix = new Matrix < Module >(Input.inputSize);

        for ( int x = 0; x < ModuleMatrix.SizeX; x++ ){
            for ( int y = 0; y < ModuleMatrix.SizeY; y++ ){
                for ( int z = 0; z < ModuleMatrix.SizeZ; z++ ){
                    
                    Module module;
                    
                    if (ChildrenByCoordinate.TryGetValue(new Vector3Int(x,y,z), out module)){
                        ModuleMatrix.MatrixData[x, y, z] = module;
                    }
                }
            }
        }
    }
    
    private void DefinePatterns(){
        int n = Input.NValue;
        
        if ( n > 0 ){
            
            for ( int x = 0; x < Input.inputSize.x; x+=n ){
                for ( int y = 0; y < Input.inputSize.y ; y+=n ){
                    for ( int z = 0; z < Input.inputSize.z; z+=n ){
                        Module[,,] newTrainingData = new Module[n,n,n];

                        bool bIsNull = true;
                        
                        for ( int nx = 0; nx < n; nx++ ){
                            for ( int ny = 0; ny < n; ny++ ){
                                for ( int nz = 0; nz < n; nz++ ){
                                    Module module;

                                    if ( ChildrenByCoordinate.TryGetValue(new Vector3Int(x + nx, y + ny, z + nz), out module) ){
                                        newTrainingData[nx, ny, nz] = module;
                                        bIsNull = false;
                                    }
                                }
                            }
                        }

                        if ( !bIsNull ){
                            Pattern newPattern = new Pattern(n, newTrainingData, new Vector3Int(x, y, z));
                            Patterns.Add(newPattern);

                            for ( int i = 1; i < 4; i++ ){
                                Pattern rotatedPattern = new Pattern(n, newTrainingData, new Vector3Int(x, y, z));
                                rotatedPattern.RotatePatternCounterClockwise(i);
                                Patterns.Add(rotatedPattern);
                            }
                        }
                    }
                }
            }
        }
    }

    private void DisplayPatterns(){

        if ( DisplayPatternObject ){

            for ( int i = DisplayPatternObject.transform.childCount; i > 0; --i ){
                DestroyImmediate(DisplayPatternObject.transform.GetChild(0).gameObject);
            }

            int index = 0;
            int patternCount = 0;

            foreach ( Pattern pattern in Patterns ){
                GameObject newPattern = new GameObject("Pattern");

                newPattern.transform.localPosition = Vector3.zero + ( index * 3 ) * Vector3.left;
                newPattern.transform.parent = DisplayPatternObject.transform;

                Module[,,] data = pattern.MatrixData;

                for ( int x = 0; x < data.GetLength(0); x++ ){
                    for ( int y = 0; y < data.GetLength(1); y++ ){
                        for ( int z = 0; z < data.GetLength(2); z++ ){
                            if ( data[x, y, z].Prefab != null ){
                                GameObject patternData = Instantiate(data[x, y, z].Prefab, newPattern.transform);
                                patternData.transform.localPosition = new Vector3(x, y, z );
                                patternData.transform.localEulerAngles = data[x, y, z].RotationEuler;

                            }
                        }
                    }
                }

                index++;
            }
        }
    }
}