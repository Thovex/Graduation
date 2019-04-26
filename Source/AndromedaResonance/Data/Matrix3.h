// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "Module.h"
#include "Matrix3.generated.h"

/**
 *
 */

USTRUCT( BlueprintType )
struct FModuleMatrix {
	GENERATED_USTRUCT_BODY()

public:

	UPROPERTY( EditAnywhere, BlueprintReadWrite )
		int32 Width;

	UPROPERTY( EditAnywhere, BlueprintReadWrite )
		int32 Depth;

	UPROPERTY( EditAnywhere, BlueprintReadWrite )
		int32 Height;

	TArray < TArray < TArray<UModule> > > WidthArrays;
	TArray < TArray < UModule > > DepthArrays;
	TArray < UModule> HeightArrays;

	FModuleMatrix() {}

	FModuleMatrix( int32 Width, int32 Depth, int32 Height ) {
		this->Width = Width;
		this->Depth = Depth;
		this->Height = Height;

		for ( int32 X = 0; X < this->Width; X++ ) {

			TArray<TArray<UModule>> NewDepthArray;
			WidthArrays.Add( NewDepthArray );

			for ( int32 Y = 0; Y < this->Depth; Y++ ) {

				TArray<UModule> NewHeightArray;
				DepthArrays.Add( NewHeightArray );

				for ( int32 Z = 0; Z < this->Height; Z++ ) {

					//UModule NewModule = UModule( FIntVector( X, Y, Z ) );
					//HeightArrays.Add( NewModule );

				}
			}
		}
	}
};

UCLASS( Abstract )
class ANDROMEDARESONANCE_API UMatrix3 : public UObject {

	GENERATED_BODY()

public:
	UMatrix3();

};

