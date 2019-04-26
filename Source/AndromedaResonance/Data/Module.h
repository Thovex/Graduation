// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "Module.generated.h"

/**
 *
 */
UCLASS( BlueprintType )
class ANDROMEDARESONANCE_API UModule : public UObject {
	GENERATED_BODY()

public:
	UModule();
	UModule( FIntVector Coordinate );

public:
	FIntVector ModuleCoordinate;

};
