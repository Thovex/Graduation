// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "Matrix3.generated.h"

DECLARE_LOG_CATEGORY_EXTERN( LogModuleMatrix, Log, All );

/**
 *
 */

USTRUCT( BlueprintType )
struct FModule {
	GENERATED_USTRUCT_BODY()

public:
	FModule() {}

	FModule( FIntVector Coordinate ) {
		this->Coordinate = Coordinate;
	}

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Data" )
		FIntVector Coordinate;

};

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
		TMap<FIntVector, FModule> Array3D;

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

	void Initialize() {
		if ( SizeX < 1 && SizeY < 1 && SizeZ < 1 ) {
			UE_LOG( LogModuleMatrix, Error, TEXT( "Initializing FModuleMatrix with < (0, 0, 0) size." ) );
		}

		for ( int X = 0; X < this->SizeX; X++ ) {
			for ( int Y = 0; Y < this->SizeY; Y++ ) {
				for ( int Z = 0; Z < this->SizeZ; Z++ ) {
					Array3D.Add( FIntVector( X, Y, Z ), FModule( FIntVector( X, Y, Z ) ) );
				}
			}
		}

		UE_LOG( LogModuleMatrix, Log, TEXT( "Initialization of ModuleMatrix Succesful. Size: (X: %d, Y: %d, Z: %d)" ), SizeX, SizeY, SizeZ );

	}

	FModule GetDataAt( FIntVector Coord ) {
		if ( IsValidCoordinate( Coord ) ) {
			return Array3D.FindRef( Coord );
		}

		UE_LOG( LogModuleMatrix, Error, TEXT( "Invalid Data at GetDataAt( %s )" ), *Coord.ToString() );
		return FModule();
	}

	void SetDataAt( FIntVector Coord, FModule Data ) {
		Array3D.Add( Coord, Data );
	}

	bool Contains( FModule CheckModule ) {
		for ( auto Pair : Array3D ) {
			FModule ComparisonModule = Pair.Value;

			//TODO: Keep Contains up to date with Module.

			if ( ComparisonModule.Coordinate == CheckModule.Coordinate ) {
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

UCLASS( Abstract )
class ANDROMEDARESONANCE_API UMatrix3 : public UObject {

	GENERATED_BODY()

public:
	UMatrix3();

};

