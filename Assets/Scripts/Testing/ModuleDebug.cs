using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleDebug : MonoBehaviour{
    [SerializeField] private bool bEmpty;
    private void OnDrawGizmos(){
        Gizmos.color = bEmpty ? new Color(1, 0, 0, 0.1F) : new Color(0, 0, 1, 0.1F);
        Gizmos.DrawWireCube(this.transform.position, Vector3.one / 4);
    }
}
