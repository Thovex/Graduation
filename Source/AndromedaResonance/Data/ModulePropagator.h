// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "ModulePropagator.generated.h"

/**
 *
 */

struct FModuleMatrix;

USTRUCT(BlueprintType)
struct FModulePropagator {
	GENERATED_USTRUCT_BODY()

public:
	FModulePropagator() { }

	FModulePropagator(TArray<FModuleMatrix*> Array)
	{
		this->Array = Array;
	}

	TArray<FModuleMatrix*> Array;

};