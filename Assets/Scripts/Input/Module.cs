using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Module
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private EOrientations rotationDir;
    [SerializeField] private Vector3Int rotationEuler;
    [SerializeField] private Vector3Int scale;
    [SerializeField] private Dictionary<EOrientations, Coefficient> allowedNeighbours;


    public GameObject Prefab {
        get => prefab;
        set => prefab = value;
    }

    public Vector3Int Scale {
        get => scale;
        set => scale = value;
    }

    public EOrientations RotationDir {
        get => rotationDir;
        private set => rotationDir = value;
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

    public Module(GameObject prefab, Vector3Int rotationEuler, Vector3Int scale)
    {
        this.prefab = prefab;
        this.rotationEuler = rotationEuler;
        this.scale = scale;
        this.rotationDir = Orientations.EulerToOrientation(rotationEuler);
        this.allowedNeighbours = new Dictionary<EOrientations, Coefficient>();
    }

    public string GenerateID(TrainingScript training)
    {
        if (!Prefab) return "ERROR?";

        return training.PrefabToId(this.Prefab).ToString();
    }

    public string GenerateRotation()
    {
        if (!Prefab) return "ERROR?";

        ModulePrototype modulePrototype = this.Prefab.GetComponent<ModulePrototype>();

        if (modulePrototype)
        {
            if (modulePrototype.IsSymmetrical)
            {
                return "S";
            }

        }

        return RotationDir.ToString()[0].ToString();

    }

    public string GenerateScale()
    {
        if (!Prefab) return "ERROR?";

        ModulePrototype modulePrototype = this.Prefab.GetComponent<ModulePrototype>();

        if (modulePrototype)
        {
            if (modulePrototype.IsSymmetrical)
            {
                return "N";
            }
        }


        if (scale == new Vector3Int(1, 1, 1)) return "N";
        if (scale == new Vector3Int(-1, 1, 1)) return "X";
        if (scale == new Vector3Int(1, 1, -1)) return "Z";

        return "ERROR?";
    }

    public string GenerateBit(TrainingScript training)
    {
        if (!Prefab) return "null";
        return GenerateID(training) + GenerateRotation() + GenerateScale();
    }
}