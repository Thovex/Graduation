// Fill out your copyright notice in the Description page of Project Settings.


#include "HISMManager.h"

AHISMManager::AHISMManager() {
	PrimaryActorTick.bCanEverTick = true;

}

void AHISMManager::AppendMesh( FTransform Transform, UStaticMesh* Mesh, TArray<UMaterialInterface*> Materials ) {
	if ( !Mesh ) return;

	for ( auto& HISM : HISMs ) {
		if ( HISM->GetStaticMesh() == Mesh ) {
			HISM->AddInstance( Transform );
			return;
		}
	}

	UHierarchicalInstancedStaticMeshComponent* NewHISM = NewObject<UHierarchicalInstancedStaticMeshComponent>( this, UHierarchicalInstancedStaticMeshComponent::StaticClass() );
	NewHISM->RegisterComponent();

	NewHISM->SetStaticMesh( Mesh );


	for ( int32 index = 0; index < Materials.Num(); index++) {
		NewHISM->SetMaterial(index, Materials[index]);
	}
	NewHISM->AddInstance( Transform );

	UE_LOG( LogTemp, Warning, TEXT( "added new boi" ) );
	HISMs.Add( NewHISM );
}

void AHISMManager::BeginPlay() {
	Super::BeginPlay();

}

void AHISMManager::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );

}

