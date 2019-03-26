using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.Serialization;

[System.Serializable]
public class Matrix<T> {
	protected Matrix(){}

	public Matrix(Vector3Int matrixSize){
		MatrixData = new T[matrixSize.x, matrixSize.y, matrixSize.z];
	}

	public T[,,] MatrixData{ get; protected set; }
	
	//public void RotatePatternCounterClockwise(){
		//T[,,] originalData = MatrixData;

		//for ( int y = 0; y < MatrixData.GetLength(1); y++ ){
		//	MatrixData[0, y, 0] = originalData[0, y, 1];
		//	MatrixData[1, y, 0] = originalData[0, y, 0];
		//	MatrixData[1, y, 1] = originalData[1, y, 0];
		//	MatrixData[0, y, 1] = originalData[1, y, 1];
		//}
	//}
	
	public void RotatePatternCounterClockwise(){
		T[,,] originalData = MatrixData;
		T[,,] copyMatrix = new T[originalData.GetLength(0), originalData.GetLength(1), originalData.GetLength(2)];

		int xN = MatrixData.GetLength(0);
		int yN = MatrixData.GetLength(1);
		int zN = MatrixData.GetLength(2);

		for (int x = 0; x < xN; x++)
		{
			for (int y = 0; y < yN; y++)
			{
				for (int z = 0; z < zN; z++)
				{
					if (x < xN - 1 && z == 0)
					{
						copyMatrix[x + 1, y, z] = originalData[x, y, z];
					} else if (x == xN - 1 && z < zN - 1) 
					{
						copyMatrix[x, y, z + 1] = originalData[x, y, z];
					} else if (z == zN - 1 && x > 0) 
					{
						copyMatrix[x - 1, y, z] = originalData[x, y, z];
					} else if (z > 0 && x == 0)
					{
						copyMatrix[x, y, z - 1] = originalData[x, y, z];
					} else
					{
						copyMatrix[x, y, z] = originalData[x, y, z];
					}
				}
			}
		}

		MatrixData = copyMatrix;
	}
	
	public void RotatePatternClockwise(){
		throw new NotImplementedException();
	}


	public virtual bool IsEqualToMatrix(Matrix<T> otherMatrix)
	{
		throw new NotImplementedException();
	}

}
