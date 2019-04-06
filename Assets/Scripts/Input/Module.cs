using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Module
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private EOrientations rotationDir;
    [SerializeField] private Vector3Int rotationEuler;
    [SerializeField] private List<OrientationModule> moduleNeighbours;


    public GameObject Prefab {
        get => prefab;
        set => prefab = value;
    }

    public EOrientations RotationDir {
        get => rotationDir;
        set => rotationDir = value;
    }

    public List<OrientationModule> ModuleNeighbours {
        get => moduleNeighbours;
        set => moduleNeighbours = value;
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
        this.moduleNeighbours = new List<OrientationModule>();
    }

    public string GenerateBit(TrainingScript training)
    {

        if (!Prefab) return "null";

        ModulePrototype modulePrototype = this.Prefab.GetComponent<ModulePrototype>();

        string bitString = training.PrefabToId(this.Prefab).ToString();

        if (modulePrototype)
        {
            if (modulePrototype.IsSymmetrical)
            {
                bitString += "S";
            }
            else
            {
                bitString += this.RotationDir.ToString()[0];
            }
        }
        else
        {
            bitString += this.RotationDir.ToString()[0];
        }


        return bitString;
    }
}