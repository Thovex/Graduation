// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "EditorTickActor.h"
#include "WaveFunctionCollapse/Module.h"
#include "Data/ModuleMatrix.h"
#include "DataInput.generated.h"

class AModuleAssignee;

UCLASS()
class ANDROMEDARESONANCE_API ADataInput : public AEditorTickActor {
	GENERATED_BODY()

public:
	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Transform" )
		USceneComponent* Transform;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		FIntVector InputSize = FIntVector( 6, 6, 4 );

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		int32 GridElementSize = 1000;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Required" )
		AModuleAssignee* ModuleAssignee;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Grid Debug" )
		FColor PatternStatus;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Grid Data" )
		FModuleMatrix ModuleData;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Grid Debug" )
		TMap<const AActor*, FString> Errors;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Grid Debug" )
		TArray<FModuleMatrix> Patterns;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Grid Debug" )
		TMap<FIntVector, FModuleData> ModulesMap;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Required" )
		FIntVector NSize = FIntVector( 2, 2, 2 );


public:
	ADataInput( const FObjectInitializer& ObjectInitializer );
	void Training();
	void SetMatrix(FIntVector Size);
	FModuleMatrix DefineNewPatternData(int32 InitX, int32 InitY, int32 InitZ);

protected:
	virtual void BeginPlay() override;
	virtual void Tick( float DeltaTime ) override;

private:
	void ValidateLocalLocation( AActor*& Child, FIntVector LocalLocationInt, FIntVector& Coord );
	void ValidateLocalRotation( AActor*& Child, FIntVector LocalRotationInt );
	void ValidateCoordCount( AActor*& Child, FIntVector Coord );

	void DrawCoord( AActor*& Child, FIntVector Coord, FString& DisplayText ) const;
	void DrawNameAndBit( AActor*& Child, FString DisplayText, FModuleData Module ) const;

	void ValidateInput();
	void DefinePatterns();

	bool SelectedCheck();

	TMap<FIntVector, int32> PatternLocations;

};
