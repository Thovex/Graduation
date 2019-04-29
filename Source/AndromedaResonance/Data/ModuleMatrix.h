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

	// CHECKED
	bool Contains( FModuleData CheckModule, FModuleData& ContainedModule ) {
		for ( auto Pair : Array3D ) {
			if ( Pair.Value == CheckModule ) {
				ContainedModule = Pair.Value;
				return true;
			}
		}
		return false;
	}

	// SEMI CHECKED
	bool IsValidCoordinate( FIntVector Coord ) {
		if ( Coord.X < 0 ) return false;
		if ( Coord.X >= SizeX ) return false;

		if ( Coord.Y < 0 ) return false;
		if ( Coord.Y >= SizeY ) return false;

		if ( Coord.Z < 0 ) return false;
		if ( Coord.Z >= SizeZ ) return false;

		return true;

	}

	// UNCHECKED
	void Clear() {
		Array3D.Empty( SizeX * SizeY * SizeZ );
	}

	// DOUBLE CHECKED (INT TIMES 1)
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

					for3( SizeX, SizeY, SizeZ,

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
						  )

						Array3D = CopyData;
				}
			}
		}
	}

	// DOUBLE CHECKED
	void PushData( FIntVector Direction ) {
		TMap<FIntVector, FModuleData> OriginalData = Array3D;
		TMap<FIntVector, FModuleData> CopyData;

		for3( SizeX, SizeY, SizeZ,

			  FIntVector SweepCoord = FIntVector( X + -Direction.X, Y + -Direction.Y, Z + -Direction.Z );

		if ( IsValidCoordinate( SweepCoord ) ) {
			CopyData.Add( FIntVector( X, Y, Z ), OriginalData.FindRef( SweepCoord ) );
		} else {
			CopyData.Add( FIntVector( X, Y, Z ), FModuleData( true ) );
		}
		)

			Array3D = CopyData;

	}
};