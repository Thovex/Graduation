// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "WaveFunctionCollapse/Module.h"
#include "WFCJsonStruct.generated.h"


USTRUCT( BlueprintType )
struct FWFCJsonModuleData
{
	GENERATED_USTRUCT_BODY()

public:
	FWFCJsonModuleData(): Symmetrical(false), Empty(false)
	{
	}

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		FString TSubClassOfAModule;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		FName ModuleID;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		FIntVector RotationEuler;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		FIntVector Scale;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		bool Symmetrical;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		bool Empty;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		FName Bit;
};


USTRUCT( BlueprintType )
struct FWFCJsonModuleCoordPattern {
	GENERATED_USTRUCT_BODY()

public:
	FWFCJsonModuleCoordPattern(): Index(0), Value(0)
	{
	}

	FWFCJsonModuleCoordPattern( FIntVector Coord, int32 Value, int32 Index )
	{
		this->Coord = Coord;
		this->Value = Value;
		this->Index = Index;
	}

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		int32 Index;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		FIntVector Coord;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		int32 Value;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly)
		FWFCJsonModuleData ModuleData;
};

USTRUCT( BlueprintType )
struct FWFCJsonStruct {
	GENERATED_USTRUCT_BODY()

public:
	FWFCJsonStruct() {}

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		TArray<FWFCJsonModuleCoordPattern> Data;
};