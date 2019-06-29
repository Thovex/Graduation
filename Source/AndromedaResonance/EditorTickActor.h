// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "EditorTickActor.generated.h"

UCLASS()
class ANDROMEDARESONANCE_API AEditorTickActor : public AActor
{
	GENERATED_BODY()
	
public:	
	AEditorTickActor();

protected:
	virtual void BeginPlay() override;
	virtual void Tick( float DeltaTime ) override;

	virtual bool ShouldTickIfViewportsOnly() const override;

};
