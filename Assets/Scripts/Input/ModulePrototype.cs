using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ModulePrototype : SerializedMonoBehaviour {
    [SerializeField] private bool isSymmetrical = false;
    private Dictionary<EOrientations, Coefficient> coefficientDict = new Dictionary<EOrientations, Coefficient>();
    [SerializeField] private Dictionary<EOrientations, string> coefficientsDisplay = new Dictionary<EOrientations, string>();

    public string bit;
    public Dictionary<EOrientations, Coefficient> CoefficientDict { get => coefficientDict; set => coefficientDict = value; }
    public bool IsSymmetrical { get => isSymmetrical; set => isSymmetrical = value; }

    public void CalculateDisplay()
    {
        coefficientsDisplay.Clear();

        foreach (KeyValuePair<EOrientations, Coefficient> pair in coefficientDict)
        {
            coefficientsDisplay.Add(pair.Key, pair.Value.Print());
        }
    }
}
