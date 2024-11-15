// Fill out your copyright notice in the Description page of Project Settings.


#include "Module.h"
#include "EngineUtils.h"


AModule::AModule( const FObjectInitializer& ObjectInitializer ) {
	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = false;

}

