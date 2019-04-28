// Fill out your copyright notice in the Description page of Project Settings.

#include "DataGrid.h"
#include "Runtime/Engine/Public/DrawDebugHelpers.h"
#include "Utility/UtilityLibrary.h"
#include "Data/Orientations.h"


ADataGrid::ADataGrid( const FObjectInitializer& ObjectInitializer ) {

	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;
}

void ADataGrid::SetMatrix( FIntVector Size ) {

	FModuleMatrix NewModuleMatrix = FModuleMatrix( Size );

	TMap<FIntVector, FModuleData> ModuleDataMap;

	for ( auto& Pair : ModulesMap ) {
		if ( Pair.Value ) {
			ModuleDataMap.Add( Pair.Key, Pair.Value->ToModuleData() );
		} else {
			ModuleDataMap.Add( Pair.Key, FModuleData( nullptr, FIntVector( 0, 0, 0 ), FIntVector( 1, 1, 1 ), FName( TEXT( "Null" ) ), true ) );
		}
	}

	NewModuleMatrix.Initialize( ModuleDataMap );

	ModuleData = NewModuleMatrix;
}

FModuleData ADataGrid::GetModuleAt( FIntVector Coord ) {
	return ModuleData.GetDataAt( Coord );
}

void ADataGrid::SetModuleAt( FIntVector Coord, FModuleData Data ) {
	ModuleData.SetDataAt( Coord, Data );
}

void ADataGrid::ButtonPress() {

}

void ADataGrid::BeginPlay() {
	Super::BeginPlay();

}

void ADataGrid::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );

	MapChildren();

	DrawDebugBox(
		GetWorld(),
		Transform->GetComponentLocation(),
		FVector( NSize * GridElementSize / 2 ),
		PatternStatus,
		false,
		-1.F,
		0,
		PatternStatus == FColor::Green ? 10.F : 100.F
	);
}

void ADataGrid::MapChildren() {
	TArray<AActor*> ChildActors;
	GetAttachedActors( ChildActors );

	ModulesMap.Empty( ChildActors.Num() );

	FIntVector Coord = FIntVector( 0, 0, 0 );

	for ( int32 i = 0; i < ( NSize.X * NSize.Y * NSize.Z ); i++ ) {

		if ( i < ChildActors.Num() ) {
			ModulesMap.Add( Coord, Cast<AModule>( ChildActors[i] ) );
		} else {
			ModulesMap.Add( Coord, nullptr );
		}

		if ( i < NSize.X * NSize.Y * NSize.Z - 1 ) {
			Coord = IncrementCoord( Coord );
		}
	}

	// Retarded sorting goes here

	ModulesMap.KeySort( [] ( FIntVector A, FIntVector B ) {
		if ( A.X < B.X ) return true;
		if ( A.Y < B.Y ) return true;
		if ( A.Z < B.Z ) return true;
		return false;
	} );

	ConformToGrid();

}

void ADataGrid::ConformToGrid() {

	TMap<FIntVector, int32> CheckMap;

	bool IsValid = true;

	for ( auto& Pair : ModulesMap ) {
		if ( Pair.Value ) {
			FVector RelativeValueLocation = Pair.Value->GetActorLocation() - GetActorLocation();

			FVector GridLocation = FVector(
				FMath::RoundToInt( FMath::GridSnap( RelativeValueLocation.X, 500.F ) ),
				FMath::RoundToInt( FMath::GridSnap( RelativeValueLocation.Y, 500.F ) ),
				FMath::RoundToInt( FMath::GridSnap( RelativeValueLocation.Z, 500.F ) )
			);

			FRotator WorldValueRotation = Pair.Value->GetActorRotation();

			WorldValueRotation.Roll = FMath::RoundToInt( WorldValueRotation.Roll / 90 ) * 90;
			WorldValueRotation.Pitch = FMath::RoundToInt( WorldValueRotation.Pitch / 90 ) * 90;
			WorldValueRotation.Yaw = FMath::RoundToInt( WorldValueRotation.Yaw / 90 ) * 90;

			int32 * Count = CheckMap.Find( FIntVector( GridLocation ) );

			if ( Count != nullptr ) {
				CheckMap.Add( FIntVector( GridLocation ), *Count + 1 );
				IsValid = false;
			} else {
				CheckMap.Add( FIntVector( GridLocation ), 0 );
			}

			Pair.Value->SetActorLocation( GetActorLocation() + GridLocation );
			Pair.Value->SetActorRotation( WorldValueRotation );
		}
	}


	PatternStatus = IsValid ? FColor::Green : FColor::Red;

	if ( IsValid ) {
		if ( IsSelectedInEditor() ) {
			SetMatrix( NSize );
			return;
		}
		for ( auto& Pair : ModulesMap ) {
			if ( Pair.Value ) {
				if ( Pair.Value->IsSelectedInEditor() ) {
					SetMatrix( NSize );
					return;
				}
			}
		}
	}
}

FIntVector ADataGrid::IncrementCoord( FIntVector Coord ) {

	FIntVector UpdatedCoord = Coord;

	if ( UpdatedCoord.X < NSize.X - 1 ) {
		UpdatedCoord.X++;
		return UpdatedCoord;
	} else {
		UpdatedCoord.X = 0;
	}

	if ( UpdatedCoord.Y < NSize.Y - 1 ) {
		UpdatedCoord.Y++;
		return UpdatedCoord;
	} else {
		UpdatedCoord.Y = 0;
	}

	if ( UpdatedCoord.Z < NSize.Z - 1 ) {
		UpdatedCoord.Z++;
		return UpdatedCoord;
	} else {
		UpdatedCoord.Z = 0;
	}

	UE_LOG( LogTemp, Error, TEXT( "DataGrid <%s> has too many values and will overwrite!" ), *GetActorLabel() );

	return UpdatedCoord;
}


