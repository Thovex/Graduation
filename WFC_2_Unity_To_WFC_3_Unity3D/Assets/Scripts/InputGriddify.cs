using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class InputGriddify : MonoBehaviour
{
    public Vector3Int inputSize = new Vector3Int(5, 5, 5);
    [SerializeField] private int _nValue = 2;
    
    private GameObject[,,] _inputMatrix;
    private bool[,,] _inputMatrixSet;
    private bool[,,] _inputMatrixWarnings;
    

    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.update += SnapToGrid;
        EditorApplication.update += CheckGridAvailability;
#endif
    }
    
    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= SnapToGrid;
        EditorApplication.update -= CheckGridAvailability;
#endif
    }

    private void Update()
    {
        SnapToGrid();
        CheckGridAvailability();
    }
    private void CheckGridAvailability()
    {
        _inputMatrix = new GameObject[inputSize.x, inputSize.y, inputSize.z];
        _inputMatrixSet = new bool[inputSize.x, inputSize.y, inputSize.z];
        _inputMatrixWarnings = new bool[inputSize.x, inputSize.y, inputSize.z];
        
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject childObject = transform.GetChild(i).gameObject;
            Vector3 childLocalPosition = childObject.transform.localPosition;

            Vector3Int childLocalPositionRounded = new Vector3Int(     
                Mathf.RoundToInt(childLocalPosition.x),
                Mathf.RoundToInt(childLocalPosition.y),
                Mathf.RoundToInt(childLocalPosition.z)
             );
            
            if ( _inputMatrixSet[childLocalPositionRounded.x, childLocalPositionRounded.y, childLocalPositionRounded.z] ) {
                _inputMatrixWarnings[childLocalPositionRounded.x, childLocalPositionRounded.y, childLocalPositionRounded.z] = true;
            }
            
            _inputMatrixSet[
                childLocalPositionRounded.x,
                childLocalPositionRounded.y,
                childLocalPositionRounded.z
            ] = true;
        }

    }

    private void SnapToGrid()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject childObject = transform.GetChild(i).gameObject;

            Vector3 childLocalPosition = childObject.transform.localPosition;
            Vector3 childLocalEulerAngles = childObject.transform.localEulerAngles;

            childObject.transform.localPosition = new Vector3(
                Mathf.FloorToInt(childLocalPosition.x),
                Mathf.FloorToInt(childLocalPosition.y),
                Mathf.FloorToInt(childLocalPosition.z)
            );

            childObject.transform.localEulerAngles = new Vector3(
                0,
                Clamp90(childLocalEulerAngles.y),
                0
            );
        }
    }

    private static float Clamp90(float value)
    {
        float[] angles = new float[] { 0, 90, 180, 270, 360 };

        for (int i = 0; i < angles.Length - 1; i++)
        {
            if (value > angles[i] && value < angles[i] + 45) value = angles[i];
            if (value >= angles[i] + 45 && value < angles[i + 1]) value = angles[i + 1];
        }
        return value;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(.75f, .75f, .75f, .15f);
        
        for (int x = 0; x < inputSize.x; x++)
        {
            for (int y = 0; y < inputSize.y; y++)
            {
                for (int z = 0; z < inputSize.z; z++)
                {
                    if (_inputMatrixSet[x, y, z])
                    {
                        Gizmos.color = new Color(0f, 0f, 1f, .15f);
                    }
                    else
                    {
                        Gizmos.color = new Color(0f, 1f, 0f, .05f);
                    }

                    if ( _inputMatrixWarnings[x, y, z] ){
                        Gizmos.color = Color.red;
                    }

                    float sphereSize = _inputMatrixWarnings[x, y, z] ? .5F : .15F;
                    
                    Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), sphereSize);
                }
            }
        }

        Gizmos.color = new Color(1F, 1F, 0F, .5f);

        Gizmos.DrawWireCube(transform.position + new Vector3(inputSize.x - 1, inputSize.y - 1, inputSize.z - 1) / 2, new Vector3(inputSize.x, inputSize.y, inputSize.z));

        Gizmos.color = new Color(1F, 0F, 0F, .1f);

        if ( _nValue > 0 ){
            for ( int x = 0; x < inputSize.x / _nValue; x++ ){
                for ( int y = 0; y < inputSize.y / _nValue; y++ ){
                    for ( int z = 0; z < inputSize.z / _nValue; z++ ){
                        // Only optimized for N == 2 in current state.
                        Gizmos.DrawWireCube(transform.position + new Vector3(x * _nValue + .5F, y * _nValue + .5F, z * _nValue + .5F), new Vector3(_nValue, _nValue, _nValue));
                    }
                }
            }
        }
    }
}
