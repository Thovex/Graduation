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

	UFUNCTION( BlueprintCallable, Category = "WFC" )
		FIntVector MinEntropyCoords();


protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;

protected:

	/*Calculates prime numbers in the game thread*/
	UFUNCTION( BlueprintCallable, Category = MultiThreading )
		void CalculatePrimeNumbers();

	/*Calculates prime numbers in a background thread*/
	UFUNCTION( BlueprintCallable, Category = MultiThreading )
		void CalculatePrimeNumbersAsync();

	/*The max prime number*/
	UPROPERTY( EditAnywhere, Category = MultiThreading )
		int32 MaxPrime;

private:
	TArray<FIntVector> Flag;
	TArray<FIntVector> Updated;

private:
	void Propagate();
	void Constrain(FIntVector Coord);

	void SpawnMod(FIntVector Coord, int32 Selected);

	bool IsFullyCollapsed();

	int32 GetWeightedPattern(TMap<int32, bool> InPatterns);

	float ShannonEntropy(FIntVector CurrentCoordinates);

};


namespace ThreadingTest {
	static void CalculatePrimeNumbers( int32 UpperLimit ) {
		//Calculating the prime numbers...
		for ( int32 i = 1; i <= UpperLimit; i++ ) {
			bool isPrime = true;

			for ( int32 j = 2; j <= i / 2; j++ ) {
				if ( FMath::Fmod( i, j ) == 0 ) {
					isPrime = false;
					break;
				}
			}

			if ( isPrime ) GLog->Log( "Prime number #" + FString::FromInt( i ) + ": " + FString::FromInt( i ) );
		}
	}
}

/*PrimeCalculateAsyncTask is the name of our task
FNonAbandonableTask is the name of the class I've located from the source code of the engine*/
class PrimeCalculationAsyncTask : public FNonAbandonableTask {
	int32 MaxPrime;

public:
	/*Default constructor*/
	PrimeCalculationAsyncTask( int32 MaxPrime ) {
		this->MaxPrime = MaxPrime;
	}

	/*This function is needed from the API of the engine.
	My guess is that it provides necessary information
	about the thread that we occupy and the progress of our task*/
	FORCEINLINE TStatId GetStatId() const {
		RETURN_QUICK_DECLARE_CYCLE_STAT( PrimeCalculationAsyncTask, STATGROUP_ThreadPoolAsyncTasks );
	}

	/*This function is executed when we tell our task to execute*/
	void DoWork() {
		ThreadingTest::CalculatePrimeNumbers( MaxPrime );

		GLog->Log( "--------------------------------------------------------------------" );
		GLog->Log( "End of prime numbers calculation on background thread" );
		GLog->Log( "--------------------------------------------------------------------" );
	}
};