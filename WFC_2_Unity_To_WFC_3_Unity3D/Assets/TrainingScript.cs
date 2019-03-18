using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrainingScript : MonoBehaviour
{

    public void TrainNeighbours() {
        Debug.Log("test");
    }
}

[CustomEditor(typeof(TrainingScript))]
public class TrainingScriptInspector : Editor {

    public override void OnInspectorGUI() {
        TrainingScript training = (TrainingScript)target;
            
        if (GUILayout.Button("Training")) {
            training.TrainNeighbours();
        }
    }
}