// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Data/Orientations.h"
#include "Utility/UtilityLibrary.h"
#include "WaveFunctionCollapse/Module.h"
#include "ModuleData.generated.h"

DECLARE_LOG_CATEGORY_EXTERN( LogModuleData, Log, All );

USTRUCT( BlueprintType )
struct FModuleData {
	GENERATED_USTRUCT_BODY()

public:
	FModuleData() {}

	FModuleData( AModule* Module, FName ModuleID ) {
		if ( Module ) {
			if ( Module->IsValidLowLevel() ) {

				this->Module = Module->GetClass();
				this->ModuleID = ModuleID;
				this->RotationEuler = UUtilityLibrary::Conv_RotatorToIntVector( Module->GetActorRotation() );
				this->Scale = UUtilityLibrary::Conv_VectorToIntVector( Module->GetActorScale3D() );
				this->Symmetrical = Module->Symmetrical;
				this->Empty = false;
				this->Bit = GenerateBit();
			}
		}
	}

	FModuleData( FName Bit ) {
		if ( Bit == FName( TEXT( "Null" ) ) ) {
			this->Empty = true;
			this->Bit = FName( TEXT( "Null" ) );
		} else {
			this->Empty = false;

			FString BitString = Bit.ToString();
			FString ModuleString = BitString;
			ModuleString.RemoveAt( 1, 2, true );

			// moduleassignee different? :l

			//this->Module = *ModuleAssignee->AssignedNames.FindKey( FName( *ModuleString ) );
			this->ModuleID = FName( *ModuleString );
			// Make rotation from bit, scale from bit, etc
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
		return this->Bit == Other.Bit;
	}

	FORCEINLINE bool operator!=( const FModuleData& Other ) const {
		return !operator==( Other );
	}
};