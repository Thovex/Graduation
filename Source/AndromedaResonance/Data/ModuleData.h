// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Data/Orientations.h"
#include "Utility/UtilityLibrary.h"
#include "WaveFunctionCollapse/Module.h"
#include "Runtime/Core/Public/Containers/UnrealString.h"
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

	FModuleData( FName Bit, TMap<TSubclassOf<AModule>, FName> AssigneeMap, bool Log = false ) {

		int32 Length = Bit.ToString().Len();

		if ( Length < 3 || Length > 4 ) {
			UE_LOG( LogModuleData, Warning, TEXT( "INVALID MODULE BIT" ) );
			return;
		}

		if ( Bit == FName( TEXT( "Null" ) ) ) {
			this->Empty = true;
			this->Bit = FName( TEXT( "Null" ) );
		} else {
			this->Empty = false;

			FString BitString = Bit.ToString();
			FString ModuleIDString = BitString;
			ModuleIDString.RemoveAt( 1, 2, true );

			this->ModuleID = FName( *ModuleIDString );

			if ( !*AssigneeMap.FindKey( this->ModuleID ) ) {
				UE_LOG( LogModuleData, Warning, TEXT( "INVALID MODULE ID" ) );
				return;
			}

			this->Module = *AssigneeMap.FindKey( this->ModuleID );

			FString RotationEulerString = BitString;
			RotationEulerString.RemoveAt( 0, 1, true );
			RotationEulerString.RemoveAt( 1, 1, true );

			FName RotationEulerChar = FName( *RotationEulerString );

			if ( RotationEulerChar == FName( TEXT( "S" ) ) ) {
				this->RotationEuler = FIntVector( 0, 0, 0 );
				this->Symmetrical = true;
			} else {

				EOrientations RotationOrientation = UOrientations::OrientationByFName.FindRef( RotationEulerChar );
				this->RotationEuler = UOrientations::OrientationEulers.FindRef( RotationOrientation );
				this->Symmetrical = false;
			}

			FString ScaleString = BitString;
			ScaleString.RemoveAt( 0, 2, true );

			if ( ScaleString == "N" ) {
				this->Scale = FIntVector( 1, 1, 1 );
			} else if ( ScaleString == "X" ) {
				this->Scale = FIntVector( -1, 1, 1 );
			} else if ( ScaleString == "Y" ) {
				this->Scale = FIntVector( 1, -1, 1 );
			}

			this->Bit = Bit;

			if ( Log ) {
				UE_LOG( LogModuleData, Warning, TEXT( "=== FMODULE CREATED START ===" ) );
				UE_LOG( LogModuleData, Warning, TEXT( "this.Module = %s" ), *this->Module->GetFName().ToString() );
				UE_LOG( LogModuleData, Warning, TEXT( "this.ModuleID = %s" ), *this->ModuleID.ToString() );
				UE_LOG( LogModuleData, Warning, TEXT( "this.RotationEuler = %s" ), *this->RotationEuler.ToString() );
				UE_LOG( LogModuleData, Warning, TEXT( "this.Scale = %s" ), *this->Scale.ToString() );
				UE_LOG( LogModuleData, Warning, TEXT( "this.Symmetrical = %s" ), this->Symmetrical ? TEXT( "true" ) : TEXT( "false" ) );
				UE_LOG( LogModuleData, Warning, TEXT( "this.Empty = %s" ), this->Empty ? TEXT( "true" ) : TEXT( "false" ) );
				UE_LOG( LogModuleData, Warning, TEXT( "this.Bit = %s" ), *this->Bit.ToString() );
				UE_LOG( LogModuleData, Warning, TEXT( "=== FMODULE CREATED END ===" ) );
			}
		}
	}

	FModuleData( bool Empty ) {
		this->Empty = Empty;
		this->Bit = FName( TEXT( "Null" ) );
	}

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		TSubclassOf<AModule> Module;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		FName ModuleID;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		FIntVector RotationEuler;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		FIntVector Scale;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		bool Symmetrical;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
		bool Empty;

	UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = "Module Data" )
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

	FORCEINLINE void SetRotationEuler( EOrientations Rotation ) {
		FIntVector RotationDirection = UOrientations::OrientationEulers.FindRef( Rotation );
		RotationEuler += RotationDirection;
		Bit = GenerateBit();
	}

	FORCEINLINE bool operator==( const FModuleData & Other ) const {
		return this->Bit == Other.Bit;
	}

	FORCEINLINE bool operator!=( const FModuleData & Other ) const {
		return !operator==( Other );
	}

	FString ToString() const {

		if ( Module ) {
			return FString::Printf( TEXT( "[Bit:%s][Module:%s][ID:%s][RotationEuler:%s][Scale:%s][Symmetrical:%s]" ),
									*Bit.ToString(), *Module->GetFName().ToString(), *ModuleID.ToString(), *RotationEuler.ToString(), *Scale.ToString(),
									Symmetrical ? TEXT( "TRUE" ) : TEXT( "FALSE" ) );
		} else {
			return FString::Printf( TEXT( "[Bit:%s]" ),
									 *Bit.ToString() );
		}

	}
};