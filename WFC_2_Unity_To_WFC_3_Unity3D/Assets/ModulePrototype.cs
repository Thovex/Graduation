using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class ModulePrototype : SerializedMonoBehaviour {
    [SerializeField] private int _moduleIndex;
    [SerializeField] private List < EDirections > _directions = new List < EDirections >();

    void Update()
    {
        if ( SceneManager.GetActiveScene().name == "Modules" ){

            if ( transform.parent != null ){

                int index = 0;
                foreach ( Transform t in transform.parent.GetComponentsInChildren < Transform >() ){
                    if ( t == this.transform ){
                        _moduleIndex = index;
                    }

                    index++;
                }
            }
        }
    }

    private void OnDrawGizmos(){
        if ( SceneManager.GetActiveScene().name == "Modules" ){
            GUIStyle moduleStyle = new GUIStyle();
            moduleStyle.alignment = TextAnchor.MiddleCenter;
            moduleStyle.normal.textColor = Color.black;
            Handles.Label(transform.position + Vector3.up, "Module: " + _moduleIndex.ToString(), moduleStyle);

            Gizmos.color = Color.red;

            foreach ( EDirections direction in _directions ){
                Gizmos.DrawLine(transform.position, transform.position + Orientations.ReturnDirectionVal(direction));
            }
        }
    }
}
