using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Module
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private EOrientations rotationDir;
    [SerializeField] private Vector3Int rotationEuler;
    [SerializeField] private Dictionary<EOrientations, Coefficient> allowedNeighbours;


    public GameObject Prefab {
        get => prefab;
        set => prefab = value;
    }

    public EOrientations RotationDir {
        get => rotationDir;
        set => rotationDir = value;
    }

    public Dictionary<EOrientations, Coefficient> ModuleNeighbours {
        get => allowedNeighbours;
        set => allowedNeighbours = value;
    }

    public Vector3Int RotationEuler {
        get => rotationEuler;
        set {
            rotationEuler = value;
            RotationDir = Orientations.EulerToOrientation(value);
        }
    }

    public Module(GameObject prefab, Vector3Int rotationEuler)
    {
        this.prefab = prefab;
        this.rotationEuler = rotationEuler;
        this.rotationDir = Orientations.EulerToOrientation(rotationEuler);
        this.allowedNeighbours = new Dictionary<EOrientations, Coefficient>();
    }

    public string GenerateID(TrainingScript training)
    {
        if (!Prefab) return "null";

        return training.PrefabToId(this.Prefab).ToString();
    }

    private string GenerateRotation(TrainingScript training)
    {
        if (!Prefab) return "null";

        ModulePrototype modulePrototype = this.Prefab.GetComponent<ModulePrototype>();

        string rotString = "";

        if (modulePrototype)
        {
            if (modulePrototype.IsSymmetrical)
            {
                rotString += "S";
            }
            else
            {
                rotString += this.RotationDir.ToString()[0];
            }
        }
        else
        {
            rotString += this.RotationDir.ToString()[0];
        }

        return rotString;
    }

    public string GenerateBit(TrainingScript training)
    {
        if (!Prefab) return "null";
        return GenerateID(training) + GenerateRotation(training);
    }
}