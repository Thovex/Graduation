// Fill out your copyright notice in the Description page of Project Settings.


#include "DataGrid.h"

ADataGrid::ADataGrid( const FObjectInitializer& ObjectInitializer )
{

	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT("Transform") );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ADataGrid::BeginPlay()
{
	Super::BeginPlay();

	SetActorTickEnabled( false );
	
}

// Called every frame
void ADataGrid::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

bool ADataGrid::ShouldTickIfViewportsOnly() const {
	return true;
}

