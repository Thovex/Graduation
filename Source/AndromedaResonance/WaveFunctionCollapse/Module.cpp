// Fill out your copyright notice in the Description page of Project Settings.


#include "Module.h"

DEFINE_LOG_CATEGORY( LogModule );


AModule::AModule( const FObjectInitializer& ObjectInitializer )
{
	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;

}

FModuleData AModule::ToModuleData() {
	FModuleData NewModuleData = FModuleData();

	NewModuleData.Module = this->GetClass();
	NewModuleData.Empty = false;

	return NewModuleData;
}

void AModule::BeginPlay()
{
	Super::BeginPlay();
	
}

void AModule::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

