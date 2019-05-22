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

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Module Data" )
		AModuleAssignee* ModuleAssignee;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "IMPORTANT Module Data " )
		int32 ConstructId = -1;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "IMPORTANT Module Data" )
		bool Symmetrical = false;

};
