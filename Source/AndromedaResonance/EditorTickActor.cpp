// Fill out your copyright notice in the Description page of Project Settings.


#include "EditorTickActor.h"

AEditorTickActor::AEditorTickActor()
{
	PrimaryActorTick.bCanEverTick = true;

}

void AEditorTickActor::BeginPlay()
{
	Super::BeginPlay();

	SetActorTickEnabled( false );
	
}

void AEditorTickActor::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

bool AEditorTickActor::ShouldTickIfViewportsOnly() const {
	return true;
}
