// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Module.generated.h"

DECLARE_LOG_CATEGORY_EXTERN( LogModule, Log, All );

USTRUCT( BlueprintType )
struct FModuleData {
	GENERATED_USTRUCT_BODY()

public:
	FModuleData() {}

	FModuleData( TSubclassOf<AModule> Module, bool Empty ) {
		this->Module = Module;
		this->Empty = Empty;
	}

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Data" )
		bool Empty;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Matrix Data" )
		TSubclassOf<AModule> Module;

};

UCLASS(BlueprintType)
class ANDROMEDARESONANCE_API AModule : public AActor
{
	GENERATED_BODY()
	
public:	

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Transform" )
		USceneComponent* Transform;

	AModule( const FObjectInitializer& ObjectInitializer );

	FModuleData ToModuleData();

protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;

};
