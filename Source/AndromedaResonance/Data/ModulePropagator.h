// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "ModulePropagator.generated.h"

/**
 *
 */

USTRUCT( BlueprintType )
struct FModulePropagator {
	GENERATED_USTRUCT_BODY()

public:
	UPROPERTY( VisibleAnywhere, BlueprintReadOnly )
		TArray<int32> Array;

	FModulePropagator() {}

	FModulePropagator( TArray<int32> Array ) {
		this->Array = Array;
	}
};