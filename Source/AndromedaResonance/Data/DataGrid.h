// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "EditorTickActor.h"
#include "WaveFunctionCollapse/Module.h"
#include "Data/ModuleMatrix.h"
#include "DataGrid.generated.h"

class AModuleAssignee;

UCLASS()
class ANDROMEDARESONANCE_API ADataGrid : public AEditorTickActor {
	GENERATED_BODY()

public:
	ADataGrid( const FObjectInitializer& ObjectInitializer );

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Transform" )
		USceneComponent* Transform;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Enabled" )
		bool bEnabled;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Required" )
		AModuleAssignee* ModuleAssignee;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		FIntVector NSize = FIntVector( 2, 2, 2 );

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		int32 GridElementSize = 1000;

	UPROPERTY( EditAnywhere, BlueprintReadOnly, Category = "Grid Settings" )
		TMap<FIntVector, int32> PatternLocations;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Settings" )
		int32 Weight = 100;

	UPROPERTY ( EditAnywhere, BlueprintReadWrite, Category = "Grid Debug")
		TArray<FModuleMatrix> RotatedPatterns;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Grid Debug" )
		TArray<UChildActorComponent*> DisplayModules;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Grid Data" )
		FModuleMatrix ModuleData;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Grid Debug" )
		FColor PatternStatus;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Grid Debug" )
		TMap<const AActor*, FString> Errors;
public:
	UFUNCTION( BlueprintCallable, Category = "Matrix Calls" )
		void SetMatrix( FIntVector SetMatrix );

	UFUNCTION( BlueprintCallable, Category = "DataGrid Calls" )
		void DisplayRotatedPatterns();


	void Training( bool bBeginPlay );
	void ButtonPress();

protected:
	virtual void BeginPlay() override;
	virtual void Tick( float DeltaTime ) override;

	void SetPatternLocations();

private:
	FIntVector DefineCoord( FIntVector RelativeValueLocation ) const;

	TMap<FIntVector, AModule*> ModulesMap;

	void AppendError( const AActor* Actor, FString Error );

	void RelativeValueLocationCheck( const AActor* Actor, const FIntVector RelativeValueLocation );
	void RelativeValueRotationCheck( const AActor* Actor, const FIntVector RelativeValueRotation );
	void ValueScaleCheck( const AActor* Actor, const FVector ValueScale );

	void MapChildren();
	void UpdateMatrix();

	void FillEmptyData();

	bool SelectedCheck();

};
