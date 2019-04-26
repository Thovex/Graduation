// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Matrix3.h"
#include "DataGrid.generated.h"

UCLASS()
class ANDROMEDARESONANCE_API ADataGrid : public AActor {
	GENERATED_BODY()

public:
	ADataGrid();

	//UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Data" )
		//UMatrix3 MatrixTest;


	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		int32 GridElementSize = 1.F;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		float GridElementSpacing = 1.F;


protected:
	virtual void BeginPlay() override;
	virtual void Tick( float DeltaTime ) override;

	virtual bool ShouldTickIfViewportsOnly() const override;

};
