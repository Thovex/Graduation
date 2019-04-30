// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Data/Orientations.h"
#include "Data/ModuleAssignee.h"
#include "Utility/UtilityLibrary.h"
#include "ModuleData.generated.h"

class AModule;

DECLARE_LOG_CATEGORY_EXTERN( LogModuleData, Log, All );

USTRUCT( BlueprintType )
struct FModuleData {
	GENERATED_USTRUCT_BODY()

public:
	FModuleData() {}

	FModuleData( AModule* Module ) {
		if ( Module ) {
			if ( Module->IsValidLowLevel() ) {

				this->Module = Module->GetClass();
				this->ModuleID = Module->ModuleAssignee->AssignedNames.FindRef( this->Module );
				this->RotationEuler = UUtilityLibrary::Conv_RotatorToIntVector( Module->GetActorRotation() );
				this->Scale = UUtilityLibrary::Conv_VectorToIntVector( Module->GetActorScale3D() );
				this->Symmetrical = Module->Symmetrical;
				this->Empty = false;
				this->Bit = GenerateBit();
			}
		}
	}

	FModuleData( bool Empty ) {
		this->Empty = Empty;
		this->Bit = FName( TEXT( "Null" ) );
	}

	UPROPERTY( EditAnywhere, BlueprintReadOnly, Category = "Module Data" )
		TSubclassOf<AModule> Module;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Module Data" )
		FName ModuleID;

	UPROPERTY( EditAnywhere, BlueprintReadOnly, Category = "Module Data" )
		FIntVector RotationEuler;

	UPROPERTY( EditAnywhere, BlueprintReadOnly, Category = "Module Data" )
		FIntVector Scale;

	UPROPERTY( EditAnywhere, BlueprintReadOnly, Category = "Module Data" )
		bool Symmetrical;

	UPROPERTY( EditAnywhere, BlueprintReadOnly, Category = "Module Data" )
		bool Empty;

	UPROPERTY( VisibleAnywhere, BlueprintReadOnly, Category = "Module Data" )
		FName Bit;


	FORCEINLINE FName GenerateBit() {

		if ( Empty ) {
			return FName( TEXT( "Null" ) );
		}

		FString GeneratedBit = ModuleID.ToString();

		if ( Symmetrical ) {
			GeneratedBit.Append( "S" );
		} else {
			EOrientations Orientation = UOrientations::EulerToOrientation( RotationEuler );
			const FName* EulerFName = UOrientations::OrientationByFName.FindKey( Orientation );

			if ( EulerFName != nullptr ) {
				GeneratedBit.Append( EulerFName->ToString() );
			}
		}

		if ( Scale == FIntVector( 1, 1, 1 ) ) {
			GeneratedBit.Append( "N" );
		} else if ( Scale == FIntVector( -1, 1, 1 ) ) {
			GeneratedBit.Append( "X" );
		} else if ( Scale == FIntVector( 1, -1, 1 ) ) {
			GeneratedBit.Append( "Y" );
		}
		return FName( *GeneratedBit );
	}

	FORCEINLINE bool operator==( const FModuleData& Other ) const {
		if ( this->Empty != Other.Empty ) return false;
		if ( this->Bit != Other.Bit ) return false;

		if ( this->Module != Other.Module ) return false;
		if ( this->ModuleID != Other.ModuleID ) return false;
		if ( this->RotationEuler != Other.RotationEuler ) return false;
		if ( this->Scale != Other.Scale ) return false;

		return true;
	}
};