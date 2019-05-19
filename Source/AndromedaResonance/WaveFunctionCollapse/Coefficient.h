// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Coefficient.generated.h"

USTRUCT( BlueprintType )
struct FCoefficient {
	GENERATED_USTRUCT_BODY()

public:
	FCoefficient() {}

	FCoefficient( int32 AllowedPattern)
	{
		TMap<int32, bool> NewAllowedPatterns;
		NewAllowedPatterns.Add(AllowedPattern, true);

		this->AllowedPatterns = NewAllowedPatterns;
	}

	FCoefficient( TMap<int32, bool> AllowedPatterns ) {
		this->AllowedPatterns = AllowedPatterns;
	}

	int32 AllowedCount() {
		int32 InAllowedCount = 0;

		for ( auto& Pair : AllowedPatterns ) {
			if ( Pair.Value ) {
				InAllowedCount++;
			}
		}

		return InAllowedCount;
	}

	int32 LastAllowedPatternIndex() {
		for ( auto& Pair : AllowedPatterns ) {
			if ( Pair.Value ) {
				return Pair.Key;
			}
		}

		return -1;
	}

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Coefficient" )
		TMap<int32, bool> AllowedPatterns;

};