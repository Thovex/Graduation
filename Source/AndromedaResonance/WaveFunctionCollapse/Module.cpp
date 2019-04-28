// Fill out your copyright notice in the Description page of Project Settings.


#include "Module.h"
#include "Data/Orientations.h"
#include "Utility/UtilityLibrary.h"
#include "Data/ModuleAssignee.h"
#include "EngineUtils.h"

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
	NewModuleData.Scale = GetFIntVectorScale();
	NewModuleData.Bit = GenerateBit();

	NewModuleData.Empty = false;

	ModuleData = NewModuleData;

	return NewModuleData;
}

FIntVector AModule::GetFIntVectorEuler() {
	return FIntVector( UUtilityLibrary::Conv_RotatorToIntVector( this->GetActorRotation() ) );
}

FIntVector AModule::GetFIntVectorScale() {
	return FIntVector( UUtilityLibrary::Conv_VectorToIntVector( this->GetActorScale3D() ) );
}

FName AModule::GetFNameFromScaleVector() {
	FIntVector Scale = GetFIntVectorScale();

	if ( Scale == FIntVector( 1, 1, 1 ) ) return FName( TEXT( "N" ) );
	if ( Scale == FIntVector( -1, 1, 1 ) ) return FName( TEXT( "X" ) );
	if ( Scale == FIntVector( 1, -1, 1 ) ) return FName( TEXT( "Y" ) );

	return FName( TEXT( "Invalid Scale" ) );
}

FName AModule::GenerateBit() {

	if ( !ModuleAssignee ) return FName( TEXT( "No Module Assignee" ) );

	FString Bit = ModuleAssignee->AssignedNames.FindRef( this->GetClass() ).ToString();

	if ( Symmetrical ) {
		Bit.Append( "S" );
	} else {
		EOrientations Orientation = UOrientations::EulerToOrientation( GetFIntVectorEuler() );

		const FName* EulerFName = UOrientations::OrientationByFName.FindKey( Orientation );

		if ( EulerFName != nullptr ) {
			Bit.Append( EulerFName->ToString() );
		}
	}

	FIntVector Scale = GetFIntVectorScale();

	Bit.Append( GetFNameFromScaleVector().ToString() );

	return FName( *Bit );
}

void AModule::BeginPlay() {
	Super::BeginPlay();
}

void AModule::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );
}

