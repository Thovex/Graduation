// Fill out your copyright notice in the Description page of Project Settings.


#include "ModuleAssignee.h"
#include "Data/DataGrid.h"
#include "EngineUtils.h"
#include "Engine/Engine.h"


AModuleAssignee::AModuleAssignee( const FObjectInitializer& ObjectInitializer ) {
	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;
}

void AModuleAssignee::BeginPlay() {
	Super::BeginPlay();

	SetActorTickEnabled( false );

	bEnabled = true;

	Training();

}

void AModuleAssignee::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );

	Training();

}

void AModuleAssignee::Training() {
	AssignedNames.Empty();
	Patterns.Empty();
	Weights.Empty();

	if ( !bEnabled ) return;


	UWorld* World = GetWorld();

	if ( World ) {
		for ( TActorIterator<AModule> ActorItr( World ); ActorItr; ++ActorItr ) {
			AModule* Module = *ActorItr;

			TSubclassOf<AModule> SubClass = Module->GetClass();
			if ( !AssignedNames.Find( SubClass ) ) {
				AssignedNames.Add( SubClass, FName( *FString::FromInt( AssignedNames.Num() ) ) );
			}

			if ( !Module->ModuleAssignee ) {
				Module->ModuleAssignee = this;
			}
		}

		for ( TActorIterator<ADataGrid> ActorItr( World ); ActorItr; ++ActorItr ) {
			ADataGrid* DataGrid = *ActorItr;

			if ( DataGrid ) {
				DataGrid->ModuleAssignee = this;
				DataGrid->Training(true);
			}
		}

		for ( TActorIterator<ADataGrid> ActorItr( World ); ActorItr; ++ActorItr ) {
			ADataGrid* DataGrid = *ActorItr;

			FModuleMatrix& CopyModuleData = DataGrid->ModuleData;

			for ( int32 i = 0; i < 4; i++ ) {
				FModuleMatrix RotationCopy = CopyModuleData;
				RotationCopy.RotateCounterClockwise( i );
				Patterns.Add( Patterns.Num(), RotationCopy );

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
			Weights.Add( Pattern.Key, 1 );
			Pattern.Value.BuildPropagator( Patterns );
		}

		if ( GEngine ) GEngine->AddOnScreenDebugMessage( -1, 2.0f, FColor::Cyan, TEXT( "Updated to Latest Data!" ) );
	}
}


