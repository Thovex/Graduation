// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "WaveFunctionCollapse/Module.h"
#include "Macros.h"
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

	void Initialize( TMap<FIntVector, FModuleData> InitializeMap ) {
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

			bool Equal = true;

			if ( ComparisonModule.Module != CheckModule.Module ) Equal = false;
			if ( ComparisonModule.RotationEuler != CheckModule.RotationEuler ) Equal = false;
			if ( ComparisonModule.Scale != CheckModule.Scale ) Equal = false;
			if ( ComparisonModule.Bit != CheckModule.Bit ) Equal = false;
			if ( ComparisonModule.Empty != CheckModule.Empty ) Equal = false;

			return Equal;

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

	void RotateCounterClockwise( int Times ) {
		int MinX = 0;
		int MaxX = SizeX - 1;

		int MinY = 0;
		int MaxY = SizeY - 1;

		for ( int32 i = 0; i < Times; i++ ) {
			for ( int32 Increment = 0; Increment < SizeX / 2; Increment++ ) {
				for ( int N = 0 + Increment; N < MaxX - Increment; N++ ) {

					TMap<FIntVector, FModuleData> OriginalData = Array3D;
					TMap<FIntVector, FModuleData> CopyData;

					for ( int32 X = 0; X < SizeX; X++ ) {
						for ( int32 Y = 0; Y < SizeY; Y++ ) {
							for ( int32 Z = 0; Z < SizeZ; Z++ ) {

								if ( X >= MinX + Increment && X <= ( MaxX - 1 ) - Increment && Y == MinY + Increment ) {
									CopyData.Add( FIntVector( X + 1, Y, Z ), OriginalData.FindRef( FIntVector( X, Y, Z ) ) );
								} else if ( X == MaxX - Increment && Y >= MinY + Increment && Y <= ( MaxY - 1 ) - Increment ) {
									CopyData.Add( FIntVector( X, Y + 1, Z ), OriginalData.FindRef( FIntVector( X, Y, Z ) ) );

								} else if ( X >= ( MinX + 1 ) + Increment && X <= MaxX - Increment && Y == MaxY - Increment ) {
									CopyData.Add( FIntVector( X - 1, Y, Z ), OriginalData.FindRef( FIntVector( X, Y, Z ) ) );

								} else if ( X == MinX + Increment && Y >= ( MinY + 1 ) + Increment && Y <= MaxY - Increment ) {
									CopyData.Add( FIntVector( X, Y - 1, Z ), OriginalData.FindRef( FIntVector( X, Y, Z ) ) );

								} else {
									CopyData.Add( FIntVector( X, Y, Z ), OriginalData.FindRef( FIntVector( X, Y, Z ) ) );
								}
							}
						}
					}

					Array3D = CopyData;
				}
			}
		}
	}
};