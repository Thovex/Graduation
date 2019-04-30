// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "EditorTickActor.h"
#include "WaveFunctionCollapse/Module.h"
#include "Data/ModuleMatrix.h"
#include "ModuleAssignee.generated.h"


UCLASS()
class ANDROMEDARESONANCE_API AModuleAssignee : public AEditorTickActor {
	GENERATED_BODY()

public:
	AModuleAssignee( const FObjectInitializer& ObjectInitializer );

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Transform" )
		USceneComponent* Transform;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Assignee" )
		TMap<TSubclassOf<AModule>, FName> AssignedNames;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Patterns" )
		TMap<int32, FModuleMatrix > Patterns;

protected:
	virtual void BeginPlay() override;
	virtual void Tick( float DeltaTime ) override;


};
