// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Kismet/BlueprintFunctionLibrary.h"
#include "UtilityLibrary.generated.h"

/**
 *
 */
UCLASS()
class ANDROMEDARESONANCE_API UUtilityLibrary : public UBlueprintFunctionLibrary {
	GENERATED_BODY()

public:

	UFUNCTION( BlueprintPure, meta = ( DisplayName = "ToIntVector (Vector)", CompactNodeTitle = "->", Keywords = "cast convert", BlueprintAutocast ), Category = "Math|Conversions" )
		static FIntVector Conv_VectorToIntVector( const FVector& InVector );

	UFUNCTION( BlueprintPure, meta = ( DisplayName = "ToIntVector (Rotator)", CompactNodeTitle = "->", Keywords = "cast convert", BlueprintAutocast ), Category = "Math|Conversions" )
		static FIntVector Conv_RotatorToIntVector( const FRotator& InRotator );

	UFUNCTION( BlueprintPure, meta = ( DisplayName = "ToIntVector (Vector)", CompactNodeTitle = "->", Keywords = "cast convert", BlueprintAutocast ), Category = "Math|Conversions" )
		static FVector Vector_AddIntVector( const FVector& InVector, const FIntVector& InIntVector );

	UFUNCTION( BlueprintCallable, Category = "Math", meta = ( WorldContext = "WorldContextObject" ) )
		static float SetScale( float CurrentValue, float OldMinScale, float OldMaxScale, float NewMinScale, float NewMaxScale );

	UFUNCTION( BlueprintCallable, BlueprintPure, meta = ( BlueprintThreadSafe ), Category = "Math", meta = ( WorldContext = "WorldContextObject" ) )
		static float SetScalePure( float CurrentValue, float OldMinScale, float OldMaxScale, float NewMinScale, float NewMaxScale );

};
