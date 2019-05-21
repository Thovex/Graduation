// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Module.generated.h"

class AModuleAssignee;

UCLASS( BlueprintType )
class ANDROMEDARESONANCE_API AModule : public AActor {
	GENERATED_BODY()

public:
	AModule( const FObjectInitializer& ObjectInitializer );

public:

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Transform" )
		USceneComponent* Transform;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		bool Symmetrical = false;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Module Data" )
		AModuleAssignee* ModuleAssignee;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Module Data" )
		bool bConstructionObject = false;

};
