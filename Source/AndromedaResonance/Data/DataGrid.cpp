// Fill out your copyright notice in the Description page of Project Settings.

#include "DataGrid.h"


ADataGrid::ADataGrid( const FObjectInitializer& ObjectInitializer ) {

	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;
}

void ADataGrid::SetMatrix( FIntVector Coord ) {

	FModuleMatrix NewModuleMatrix = FModuleMatrix( Coord );

	TMap<FIntVector, FModuleData> ModuleDataMap;

	for ( auto& Pair : ModulesMap ) {
		if ( Pair.Value ) {
			ModuleDataMap.Add( Pair.Key, Pair.Value->ToModuleData() );
		} else {
			ModuleDataMap.Add( Pair.Key, FModuleData( nullptr, true ) );
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
	SetMatrix( NSize );
}

void ADataGrid::BeginPlay() {
	Super::BeginPlay();

	SetActorTickEnabled( false );
}

void ADataGrid::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );

	MapChildren();
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

	ModulesMap.KeySort( [] ( FIntVector A, FIntVector B ) {
		if ( A.X < B.X ) return true;
		if ( A.Y < B.Y ) return true;
		if ( A.Z < B.Z ) return true;
		return false;
	} );

	ConformToGrid();
}

void ADataGrid::ConformToGrid() {

	FVector CurrentLocation = GetActorLocation();

	for ( auto& Pair : ModulesMap ) {

		if ( Pair.Value ) {
			FVector LocalLocation = CurrentLocation;

			LocalLocation.X += ( Pair.Key.X * GridElementSize ) - ( GridElementSize / NSize.X );
			LocalLocation.Y += ( Pair.Key.Y * GridElementSize ) - ( GridElementSize / NSize.Y );
			LocalLocation.Z += ( Pair.Key.Z * GridElementSize ) - ( GridElementSize / NSize.Z );

			Pair.Value->SetActorLocation( LocalLocation );
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

bool ADataGrid::ShouldTickIfViewportsOnly() const {
	return true;
}


