// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Data/ModuleAssignee.h"
#include "Data/WaveMatrix.h"
#include "Export/WFCJsonStruct.h"
#include "Runtime/JsonUtilities/Public/JsonObjectConverter.h"
#include "WaveFunctionCollapse/Coefficient.h"
#include "JsonExporter.generated.h"

UCLASS()
class ANDROMEDARESONANCE_API AJsonExporter : public AActor {
	GENERATED_BODY()

public:
	AJsonExporter();

protected:
	UFUNCTION( BlueprintCallable )
		void ParseData( FWaveMatrix Wave, AModuleAssignee* ModuleAssignee );

	UFUNCTION( BlueprintCallable )
		FWaveMatrix RetrieveDataFileName( FString FileName );

	UFUNCTION( BlueprintCallable )
		FWaveMatrix RetrieveDataRandom(FIntVector Size, FString& OutFileName );

};
