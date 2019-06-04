// Fill out your copyright notice in the Description page of Project Settings.


#include "ModuleAssignee.h"
#include "Data/DataGrid.h"
#include "EngineUtils.h"
#include "Engine/Engine.h"
#include "DataInput.h"


AModuleAssignee::AModuleAssignee( const FObjectInitializer& ObjectInitializer ) {
	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;
}

void AModuleAssignee::BeginPlay() {
	Super::BeginPlay();

	SetActorTickEnabled( false );

	Training();

}

void AModuleAssignee::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );
}

void AModuleAssignee::Training() {

	AssignedNames.Empty();
	Patterns.Empty();
	Weights.Empty();

	UWorld* World = GetWorld();

	if ( World ) {
		for ( TActorIterator<AModule> ActorItr( World ); ActorItr; ++ActorItr ) {
			AModule* Module = *ActorItr;

			TSubclassOf<AModule> SubClass = Module->GetClass();
			if ( !AssignedNames.Find( SubClass ) ) {
				AssignedNames.Add( SubClass, FName( *FString::FromInt( Module->ConstructId ) ) );
			}

			if ( !Module->ModuleAssignee ) {
				Module->ModuleAssignee = this;
			}
		}

		for ( TActorIterator<ADataInput> ActorItr( World ); ActorItr; ++ActorItr ) {
			ADataInput* DataInput = *ActorItr;

			if ( DataInput ) {
				DataInput->Training();

				for ( auto& Pattern : DataInput->Patterns ) {
					Weights.Add(Patterns.Num(), 100);
					Patterns.Add( Patterns.Num(), Pattern );
				}

				DataInput->Patterns.Empty();
			}
		}

		TArray<int32> SimilarityIndices;

		for ( int32 i = 0; i < Patterns.Num(); i++ ) {
			for ( int32 j = 0; j < Patterns.Num(); j++ ) {
				if ( i != j && !SimilarityIndices.Contains( i ) && !SimilarityIndices.Contains( j ) ) {
					if ( Patterns[i] == Patterns[j] ) {
						SimilarityIndices.Add( j );
					}
				}
			}
		}

		for ( int32 Similar : SimilarityIndices ) {
			Patterns.Remove( Similar );
		}

		for ( auto& Pattern : Patterns ) {
			Pattern.Value.BuildPropagator( Patterns );
		}
	}
}


