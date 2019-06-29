// Fill out your copyright notice in the Description page of Project Settings.


#include "MatrixDebug.h"
#include "Utility/WaveFunctionLibrary.h"
#include "Data/Orientations.h"

AMatrixDebug::AMatrixDebug( const FObjectInitializer& ObjectInitializer ) {

	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	const UWorld* World = GetWorld();

	if ( World ) {
		if ( ModuleAssignee ) {
			CopyModuleMatrix( ModuleAssignee->Patterns.FindRef( PatternIndex ) );
		}
	}

	Clean();
}

void AMatrixDebug::BeginPlay() {
	Super::BeginPlay();

}

void AMatrixDebug::Clean() {
	for ( auto& Spawned : DisplayModules ) {
		if ( Spawned ) {
			if ( Spawned->IsValidLowLevel() ) {

				TArray<USceneComponent*> ChildComponents;
				Spawned->GetChildrenComponents( true, ChildComponents );

				for ( USceneComponent* ChildComponent : ChildComponents ) {
					ChildComponent->DestroyComponent();
				}

				Spawned->DestroyComponent();
			}
		}
	}

	DisplayModules.Empty();
}

void AMatrixDebug::PostEditChangeProperty( FPropertyChangedEvent& PropertyChangedEvent ) {
	Super::PostEditChangeProperty( PropertyChangedEvent );

	bIsSet = false;

	Clean();

	if ( ModuleAssignee ) {
		CopyModuleMatrix( ModuleAssignee->Patterns.FindRef( PatternIndex ) );
	}
}

void AMatrixDebug::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );

	const UWorld* World = GetWorld();

	if ( World ) {
		if ( !bIsSet ) {
			if ( ModuleAssignee ) {
				CopyModuleMatrix( ModuleAssignee->Patterns.FindRef( PatternIndex ) );
			}
		}
	}
}

void AMatrixDebug::ButtonPress( FName Bit ) {
	//this->ModuleMatrix.RotateCounterClockwise( 1 );
	//this->ModuleMatrix.PushData( UOrientations::OrientationUnitVectors.FindRef( EOrientations::BACK_LEFT_UP ) );

// 	if ( DataGrid ) {
// 		FModuleData Contains;
// 		this->ModuleMatrix.Contains( DataGrid->ModuleData.Array3D.FindRef( FIntVector( 0, 0, 0 ) ), Contains );
// 
// 	}


// 	if (DataGrid) {
// 
// 
// 		if (DataGrid->ModuleAssignee) {
// 
// 			//this->ModuleMatrix.Flip( EOrientations::UP );
// 
// 			this->ModuleMatrix.BuildPropagator(DataGrid->ModuleAssignee->Patterns);
// 
// 		}
// 	}

	// 	if ( DataGrid ) {
	// 		if ( DataGrid->ModuleAssignee ) {
	// 			TMap<TSubclassOf<AModule>, FName> Map = DataGrid->ModuleAssignee->AssignedNames;
	// 
	// 			if ( Bit != TEXT( "" ) ) {
	// 				FModuleData NewData = FModuleData( Bit, Map, true);
	// 			}
	// 		}
	// 	}

}

void AMatrixDebug::CopyModuleMatrix( FModuleMatrix ModuleMatrixToCopy ) {
	this->ModuleMatrix = ModuleMatrixToCopy;

	bIsSet = true;
}
