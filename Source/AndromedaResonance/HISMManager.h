// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Runtime/Engine/Classes/Components/HierarchicalInstancedStaticMeshComponent.h"
#include "WaveFunctionCollapse/Module.h"
#include "WaveFunctionCollapse/WFC.h"
#include "HISMManager.generated.h"

UCLASS()
class ANDROMEDARESONANCE_API AHISMManager : public AActor {
	GENERATED_BODY()

public:
	AHISMManager();

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "HISM - REQUIRED" )
		TArray<TSubclassOf<AActor>> Modules;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "HISM - REQUIRED" )
		AWFC* WFC;

	UPROPERTY( VisibleAnywhere, BlueprintReadWrite, Category = "HISM" )
		TArray<UHierarchicalInstancedStaticMeshComponent*> HISMs;

public:
	UFUNCTION( BlueprintCallable )
		void AppendMesh( FTransform Transform, UStaticMesh* Mesh, TArray<UMaterialInterface*> Materials );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "Flow Control" )
	static FORCEINLINE bool IsValid( UObject* Object ) {
		if ( Object ) {
			if ( Object->IsValidLowLevel() ) {
				return true;
			}
		}
		return false;
	}


protected:
	virtual void BeginPlay() override;
	virtual void Tick( float DeltaTime ) override;

};
