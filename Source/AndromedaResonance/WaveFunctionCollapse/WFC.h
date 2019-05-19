// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "EditorTickActor.h"
#include "Data/ModuleAssignee.h"
#include "Data/WaveMatrix.h"
#include "WaveFunctionCollapse/Coefficient.h"
#include "Runtime/Engine/Classes/Components/TextRenderComponent.h"
#include "WFC.generated.h"

USTRUCT(BlueprintType)
struct FPatternIndexArray
{

	GENERATED_USTRUCT_BODY()

public:
	FPatternIndexArray() { };

	UPROPERTY()
		TArray<int32> Array;
};

UCLASS()
class ANDROMEDARESONANCE_API AWFC : public AEditorTickActor {
	GENERATED_BODY()

public:
	AWFC(const FObjectInitializer& ObjectInitializer);

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Transform")
		USceneComponent* Transform;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "WFC")
		FIntVector OutputSize = FIntVector(5, 5, 5);

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "WFC")
		AModuleAssignee* ModuleAssignee;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "WFC")
		FWaveMatrix Wave;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "WFC" )
		bool bInitialized = false;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Spawned")
		TArray<UChildActorComponent*> SpawnedComponents;


public:

	UFUNCTION(BlueprintCallable, Category = "WFC")
		void Initialize();
	
	UFUNCTION( BlueprintCallable, Category = "WFC" )
		void StartWFC();

	UFUNCTION(BlueprintCallable, Category = "WFC" )
		void Observe( FIntVector ObserveValue, int32 Selected );

	UFUNCTION(BlueprintCallable, Category = "WFC" )
		void CreateFromJson(FWaveMatrix JsonWave );

	UFUNCTION( BlueprintCallable, Category = "WFC" )
		FIntVector MinEntropyCoords();


protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;

private:
	TArray<FIntVector> Flag;
	TArray<FIntVector> Updated;

	TMap<FIntVector, bool> HasModule;

private:
	void Propagate();
	void Constrain(FIntVector Coord);

	void SpawnMod(FIntVector Coord, int32 Selected);

	bool IsFullyCollapsed();

	int32 GetWeightedPattern(TMap<int32, bool> InPatterns);
	float ShannonEntropy(FIntVector CurrentCoordinates);

};
