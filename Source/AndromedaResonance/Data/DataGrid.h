// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "WaveFunctionCollapse/Module.h"
#include "Data/ModuleMatrix.h"
#include "DataGrid.generated.h"

UCLASS()
class ANDROMEDARESONANCE_API ADataGrid : public AActor {
	GENERATED_BODY()

public:
	ADataGrid( const FObjectInitializer& ObjectInitializer );

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Transform" )
		USceneComponent* Transform;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		FIntVector NSize = FIntVector( 2, 2, 2 );

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		int32 GridElementSize = 1000;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		float GridElementSpacing = 1.F;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		FColor PatternStatus;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		TMap<FIntVector, AModule*> ModulesMap;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Data" )
		FModuleMatrix ModuleData;

public:
	UFUNCTION( BlueprintCallable, Category = "Matrix Calls" )
		void SetMatrix( FIntVector SetMatrix );

	UFUNCTION( BlueprintCallable, Category = "Matrix Calls" )
		FModuleData GetModuleAt( FIntVector Coord );

	UFUNCTION( BlueprintCallable, Category = "Matrix Calls" )
		void SetModuleAt( FIntVector Coord, FModuleData Data );

	void ButtonPress();

protected:
	virtual void BeginPlay() override;
	virtual void Tick( float DeltaTime ) override;
	virtual bool ShouldTickIfViewportsOnly() const override;

private:


private:

	void MapChildren();
	void ConformToGrid();

	int32 RoundUp( int32 NumToRound, int32 Multiple );

	FIntVector IncrementCoord( FIntVector Coord );

};
