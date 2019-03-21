using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrainingScript))]
public class TrainingScriptInspector : OdinEditor
{

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        TrainingScript training = (TrainingScript)target;
            
        if (GUILayout.Button("[1] Translate Prefabs to ID")) {
            training.TranslatePrefabsToId();
        }
    }
}

public class TrainingScript : SerializedMonoBehaviour
{
    [SerializeField] private List<GameObject> resources;
    [SerializeField] private Dictionary<string, GameObject> conversionDictionary = new Dictionary<string, GameObject>();
    [SerializeField] private Dictionary<Vector3Int, GameObject> children = new Dictionary<Vector3Int, GameObject>();


    private InputGriddify _input;

    private GameObject[,,] _inputMatrix;
    private string[,,] _inputStringMatrix;


    private char DefineNewChar(int index)
    {
        return "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[index];
    }

    private string PrefabToString(GameObject prefab)
    {
        foreach (KeyValuePair<string, GameObject> entry in conversionDictionary)
        {
            if (entry.Value == prefab.gameObject)
            {
                return entry.Key;
            }
        }

        return "";
    }

    private GameObject StringToPrefab(string s)
    {
        conversionDictionary.TryGetValue(s, out var prefab);
        return prefab;
    }

    private GameObject GetResourceById(string id)
    {
        foreach (GameObject g in resources)
        {
            if (g.GetComponent<UniqueId>().uniquePrefabId == id)
            {
                return g;
            }
        }

        return null;
    }

    public void TranslatePrefabsToId()
    {
        conversionDictionary = new Dictionary<string, GameObject>();

        _input = GetComponent<InputGriddify>();

        if (!_input)
        {
            Debug.LogError("No input (using InputGriddify)! Aborting.)");
            return;
        }

        InitializeMatrixValues();

        DefineMatrixSize();
        GameObjectMatrixToStringMatrix();

    }

    private void InitializeMatrixValues()
    {
        children.Clear();
        conversionDictionary.Clear();

        int index = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 location = transform.GetChild(i).transform.localPosition;
            Vector3Int gridLocation = new Vector3Int(
                Mathf.RoundToInt(location.x),
                Mathf.RoundToInt(location.y),
                Mathf.RoundToInt(location.z)
            );

            transform.GetChild(i).transform.name = gridLocation.ToString();
            children.Add(gridLocation, transform.GetChild(i).gameObject);
        }


        for (int x = 0; x < _input.inputSize.x; x++)
        {
            for (int y = 0; y < _input.inputSize.y; y++)
            {
                for (int z = 0; z < _input.inputSize.z; z++)
                {
                    if (!children.ContainsKey(new Vector3Int(x, y, z)))
                    {
                        children.Add(new Vector3Int(x, y, z), resources[0].gameObject);
                    }
                }
            }
        }
    }


    private void DefineMatrixSize()
    {
        // Define matrix size
        _inputMatrix = new GameObject[_input.inputSize.x, _input.inputSize.y, _input.inputSize.z];

        for (int x = 0; x < _input.inputSize.x; x++)
        {
            for (int y = 0; y < _input.inputSize.y; y++)
            {
                for (int z = 0; z < _input.inputSize.z; z++)
                {
                    // Assign children to matrix location based on their 2D coordinates.
                    children.TryGetValue(new Vector3Int(x, y, z), out var child);
                    _inputMatrix[x, y, z] = child;
                }
            }
        }
    }

    private void GameObjectMatrixToStringMatrix()
    {
        int maxX = _input.inputSize.x, maxY = _input.inputSize.y, maxZ = _input.inputSize.z;

        _inputStringMatrix = new string[maxX, maxY, maxZ];

        HashSet<string> currentIds = new HashSet<string>();

        int index = 0;

        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                for (int z = 0; z < maxZ; z++)
                {
                    if (currentIds.Add(_inputMatrix[x, y, z].GetComponent<UniqueId>().uniquePrefabId))
                    {
                        string newChar = DefineNewChar(index).ToString();
                        conversionDictionary.Add(newChar,
                            GetResourceById(_inputMatrix[x, y, z].GetComponent<UniqueId>().uniquePrefabId));
                        index++;
                    }
                }
            }
        }

        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                for (int z = 0; z < maxZ; z++)
                {
                    _inputStringMatrix[x, y, z] =
                        PrefabToString(GetResourceById(_inputMatrix[x, y, z].GetComponent<UniqueId>().uniquePrefabId));
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (KeyValuePair<Vector3Int, GameObject> pair in children)
        {
            if (pair.Value)
            {
                if (pair.Value != resources[0].gameObject)
                {
                    Handles.Label(pair.Value.transform.position, _inputStringMatrix[pair.Key.x, pair.Key.y, pair.Key.z]);
                }
            }
        }
    }
}