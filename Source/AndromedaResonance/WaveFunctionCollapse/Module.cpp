// Fill out your copyright notice in the Description page of Project Settings.


#include "Module.h"
#include "Data/Orientations.h"
#include "Utility/UtilityLibrary.h"

DEFINE_LOG_CATEGORY( LogModule );

AModule::AModule( const FObjectInitializer& ObjectInitializer ) {
	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;
}

FModuleData AModule::ToModuleData() {
	FModuleData NewModuleData = FModuleData();

	NewModuleData.Module = this->GetClass();

	NewModuleData.RotationEuler = GetFIntVectorEuler();
	NewModuleData.Scale = FIntVector( GetActorScale3D() );
	NewModuleData.Bit = GenerateBit();
	NewModuleData.Empty = false;

	return NewModuleData;
}

FIntVector AModule::GetFIntVectorEuler() {
	return FIntVector( UUtilityLibrary::Conv_RotatorToIntVector( this->GetActorRotation() ) );
}

FName AModule::GenerateBit() {

	FString Bit = GetClass()->GetFName().ToString();

	EOrientations Orientation = UOrientations::EulerToOrientation( GetFIntVectorEuler() );

	const FName* EulerFName = UOrientations::OrientationByFName.FindKey( Orientation );

	if ( EulerFName != nullptr ) {
		Bit.Append( EulerFName->ToString() );
	}

	return FName( *Bit );
}

void AModule::BeginPlay() {
	Super::BeginPlay();
}

void AModule::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );
}

