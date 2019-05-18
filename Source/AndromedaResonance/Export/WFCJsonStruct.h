// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "WaveFunctionCollapse/Module.h"
#include "WFCJsonStruct.generated.h"


USTRUCT( BlueprintType )
struct FWFCJsonModuleCoordPattern {
	GENERATED_USTRUCT_BODY()

public:
	FWFCJsonModuleCoordPattern() {}

	FWFCJsonModuleCoordPattern( FIntVector Coord, int32 Value )
	{
		this->Coord = Coord;
		this->Value = Value;
	}

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		FIntVector Coord;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		int32 Value;
};

USTRUCT( BlueprintType )
struct FWFCJsonStruct {
	GENERATED_USTRUCT_BODY()

public:
	FWFCJsonStruct() {}

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		TArray<FWFCJsonModuleCoordPattern> Data;
};