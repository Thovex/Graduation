using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrainingScript))]
public class TrainingScriptInspector : Editor {

    public override void OnInspectorGUI() {
        TrainingScript training = (TrainingScript)target;
            
        if (GUILayout.Button("Training")) {
            training.TrainNeighbours();
        }
    }
}

public class TrainingScript : MonoBehaviour
{
    
    //public List<Rules>

    public void TrainNeighbours() {
        Debug.Log("test");
    }
}
