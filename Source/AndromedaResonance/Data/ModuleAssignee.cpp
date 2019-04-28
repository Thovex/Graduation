// Fill out your copyright notice in the Description page of Project Settings.


#include "ModuleAssignee.h"
#include "EngineUtils.h"


AModuleAssignee::AModuleAssignee( const FObjectInitializer& ObjectInitializer ) {
	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;
}

void AModuleAssignee::BeginPlay() {
	Super::BeginPlay();

}

void AModuleAssignee::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );

	AssignedNames.Empty();

	for ( TActorIterator<AModule> ActorItr( GetWorld() ); ActorItr; ++ActorItr ) {
		AModule* Module = *ActorItr;
		if ( Module ) {
			TSubclassOf<AModule> SubClass = Module->GetClass();

			if ( !AssignedNames.Find( SubClass ) ) {
				AssignedNames.Add( SubClass, FName( *FString::FromInt( AssignedNames.Num() ) ) );

			}

			if ( !Module->ModuleAssignee ) {
				Module->ModuleAssignee = this;
			}
		}
	}
}


