// Fill out your copyright notice in the Description page of Project Settings.


#include "WaveFunctionLibrary.h"
#include "Runtime/Core/Public/Math/NumericLimits.h"
#include "Runtime/Engine/Classes/Components/TextRenderComponent.h"
#include "Runtime/Engine/Classes/Materials/MaterialInstanceDynamic.h"
#include "Data/ModuleAssignee.h"
#include "ConstructorHelpers.h"
#include "UObjectGlobals.h"
#include "Data/WaveMatrix.h"


UChildActorComponent* UWaveFunctionLibrary::CreateModule( UObject* WorldContextObject, FModuleData ModuleData, AActor* ParentActor, FVector Location ) {
	UWorld* World = WorldContextObject->GetWorld();

	if ( ModuleData.Module && ParentActor ) {

		UChildActorComponent* ChildActor = NewObject<UChildActorComponent>( ParentActor, UChildActorComponent::StaticClass() );
		ChildActor->SetChildActorClass( ModuleData.Module );
		ChildActor->RegisterComponent();

		ChildActor->SetWorldLocation( Location );
		ChildActor->SetWorldRotation( UUtilityLibrary::Conv_IntVectorToRotator( ModuleData.RotationEuler ) );

		// This is still quite odd but turning on two-sided materials fixes this issue for now, it's some material 
		// related issue, the code seems fine :-/
		ChildActor->SetWorldScale3D( FVector( ModuleData.Scale ) );

		ChildActor->AttachToComponent( ParentActor->GetRootComponent(), FAttachmentTransformRules::KeepWorldTransform );

		/*
		UTextRenderComponent* ChildActorBitRenderer = NewObject<UTextRenderComponent>( ChildActor, UTextRenderComponent::StaticClass() );
		ChildActorBitRenderer->RegisterComponent();

		ChildActorBitRenderer->AttachToComponent( ChildActor, FAttachmentTransformRules::SnapToTargetIncludingScale );
		ChildActorBitRenderer->SetText( FText::FromString( ModuleData.Bit.ToString() ) );

		ChildActorBitRenderer->SetXScale( 5 );
		ChildActorBitRenderer->SetYScale( 5 );
		ChildActorBitRenderer->SetHorizontalAlignment( EHorizTextAligment::EHTA_Center );
		ChildActorBitRenderer->SetVerticalAlignment( EVerticalTextAligment::EVRTA_TextCenter );

		ChildActorBitRenderer->SetWorldLocation( ChildActorBitRenderer->GetComponentLocation() + FVector::UpVector * 250 );
		ChildActorBitRenderer->SetWorldRotation( FRotator( 0, -90, 0 ) );
		ChildActorBitRenderer->SetWorldScale3D( FVector(1, 1, -1) );

		UMaterial * TextMaterial = LoadObjFromPath<UMaterial>( FName( TEXT( "/Game/Materials/M_Text_FacingCamera.M_Text_FacingCamera" ) ) );

		if ( TextMaterial ) {
			UMaterialInstanceDynamic* TextMaterialInstance = UMaterialInstanceDynamic::Create(TextMaterial, WorldContextObject);

			if ( TextMaterialInstance ) {
				ChildActorBitRenderer->SetTextMaterial( TextMaterialInstance );
			}
		}
		*/

		return ChildActor;
	}

	return nullptr;
}

TArray<UChildActorComponent*> UWaveFunctionLibrary::CreatePatternIndex( UObject* WorldContextObject, AActor* ParentActor, AModuleAssignee* ModuleAssignee, int32 PatternIndex, FVector Location ) {
	TArray<UChildActorComponent*> Components;

	if ( ModuleAssignee ) {
		FModuleMatrix SelectedPatternMatrix = ModuleAssignee->Patterns.FindRef( PatternIndex );

		FIntVector Size = FIntVector( ForceInit );

		Size.X = SelectedPatternMatrix.SizeX;
		Size.Y = SelectedPatternMatrix.SizeY;
		Size.Z = SelectedPatternMatrix.SizeZ;

		for3( Size.X, Size.Y, Size.Z, {
			UChildActorComponent * NewModule = CreateModule( WorldContextObject, SelectedPatternMatrix.GetDataAt( FIntVector( X,Y,Z ) ), ParentActor, Location + FVector( X,Y,Z ) * 1000 );
			Components.Add( NewModule );
			  } )

	}
	return Components;
}

