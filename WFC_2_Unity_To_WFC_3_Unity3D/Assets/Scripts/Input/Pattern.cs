using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ViewDict{
    
    public ViewDict(Vector3Int coordinate, Module trainingData){
        _coordinate = coordinate;
        _trainingData = trainingData;
    }
    
    [SerializeField] private Vector3Int _coordinate;
    [SerializeField] private Module _trainingData;
}

[System.Serializable]
public class Pattern : Matrix<Module> {
    
    private int _patternSizeN;
    [SerializeField] private Vector3Int _patternCoordinate;

    [SerializeField] private List < ViewDict> _patternDictionary = new List<ViewDict>();

    public Pattern(int patternSizeN, Module[,,] patternData, Vector3Int patternCoordinate){        
        MatrixData = patternData;

        _patternSizeN = patternSizeN;
        _patternCoordinate = patternCoordinate;

        for ( int x = 0; x < MatrixData.GetLength(0); x++ ){
            for ( int y = 0; y < MatrixData.GetLength(1); y++ ){
                for ( int z = 0; z < MatrixData.GetLength(2); z++ ){
                    _patternDictionary.Add(new ViewDict(new Vector3Int(x,y,z), MatrixData[x,y,z]) );
                }
            }
        }
    }
    
    public override void RotatePatternCounterClockwise(int times)
    {
        base.RotatePatternCounterClockwise(times);
        
        for ( int i = 0; i < times; i++ ){
            for ( int x = 0; x < MatrixData.GetLength(0); x++ ){
                for ( int y = 0; y < MatrixData.GetLength(1); y++ ){
                    for ( int z = 0; z < MatrixData.GetLength(2); z++ ){
                        MatrixData[x, y, z].RotationEuler += new Vector3Int(0, -90, 0);
                    }
                }
            }
        }
    }

    public override bool IsEqualToMatrix(Matrix<Module> otherMatrix)
    {
        for (int i = 0; i < 3; i++)
        {
            if (MatrixData.GetLength(i) != otherMatrix.MatrixData.GetLength(i)) return false;
        }

        bool bIsEqual = true;
        
        for (int x = 0; x < MatrixData.GetLength(0); x++)
        {
            for (int y= 0; y < MatrixData.GetLength(1); y++)
            {
                for (int z = 0; z < MatrixData.GetLength(2); z++)
                {
                    Module original = MatrixData[x, y, z];
                    Module comparison = otherMatrix.MatrixData[x, y, z];

                    if (original.Prefab != comparison.Prefab)
                    {
                        bIsEqual = false;
                        break;
                    }

                    if (original.RotationEuler != comparison.RotationEuler)
                    {
                        bIsEqual = false;
                        break;
                    }

                }
            }
        }

        return bIsEqual;
    }
}
