// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "Matrix3.generated.h"

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
struct FArrayZ {
	GENERATED_USTRUCT_BODY()

public:
	FArrayZ() {}
	FArrayZ( TArray<struct FModule> ZArray ) {
		this->ZArray = ZArray;

	}

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Data" )
		TArray<struct FModule> ZArray;

};

USTRUCT( BlueprintType )
struct FArrayY {
	GENERATED_USTRUCT_BODY()

public:
	FArrayY() {}
	FArrayY( TArray<struct FArrayZ> YArray ) {
		this->YArray = YArray;

	}

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Data" )
		TArray<struct FArrayZ> YArray;

};

USTRUCT( BlueprintType )
struct FArrayX {
	GENERATED_USTRUCT_BODY()

public:
	FArrayX() {}
	FArrayX( TArray<struct FArrayY> XArray ) {
		this->XArray = XArray;
	}

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Data" )
		TArray<struct FArrayY> XArray;

};


USTRUCT( BlueprintType )
struct FModuleMatrix {
	GENERATED_USTRUCT_BODY()

public:

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Settings" )
		int32 Width;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Settings" )
		int32 Depth;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Settings" )
		int32 Height;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Data" )
		FArrayX Array3D;

	FModuleMatrix() {

	}

	explicit FModuleMatrix( int32 Width, int32 Depth, int32 Height, FArrayX Array3D) :
		Width( Width ), Depth(Depth), Height(Height), Array3D(Array3D) {

	}

	FModuleMatrix( int32 Width, int32 Depth, int32 Height ) {


		this->Width = Width;
		this->Depth = Depth;
		this->Height = Height;

		FArrayX ArrayXInit = FArrayX();
		FArrayY ArrayYInit = FArrayY();
		FArrayZ ArrayZInit = FArrayZ();

		for ( int X = 0; X < this->Width; X++ ) {
			for ( int Y = 0; Y < this->Depth; Y++ ) {
				for ( int Z = 0; Z < this->Height; Z++ ) {
					ArrayZInit.ZArray.Add( FModule( FIntVector( X, Y, Z ) ) );
				}
				ArrayYInit.YArray.Add( ArrayZInit );
			}
			ArrayXInit.XArray.Add( ArrayYInit );
		}
		this->Array3D = ArrayXInit;
	}
};

UCLASS( Abstract )
class ANDROMEDARESONANCE_API UMatrix3 : public UObject {

	GENERATED_BODY()

public:
	UMatrix3();

};

