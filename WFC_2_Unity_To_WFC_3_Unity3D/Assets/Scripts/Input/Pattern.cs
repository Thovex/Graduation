using System;
using System.Collections;
using System.Collections.Generic;
using static Thovex.Utility;
using UnityEngine;

// used for testing purposes
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
    [SerializeField] private readonly List < ViewDict> _patternDictionary = new List<ViewDict>();

    public Pattern(int patternSizeN, Module[,,] patternData, Vector3Int patternCoordinate){
        MatrixData = patternData;

        patternSizeN = patternSizeN;
        patternCoordinate = patternCoordinate;

        SizeX = MatrixData.GetLength(0);
        SizeY = MatrixData.GetLength(1);
        SizeZ = MatrixData.GetLength(2);

        Nested3(this, (x, y, z) => {
            _patternDictionary.Add(new ViewDict(new Vector3Int(x, y, z), MatrixData[x, y, z]));
        });
    }

    public override void RotatePatternCounterClockwise(int times)
    {
        base.RotatePatternCounterClockwise(times);
        
        for ( int i = 0; i < times; i++ ){
            Nested3(this, (x, y, z) => {
                MatrixData[x, y, z].RotationEuler += new Vector3Int(0, -90, 0);
            });
        }
    }

    public override bool IsEqualToMatrix(Matrix<Module> otherMatrix)
    {
        for (int i = 0; i < 3; i++)
        {
            if (MatrixData.GetLength(i) != otherMatrix.MatrixData.GetLength(i)) return false;
        }

        bool bIsEqual = true;
        
        Nested3(this, (x, y, z) => {
            Module original = MatrixData[x, y, z];
            Module comparison = otherMatrix.MatrixData[x, y, z];

            if (original.Prefab != comparison.Prefab) bIsEqual = false;
            if ( original.RotationEuler != comparison.RotationEuler ) bIsEqual = false;
        });
        
        return bIsEqual;
    }

    public Matrix<string> GenerateBits(TrainingScript training){

        Matrix < string > bits = new Matrix < string >(new Vector3Int(this.SizeX, this.SizeY, this.SizeZ));

        Nested3(bits, (x, y, z) => {
            bits.MatrixData[x, y, z] = this.MatrixData[x,y,z].GenerateBit(training);
        });

        return bits;
    }
}
