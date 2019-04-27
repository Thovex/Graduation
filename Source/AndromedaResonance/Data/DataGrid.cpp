// Fill out your copyright notice in the Description page of Project Settings.


#include "DataGrid.h"

ADataGrid::ADataGrid( const FObjectInitializer& ObjectInitializer ) {

	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;


}

void ADataGrid::SetMatrix( FIntVector Coord ) {

	FModuleMatrix NewModuleMatrix = FModuleMatrix( Coord );
	NewModuleMatrix.Initialize();

	ModuleData = NewModuleMatrix;
}

FModule ADataGrid::GetModuleAt( FIntVector Coord ) {
	return ModuleData.GetDataAt( Coord );
}

void ADataGrid::SetModuleAt( FIntVector Coord, FModule Data ) {
	ModuleData.SetDataAt( Coord, Data );
}

// Called when the game starts or when spawned
void ADataGrid::BeginPlay() {
	Super::BeginPlay();

	SetActorTickEnabled( false );

}

// Called every frame
void ADataGrid::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );

}

bool ADataGrid::ShouldTickIfViewportsOnly() const {
	return true;
}

