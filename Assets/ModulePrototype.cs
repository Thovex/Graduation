using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModulePrototype : MonoBehaviour{
    [SerializeField] private bool _isSymmetrical = false;

    public bool IsSymmetrical{
        get{ return _isSymmetrical; }
        set{ _isSymmetrical = value; }
    }
}
