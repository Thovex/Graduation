// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "WaveFunctionCollapse/Module.h"
#include "ModuleMatrix.generated.h"

DECLARE_LOG_CATEGORY_EXTERN( LogModuleMatrix, Log, All );

/**
 *
 */

USTRUCT( BlueprintType )
struct FModuleMatrix {
	GENERATED_USTRUCT_BODY()

public:

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Settings" )
		int32 SizeX;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Settings" )
		int32 SizeY;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Settings" )
		int32 SizeZ;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Data" )
		TMap<FIntVector, FModuleData> Array3D;

	FModuleMatrix() {

	}

	FModuleMatrix( FIntVector Size ) {
		SetSize( Size.X, Size.Y, Size.Z );
	}

	FModuleMatrix( int32 X, int32 Y, int32 Z ) {
		SetSize( X, Y, Z );
	}

	void SetSize( int32 X, int32 Y, int32 Z ) {
		this->SizeX = X;
		this->SizeY = Y;
		this->SizeZ = Z;
	}

	void Initialize( TMap<FIntVector, FModuleData> InitializeMap) {
		if ( SizeX < 1 && SizeY < 1 && SizeZ < 1 ) {
			UE_LOG( LogModuleMatrix, Error, TEXT( "Initializing FModuleMatrix with < (0, 0, 0) size." ) );
		}

		Array3D = InitializeMap;

	}

	FModuleData GetDataAt( FIntVector Coord ) {
		if ( IsValidCoordinate( Coord ) ) {
			return Array3D.FindRef( Coord );
		}

		UE_LOG( LogModuleMatrix, Error, TEXT( "Invalid Data at GetDataAt( %s )" ), *Coord.ToString() );
		return FModuleData();
	}

	void SetDataAt( FIntVector Coord, FModuleData Data ) {
		Array3D.Add( Coord, Data );
	}

	bool Contains( FModuleData CheckModule ) {
		for ( auto Pair : Array3D ) {
			FModuleData ComparisonModule = Pair.Value;

			//TODO: Keep Contains up to date with Module.

			if ( ComparisonModule.Module == CheckModule.Module ) {
				return true;
			}
		}

		return false;
	}

	bool IsValidCoordinate( FIntVector Coord ) {
		if ( !( Coord.X > 0 && Coord.Y > 0 && Coord.Z > 0 ) ) return false;
		if ( !( Coord.X < SizeX && Coord.Y < SizeY && Coord.Z < SizeZ ) ) return false;
		return true;
	}

	void Clear() {
		Array3D.Empty( SizeX * SizeY * SizeZ );
	}
};