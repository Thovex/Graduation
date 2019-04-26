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

	/** Convert an IntVector to a vector */
	UFUNCTION( BlueprintPure, meta = ( DisplayName = "ToIntVector (Vector)", CompactNodeTitle = "->", Keywords = "cast convert", BlueprintAutocast ), Category = "Math|Conversions" )
		static FIntVector Conv_VectorToIntVector( const FVector& InVector );

	UFUNCTION( BlueprintCallable, Category = "Math", meta = ( WorldContext = "WorldContextObject" ) )
		static float SetScale( float CurrentValue, float OldMinScale, float OldMaxScale, float NewMinScale, float NewMaxScale );

	UFUNCTION( BlueprintCallable, BlueprintPure, meta = ( BlueprintThreadSafe ), Category = "Math", meta = ( WorldContext = "WorldContextObject" ) )
		static float SetScalePure( float CurrentValue, float OldMinScale, float OldMaxScale, float NewMinScale, float NewMaxScale );

};
