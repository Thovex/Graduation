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
	
	// assuming N == 2 initially.
	public void RotatePatternCounterClockwise(){
		T[,,] originalData = MatrixData;

		for ( int y = 0; y < MatrixData.GetLength(1); y++ ){
			MatrixData[0, y, 0] = originalData[0, y, 1];
			MatrixData[1, y, 0] = originalData[0, y, 0];
			MatrixData[1, y, 1] = originalData[1, y, 0];
			MatrixData[0, y, 1] = originalData[1, y, 1];
		}
	}

	public void RotatePatternLeft(){
		throw new NotImplementedException();
	}

}
