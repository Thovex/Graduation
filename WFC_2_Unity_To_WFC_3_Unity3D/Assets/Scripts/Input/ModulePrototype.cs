using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModulePrototype : SerializedMonoBehaviour {
    [SerializeField] private Dictionary < EOrientations, TrainingNeighbourData> _moduleOrientations = new Dictionary < EOrientations, TrainingNeighbourData >();

    public Dictionary < EOrientations, TrainingNeighbourData > ModuleOrientations{
        get{ return _moduleOrientations; }
        set{ _moduleOrientations = value; }
    }

    private void OnDrawGizmosSelected(){

    }

    private void DrawForwardRightUpVectors(){
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
    }
}
