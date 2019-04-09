using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;
using static Thovex.Utility;

[CustomEditor(typeof(MatrixTest))]
public class MatrixTestInspector : OdinEditor
{

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        MatrixTest matrixTest = (MatrixTest)target;
            
        if (GUILayout.Button("Clean Matrix")){
            matrixTest.CleanMatrix();
        }
        
        if (GUILayout.Button("Fill Matrix")){
            matrixTest.FillMatrix();
        }
        
        if (GUILayout.Button("Rotate 90 counter-clockwise Matrix")){
            matrixTest.Rotate(false);
        }
        
        if (GUILayout.Button("Rotate 90 clockwise Matrix")){
            matrixTest.Rotate(true);
        }
    }
}

public class MatrixTest : SerializedMonoBehaviour
{
    private Matrix3<Color> vectorMatrix = new Matrix3<Color>(new Vector3Int(0, 0, 0));
    [SerializeField] private int N = 2;

    public void CleanMatrix()
    {
        vectorMatrix = new Matrix3<Color>(new Vector3Int(0, 0, 0));
    }
    
    public void FillMatrix()
    {
        vectorMatrix = new Matrix3<Color>(new Vector3Int(N, N, N));

        For3(vectorMatrix, (x, y, z) =>
        {
            if (x == 0 && y == Mathf.RoundToInt(N / 2) && z == 0)
            {
                vectorMatrix.MatrixData[x, y, z] = new Color(1.0F, 0.2F, 0.2F, 1.0F);
            }
            else
            {
                vectorMatrix.MatrixData[x, y, z] = new Color(0.1F + y * 0.1F, 0.1F + y * 0.1F, 0.1F + y * 0.1F, 0.1F + y * 0.1F);
            }
        });
  
    }

    public void Rotate(bool bClockwise)
    {
        if (bClockwise)
        {
            vectorMatrix.RotatePatternClockwise();
        } 
        else 
        {
            vectorMatrix.RotateCounterClockwise(1);
        }
    }

    private void OnDrawGizmos()
    {
        For3(vectorMatrix, (x, y, z) =>
        {
            Gizmos.color = vectorMatrix.MatrixData[x, y, z];
            Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), 0.25F);
        });
    }
}
