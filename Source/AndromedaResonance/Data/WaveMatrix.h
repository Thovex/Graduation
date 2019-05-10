// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Macros.h"
#include "WaveFunctionCollapse/Coefficient.h"
#include "WaveMatrix.generated.h"

/**
 *
 */

DECLARE_LOG_CATEGORY_EXTERN( LogWaveMatrix, Log, All );

USTRUCT( BlueprintType )
struct FWaveMatrix {
	GENERATED_USTRUCT_BODY()

public:

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Settings" )
		int32 SizeX;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Settings" )
		int32 SizeY;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Settings" )
		int32 SizeZ;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Data" )
		TMap<FIntVector, FCoefficient> Array3D;

	FWaveMatrix() {}

	FWaveMatrix( const FIntVector Size ) {
		SetSize( Size.X, Size.Y, Size.Z );
	}

	FWaveMatrix( const int32 X, const int32 Y, const int32 Z ) {
		SetSize( X, Y, Z );
	}

	void SetSize( const int32 X, const int32 Y, const int32 Z ) {
		this->SizeX = X;
		this->SizeY = Y;
		this->SizeZ = Z;
	}

	// SEMI CHECKED
	bool IsValidCoordinate( const FIntVector Coord ) {
		if ( Coord.X < 0 ) return false;
		if ( Coord.X >= SizeX ) return false;

		if ( Coord.Y < 0 ) return false;
		if ( Coord.Y >= SizeY ) return false;

		if ( Coord.Z < 0 ) return false;
		if ( Coord.Z >= SizeZ ) return false;

		return true;

	}

	FCoefficient GetDataAt( const FIntVector Coord ) {
		if ( IsValidCoordinate( Coord ) ) {
			return Array3D.FindRef( Coord );
		}

		UE_LOG( LogWaveMatrix, Error, TEXT( "Invalid Data at GetDataAt( %s )" ), *Coord.ToString() );
		return FCoefficient();
	}

	void AddData( const FIntVector Coord, const FCoefficient Data ) {
		Array3D.Add( Coord, Data );
	}

};