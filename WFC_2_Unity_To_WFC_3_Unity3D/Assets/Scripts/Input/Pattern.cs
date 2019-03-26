using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ViewDict{
    
    public ViewDict(Vector3Int coordinate, TrainingData trainingData){
        _coordinate = coordinate;
        _trainingData = trainingData;
    }
    
    [SerializeField] private Vector3Int _coordinate;
    [SerializeField] private TrainingData _trainingData;
}

[System.Serializable]
public class Pattern : Matrix<TrainingData> {
    
    private int _patternSizeN;
    [SerializeField] private Vector3Int _patternCoordinate;

    [SerializeField] private List < ViewDict> _patternDictionary = new List<ViewDict>();

    public Pattern(int patternSizeN, TrainingData[,,] patternData, Vector3Int patternCoordinate){        
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

    public override bool IsEqualToMatrix(Matrix<TrainingData> otherMatrix)
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
                    TrainingData original = MatrixData[x, y, z];
                    TrainingData comparison = otherMatrix.MatrixData[x, y, z];

                    if (original.GameObjectIndex != comparison.GameObjectIndex)
                    {
                        bIsEqual = false;
                        break;
                    }

                    if (original.LocalRotation != comparison.LocalRotation)
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