TArray<UChildActorComponent*> UWaveFunctionLibrary::CreatePatternData( UObject * WorldContextObject, AActor * ParentActor, AModuleAssignee * ModuleAssignee, FModuleMatrix Pattern, FVector Location ) {

	TArray<UChildActorComponent*> Components;

	FIntVector Size = FIntVector( ForceInit );

	Size.X = Pattern.SizeX;
	Size.Y = Pattern.SizeY;
	Size.Z = Pattern.SizeZ;

	for3( Size.X, Size.Y, Size.Z, {
		UChildActorComponent * NewModule = CreateModule( WorldContextObject, Pattern.GetDataAt( FIntVector( X,Y,Z ) ), ParentActor, Location + FVector( X,Y,Z ) * 1000 );
		Components.Add( NewModule );
		  } )

		return Components;
}


FModuleData UWaveFunctionLibrary::CreateModuleDataFromBit( FName Bit, AModuleAssignee * ModuleAssignee ) {
	if ( ModuleAssignee ) {
		TMap<TSubclassOf<AModule>, FName> Map = ModuleAssignee->AssignedNames;

		return FModuleData( Bit, Map );
	}

	return FModuleData();
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesByEmptyQuery( bool& HasResults, int32 & ResultCount, const FWaveMatrix Wave, const bool EmptyQuery ) {
	TArray<FIntVector> Coordinates;

	for ( auto& Pair : Wave.WaveValues ) {
		if ( Pair.Value.Empty == EmptyQuery ) {
			Coordinates.Add( Pair.Key );
		}
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesByModuleClassQuery( bool& HasResults, int32 & ResultCount, const FWaveMatrix Wave, const TSubclassOf<AModule> ModuleQuery ) {
	TArray<FIntVector> Coordinates;

	for ( auto& Pair : Wave.WaveValues ) {
		if ( Pair.Value.Module == ModuleQuery ) {
			Coordinates.Add( Pair.Key );
		}
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesByModuleIDQuery( bool& HasResults, int32 & ResultCount, const FWaveMatrix Wave, const FName ModuleIDQuery ) {
	TArray<FIntVector> Coordinates;

	for ( auto& Pair : Wave.WaveValues ) {
		if ( Pair.Value.ModuleID == ModuleIDQuery ) {
			Coordinates.Add( Pair.Key );
		}
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesByZRotationQuery( bool& HasResults, int32 & ResultCount, FWaveMatrix Wave, ERotationQuery ZRotation ) {
	TArray<FIntVector> Coordinates;

	for ( auto& Pair : Wave.WaveValues ) {
		switch ( ZRotation ) {
			case ERotationQuery::ERQ_Forward:
			{
				const int32 EulerZ = FMath::Abs( Pair.Value.RotationEuler.Z );

				if ( EulerZ == 0 || EulerZ == 360 ) {
					Coordinates.Add( Pair.Key );
				}

			} break;
			case ERotationQuery::ERQ_Left:
			{
				if ( Pair.Value.RotationEuler.Z == -90 || Pair.Value.RotationEuler.Z == 270 ) {
					Coordinates.Add( Pair.Key );
				}
			} break;
			case ERotationQuery::ERQ_Right:
			{
				if ( Pair.Value.RotationEuler.Z == 90 || Pair.Value.RotationEuler.Z == -270 ) {
					Coordinates.Add( Pair.Key );
				}
			} break;
			case ERotationQuery::ERQ_Back:
			{
				const int32 EulerZ = FMath::Abs( Pair.Value.RotationEuler.Z );

				if ( EulerZ == 180 ) {
					Coordinates.Add( Pair.Key );
				}
			} break;
		}
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesByXRowQuery( bool& HasResults, int32 & ResultCount, FWaveMatrix Wave, int32 XRowQuery ) {
	TArray<FIntVector> Coordinates;

	for ( auto& Pair : Wave.WaveValues ) {
		if ( Pair.Key.X == XRowQuery ) {
			Coordinates.Add( Pair.Key );
		}
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesByYRowQuery( bool& HasResults, int32 & ResultCount, FWaveMatrix Wave, int32 YRowQuery ) {
	TArray<FIntVector> Coordinates;

	for ( auto& Pair : Wave.WaveValues ) {
		if ( Pair.Key.Y == YRowQuery ) {
			Coordinates.Add( Pair.Key );
		}
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesByZRowQuery( bool& HasResults, int32 & ResultCount, FWaveMatrix Wave, int32 ZRowQuery ) {
	TArray<FIntVector> Coordinates;

	for ( auto& Pair : Wave.WaveValues ) {
		if ( Pair.Key.Z == ZRowQuery ) {
			Coordinates.Add( Pair.Key );
		}
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesValidatedTwo( bool& HasResults, int32 & ResultCount, 
	const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo ) {
	TArray<FIntVector> Coordinates;

	const int32 CheckCount = 2;

	TMap<FIntVector, int32> CounterMap;

	for ( auto& Coords : CoordinateSetOne ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetTwo ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Pair : CounterMap ) {
		if ( Pair.Value == CheckCount ) Coordinates.Add( Pair.Key );
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;

}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesValidatedThree(bool& HasResults, int32& ResultCount,
	const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo,
	const TArray<FIntVector> CoordinateSetThree)
{
	TArray<FIntVector> Coordinates;

	const int32 CheckCount = 3;

	TMap<FIntVector, int32> CounterMap;

	for ( auto& Coords : CoordinateSetOne ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetTwo ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetThree ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Pair : CounterMap ) {
		if ( Pair.Value == CheckCount ) Coordinates.Add( Pair.Key );
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesValidatedFour(bool& HasResults, int32& ResultCount,
	const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo,
	const TArray<FIntVector> CoordinateSetThree, const TArray<FIntVector> CoordinateSetFour)
{
	TArray<FIntVector> Coordinates;

	const int32 CheckCount = 4;

	TMap<FIntVector, int32> CounterMap;

	for ( auto& Coords : CoordinateSetOne ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetTwo ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetThree ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetFour ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Pair : CounterMap ) {
		if ( Pair.Value == CheckCount ) Coordinates.Add( Pair.Key );
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesValidatedFive(bool& HasResults, int32& ResultCount,
	const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo,
	const TArray<FIntVector> CoordinateSetThree, const TArray<FIntVector> CoordinateSetFour,
	const TArray<FIntVector> CoordinateSetFive)
{
	TArray<FIntVector> Coordinates;

	const int32 CheckCount = 5;

	TMap<FIntVector, int32> CounterMap;

	for ( auto& Coords : CoordinateSetOne ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetTwo ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetThree ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetFour ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetFive ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Pair : CounterMap ) {
		if ( Pair.Value == CheckCount ) Coordinates.Add( Pair.Key );
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

TArray<FIntVector> UWaveFunctionLibrary::GetCoordinatesValidatedSix(bool& HasResults, int32& ResultCount,
	const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo,
	const TArray<FIntVector> CoordinateSetThree, const TArray<FIntVector> CoordinateSetFour,
	const TArray<FIntVector> CoordinateSetFive, const TArray<FIntVector> CoordinateSetSix)
{
	TArray<FIntVector> Coordinates;

	const int32 CheckCount = 6;

	TMap<FIntVector, int32> CounterMap;

	for ( auto& Coords : CoordinateSetOne ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetTwo ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetThree ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetFour ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetFive ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Coords : CoordinateSetSix ) {
		const int32 Count = CounterMap.FindOrAdd( Coords );
		CounterMap.Add( Coords, Count + 1 );
	}

	for ( auto& Pair : CounterMap ) {
		if ( Pair.Value == CheckCount ) Coordinates.Add( Pair.Key );
	}

	QuerySetReferenceValues( HasResults, ResultCount, Coordinates );
	return Coordinates;
}

void UWaveFunctionLibrary::QuerySetReferenceValues( bool& HasResults, int32& ResultCount, const TArray<FIntVector> Coordinates ) {
	if ( Coordinates.Num() > 0 ) {
		HasResults = true;
		ResultCount = Coordinates.Num();
	} else {
		HasResults = false;
		ResultCount = 0;
	}
}
