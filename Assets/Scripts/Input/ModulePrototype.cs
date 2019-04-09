using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ModulePrototype : SerializedMonoBehaviour {
    [SerializeField] private bool _isSymmetrical = false;
    private Dictionary<EOrientations, Coefficient> _coefficientDict = new Dictionary<EOrientations, Coefficient>();
    [SerializeField] private Dictionary<EOrientations, string> _coefficientsDisplay = new Dictionary<EOrientations, string>();

    public string bit;
    public Dictionary<EOrientations, Coefficient> CoefficientDict { get => _coefficientDict; set => _coefficientDict = value; }
    public bool IsSymmetrical { get => _isSymmetrical; set => _isSymmetrical = value; }

    public void CalculateDisplay()
    {
        _coefficientsDisplay.Clear();

        foreach (KeyValuePair<EOrientations, Coefficient> pair in _coefficientDict)
        {
            _coefficientsDisplay.Add(pair.Key, pair.Value.Print());
        }
    }
}
