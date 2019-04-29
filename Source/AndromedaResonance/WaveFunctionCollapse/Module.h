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

	FModuleData( bool Empty ) {
		this->Empty = Empty;
		this->Bit = "Null";
	}

	FModuleData( TSubclassOf<AModule> Module, FIntVector RotationEuler, FIntVector Scale, FName Bit, bool Empty ) {
		this->Module = Module;
		this->RotationEuler = RotationEuler;
		this->Scale = Scale;
		this->Bit = Bit;
		this->Empty = Empty;
	}

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		bool Empty;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		FIntVector RotationEuler;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		FIntVector Scale;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		TSubclassOf<AModule> Module;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Module Data" )
		FName Bit;

	FORCEINLINE bool operator==( const FModuleData& Other ) const {
		bool bIsEqual = true;

		if ( this->Module != Other.Module ) bIsEqual = false;
		if ( this->RotationEuler != Other.RotationEuler ) bIsEqual = false;
		if ( this->Scale != Other.Scale ) bIsEqual = false;
		if ( this->Bit != Other.Bit ) bIsEqual = false;
		if ( this->Empty != Other.Empty ) bIsEqual = false;

		return bIsEqual;
	}


};

class AModuleAssignee;

UCLASS( BlueprintType )
class ANDROMEDARESONANCE_API AModule : public AActor {
	GENERATED_BODY()

public:
	AModule( const FObjectInitializer& ObjectInitializer );

public:

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Transform" )
		USceneComponent* Transform;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		bool Symmetrical = false;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Module Data" )
		FModuleData ModuleData;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Module Data" )
		AModuleAssignee* ModuleAssignee;

public:

	FModuleData ToModuleData();
	FName GenerateBit();

protected:
	virtual void BeginPlay() override;
	virtual void Tick( float DeltaTime ) override;

private:
	FIntVector GetFIntVectorEuler();

	FIntVector GetFIntVectorScale();
	FName GetFNameFromScaleVector();

};
