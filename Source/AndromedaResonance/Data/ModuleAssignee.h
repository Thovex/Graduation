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

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Enabled" )
		bool bEnabled = false;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Assignee" )
		TMap<TSubclassOf<AModule>, FName> AssignedNames;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Patterns" )
		TMap<int32, FModuleMatrix > Patterns;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Patterns" )
		TMap<int32, int32 > Weights;

protected:
	virtual void BeginPlay() override;
	virtual void Tick( float DeltaTime ) override;

	void Training();


};
