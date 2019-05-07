// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Coefficient.generated.h"

USTRUCT( BlueprintType )
struct FCoefficient {
	GENERATED_USTRUCT_BODY()

public:
	FCoefficient() {}

	FCoefficient( TMap<int32, bool> AllowedPatterns ) {
		this->AllowedPatterns = AllowedPatterns;
	}

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Coefficient")
		TMap<int32, bool> AllowedPatterns;

};