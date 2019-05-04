// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "EditorTickActor.h"
#include "Data/ModuleMatrix.h"
#include "Data/DataGrid.h"
#include "Data/ModuleAssignee.h"
#include "Runtime/Engine/Classes/Components/TextRenderComponent.h"
#include "MatrixDebug.generated.h"

/**
 *
 */
UCLASS()
class ANDROMEDARESONANCE_API AMatrixDebug : public AEditorTickActor {
	GENERATED_BODY()


public:
	AMatrixDebug( const FObjectInitializer& ObjectInitializer );

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Transform" )
		USceneComponent* Transform;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, meta = ( ExposeOnSpawn = true ), Category = "Debug" )
		ADataGrid * DataGrid;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Debug" )
		FModuleMatrix ModuleMatrix;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Debug" )
		TMap<FIntVector, UTextRenderComponent*> MatrixTestRenderers;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Debug" )
		TArray<UChildActorComponent*> DisplayModules;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Debug" )
		bool bIsSet = false;

public:

	UFUNCTION( BlueprintCallable )
		void ButtonPress( FName Bit );

protected:
	virtual void BeginPlay() override;
	virtual void PostEditChangeProperty( FPropertyChangedEvent & PropertyChangedEvent ) override;
	virtual void Tick( float DeltaTime ) override;

private:

private:
	void CopyModuleMatrix( FModuleMatrix ModuleMatrixToCopy );


};
