using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Ya : MonoBehaviour
{
    [SerializeField] private Vector3Int m_InputSize;
    private int[,,] m_InputMatrix;
    private bool[,,] m_InputMatrixSet;


    void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.update += SnapToGrid;
        EditorApplication.update += CheckGridAvailability;
#endif

    }
    void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= SnapToGrid;
        EditorApplication.update -= CheckGridAvailability;
#endif

    }
    
    void Update()
    {
        SnapToGrid();
        CheckGridAvailability();
    }

    private void CheckGridAvailability()
    {
        m_InputMatrix = new int[m_InputSize.x, m_InputSize.y, m_InputSize.z];
        m_InputMatrixSet = new bool[m_InputSize.x, m_InputSize.y, m_InputSize.z];

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject childObject = transform.GetChild(i).gameObject;

            Vector3 childLocalPosition = childObject.transform.localPosition;

            m_InputMatrixSet[
                Mathf.RoundToInt(childLocalPosition.x),
                Mathf.RoundToInt(childLocalPosition.y),
                Mathf.RoundToInt(childLocalPosition.z)
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

    float Clamp90(float _value)
    {
        float[] angles = new float[] { 0, 90, 180, 270, 360 };

        for (int i = 0; i < angles.Length - 1; i++)
        {
            if (_value > angles[i] && _value < angles[i] + 45) _value = angles[i];
            if (_value >= angles[i] + 45 && _value < angles[i + 1]) _value = angles[i + 1];
        }
        return _value;

    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(.75f, .75f, .75f, .15f);
        for (int x = 0; x < m_InputSize.x; x++)
        {
            for (int y = 0; y < m_InputSize.y; y++)
            {
                for (int z = 0; z < m_InputSize.z; z++)
                {
                    if (m_InputMatrixSet[x, y, z])
                    {
                        Gizmos.color = new Color(1f, 0f, 0f, .35f);
                    }
                    else
                    {
                        Gizmos.color = new Color(0f, 1f, 0f, .35f);
                    }

                    Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), .25F);
                }
            }
        }

        Gizmos.color = new Color(1F, 1F, 0F, .5f);

        Gizmos.DrawWireCube(transform.position + new Vector3(m_InputSize.x - 1, m_InputSize.y - 1, m_InputSize.z - 1) / 2, new Vector3(m_InputSize.x, m_InputSize.y, m_InputSize.z));
    }
}
    