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

		int32 DataGridCount = 0;
		for ( TActorIterator<ADataGrid> ActorItr( World ); ActorItr; ++ActorItr ) {
			ADataGrid* DataGrid = *ActorItr;

			if ( DataGrid ) {
				DataGrid->ModuleAssignee = this;
				DataGrid->Training( true );

				DataGridCount++;
			}
		}

		TMap<int32, ADataGrid*> DataGridSorted;

		for ( TActorIterator<ADataGrid> ActorItr( World ); ActorItr; ++ActorItr ) {
			ADataGrid* DataGrid = *ActorItr;

			if ( DataGrid ) {
				int32 ID = FCString::Atoi( *DataGrid->GetActorLabel());
				DataGridSorted.Add(ID, DataGrid);
			}
		}

		DataGridSorted.KeySort( [] ( int32 A, int32 B ) {
			return A < B; 
		} );

		for (auto& Pair : DataGridSorted)
		{
			FModuleMatrix& CopyModuleData = Pair.Value->ModuleData;

			for ( int32 i = 0; i < 4; i++ ) {
				FModuleMatrix RotationCopy = CopyModuleData;
				RotationCopy.RotateCounterClockwise( i );

				Patterns.Add( Patterns.Num(), RotationCopy );
				Weights.Add( Patterns.Num() - 1, Pair.Value->Weight );
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


