// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Kismet/BlueprintFunctionLibrary.h"
#include "Data/ModuleData.h"
#include "Data/ModuleMatrix.h"
#include "WaveFunctionCollapse/Module.h"
#include "Runtime/CoreUObject/Public/UObject/NoExportTypes.h"
#include "Runtime/Engine/Classes/Engine/World.h"
#include "WaveFunctionLibrary.generated.h"

/**
 *
 */
UCLASS()
class ANDROMEDARESONANCE_API UWaveFunctionLibrary : public UBlueprintFunctionLibrary {
	GENERATED_BODY()

public:

	UFUNCTION( BlueprintCallable, Category = "Wave", meta = ( WorldContext = "WorldContextObject" ) )
		static UChildActorComponent * CreateModule( UObject * WorldContextObject, FModuleData ModuleData, AActor * ParentActor, FVector Location );

	UFUNCTION( BlueprintCallable, Category = "Wave", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray< UChildActorComponent*> CreatePatternIndex( UObject* WorldContextObject, AActor* ParentActor, AModuleAssignee* ModuleAssignee, int32 PatternIndex, FVector Location );

	UFUNCTION( BlueprintCallable, Category = "Wave", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray< UChildActorComponent*> CreatePatternData( UObject* WorldContextObject, AActor* ParentActor, AModuleAssignee* ModuleAssignee, FModuleMatrix Pattern, FVector Location );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "Wave", meta = ( WorldContext = "WorldContextObject" ) )
		static FModuleData CreateModuleDataFromBit( FName Bit, AModuleAssignee * ModuleAssignee );
};
