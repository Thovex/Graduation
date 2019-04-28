// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "WaveFunctionCollapse/Module.h"
#include "ModuleAssignee.generated.h"

UCLASS()
class ANDROMEDARESONANCE_API AModuleAssignee : public AActor
{
	GENERATED_BODY()
	
public:	
	AModuleAssignee( const FObjectInitializer& ObjectInitializer );

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Transform" )
		USceneComponent* Transform;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Assignee" )
		TMap<TSubclassOf<AModule>, FName> AssignedNames;

protected:
	virtual void BeginPlay() override;
	virtual void Tick( float DeltaTime ) override;
	virtual bool ShouldTickIfViewportsOnly() const override;


};
