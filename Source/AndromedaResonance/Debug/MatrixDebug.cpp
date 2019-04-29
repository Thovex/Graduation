// Fill out your copyright notice in the Description page of Project Settings.


#include "MatrixDebug.h"

AMatrixDebug::AMatrixDebug() {

	const UWorld* World = GetWorld();

	if ( World ) {
		if ( DataGrid ) {
			CopyModuleMatrix( DataGrid->ModuleData );
		}
	}
}

void AMatrixDebug::BeginPlay() {
	Super::BeginPlay();

}

void AMatrixDebug::PostEditChangeProperty( FPropertyChangedEvent& PropertyChangedEvent ) {
	Super::PostEditChangeProperty( PropertyChangedEvent );

	bIsSet = false;
}

void AMatrixDebug::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );

	const UWorld* World = GetWorld();

	if ( World ) {
		if ( !bIsSet ) {
			if ( DataGrid ) {
				CopyModuleMatrix( DataGrid->ModuleData );
			}
		} else {
			for ( auto& Pair : this->ModuleMatrix.Array3D ) {
				UTextRenderComponent* TextRenderer = MatrixTestRenderers.FindRef( Pair.Key );

				if ( TextRenderer ) {
					TextRenderer->SetText( FText::FromName( Pair.Value.Bit ) );
				}
			}
		}
	}
}

void AMatrixDebug::ButtonPress() {
	this->ModuleMatrix.RotateCounterClockwise( 1 );
}

void AMatrixDebug::CopyModuleMatrix( FModuleMatrix ModuleMatrixToCopy ) {
	this->ModuleMatrix = FModuleMatrix();

	this->ModuleMatrix.SizeX = ModuleMatrixToCopy.SizeX;
	this->ModuleMatrix.SizeY = ModuleMatrixToCopy.SizeY;
	this->ModuleMatrix.SizeZ = ModuleMatrixToCopy.SizeZ;

	this->ModuleMatrix.Array3D = ModuleMatrixToCopy.Array3D;

	bIsSet = true;
}
