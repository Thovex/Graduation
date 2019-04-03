﻿using System;
using System.Collections.Generic;
using UnityEngine;
using static Thovex.Utility;

[Serializable]
public class Matrix<T>
{
    public Matrix(){}
	public Matrix(Vector3Int matrixSize){
		MatrixData = new T[matrixSize.x, matrixSize.y, matrixSize.z];

		SizeX = matrixSize.x;
		SizeY = matrixSize.y;
		SizeZ = matrixSize.z;
	}

	public T[,,] MatrixData{ get;  set; }

	public int SizeX{ get; protected set; }
	public int SizeY{ get; protected set; }
	public int SizeZ{ get; protected set; }

    public T GetDataAt(Vector3Int coordinate )
    {
        return MatrixData[coordinate.x, coordinate.y, coordinate.z];
    }

    public bool Contains(T check)
    {
        bool contains = false;

        For3(this, (x, y, z) =>
        {
            if (Equals(MatrixData[x, y, z], check)) {
                contains = true;
            }
        });

        return contains;
    }

	public virtual void RotatePatternCounterClockwise(int times){
		for ( int i = 0; i < times; i++ ){	
			
			// Todo: Replace with Quaternion rotation
			for ( int n = 0; n < SizeX - 1; n++ ){

				T[,,] originalData = MatrixData;
				T[,,] copyMatrix = new T[originalData.GetLength(0), originalData.GetLength(1), originalData.GetLength(2)];

				For3(this, (x, y, z) => {
					if ( x < SizeX - 1 && z == 0 ){
						copyMatrix[x + 1, y, z] = originalData[x, y, z];
					}
					else if ( x == SizeX - 1 && z < SizeZ - 1 ){
						copyMatrix[x, y, z + 1] = originalData[x, y, z];
					}
					else if ( z == SizeZ - 1 && x > 0 ){
						copyMatrix[x - 1, y, z] = originalData[x, y, z];
					}
					else if ( z > 0 && x == 0 ){
						copyMatrix[x, y, z - 1] = originalData[x, y, z];
					}
					else{
						copyMatrix[x, y, z] = originalData[x, y, z];
					}
				});

				MatrixData = copyMatrix;
			}
		}
	}

	public void RotatePatternClockwise(){
		throw new NotImplementedException();
	}


	public virtual bool IsEqualToMatrix(Matrix<T> otherMatrix)
	{
		throw new NotImplementedException();
	}

    public bool Equals(T data, T equal)
    {
        if (typeof(T) == typeof(T))
        {
            Vector3Int dataVector3Int = (Vector3Int)Convert.ChangeType(data, typeof(Vector3Int));
            Vector3Int equalVector3Int = (Vector3Int)Convert.ChangeType(equal, typeof(Vector3Int));

            if (dataVector3Int == equalVector3Int) return true;
        }
        else
        {
            Debug.LogError("Cannot check Equals T == Your type because it is not implemented specifically!");
            return false;
        }

        return false;
    }
}