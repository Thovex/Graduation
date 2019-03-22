using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class ModulePrototype : SerializedMonoBehaviour {
    [SerializeField] private int _moduleIndex;
    [SerializeField] private Dictionary < EOrientations, TrainingNeighbourData> _moduleOrientations = new Dictionary < EOrientations, TrainingNeighbourData >();

    public Dictionary < EOrientations, TrainingNeighbourData > ModuleOrientations{
        get{ return _moduleOrientations; }
        set{ _moduleOrientations = value; }
    }

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

    private void OnDrawGizmosSelected(){

        Gizmos.color = Color.red;

        foreach ( KeyValuePair <EOrientations, TrainingNeighbourData> pair in ModuleOrientations ){
            Gizmos.DrawLine(transform.position, transform.position + Orientations.ReturnDirectionVal(pair.Key));
        }
    }
}
