// Fill out your copyright notice in the Description page of Project Settings.


#include "WFC.h"
#include "DrawDebugHelpers.h"
#include "Data/Orientations.h"
#include "Data/ModulePropagator.h"
#include "Utility/WaveFunctionLibrary.h"

AWFC::AWFC( const FObjectInitializer& ObjectInitializer ) {

	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;
}

void AWFC::BeginPlay() {
	Super::BeginPlay();

}

void AWFC::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );

	const UWorld* World = GetWorld();

	if ( World ) {

		for3( OutputSize.X, OutputSize.Y, OutputSize.Z,
			  {
					DrawDebugBox( World, GetActorLocation() + ( FVector( X, Y, Z ) * 1000 ), FVector::OneVector * 100, FColor::Red, false, -1.F, 0, 10.F );
			  }
		)
	}
}

void AWFC::Initialize() {

	for ( auto& Spawned : SpawnedComponents ) {
		if ( Spawned ) {
			if ( Spawned->IsValidLowLevel() ) {

				TArray<USceneComponent*> ChildComponents;
				Spawned->GetChildrenComponents( true, ChildComponents );

				for ( USceneComponent* ChildComponent : ChildComponents ) {
					ChildComponent->DestroyComponent();
				}

				Spawned->DestroyComponent();
			}
		}
	}

	SpawnedComponents.Empty();

	if ( !ModuleAssignee ) return;

	Wave = FWaveMatrix( OutputSize );

	TMap<int32, bool> InitMap;

	for ( auto& Pattern : ModuleAssignee->Patterns ) {
		InitMap.Add( Pattern.Key, true );
	}

	for3( OutputSize.X, OutputSize.Y, OutputSize.Z,
		  {
			  Wave.AddData( FIntVector( X, Y, Z ), FCoefficient( InitMap ) );
		  }
	)

	Observe( FIntVector( 0, 0, 0 ) );
}

void AWFC::Observe( FIntVector ObserveValue ) {
	FIntVector Coord = ObserveValue == FIntVector::ZeroValue ? ObserveValue : MinEntropyCoords();
	FCoefficient Coefficient = Wave.GetDataAt( Coord );

	int32 Selected = GetWeightedPattern( Coefficient.AllowedPatterns );

	TMap<int32, bool> NewAllowedPatterns;

	for ( auto& AllowedPattern : Coefficient.AllowedPatterns ) {
		if ( AllowedPattern.Key == Selected ) {
			NewAllowedPatterns.Add( AllowedPattern.Key, true );
		} else {
			NewAllowedPatterns.Add( AllowedPattern.Key, false );
		}
	}

	Coefficient.AllowedPatterns = NewAllowedPatterns;
	Wave.AddData( Coord, Coefficient );

	SpawnMod( Coord, Selected );
	Flag.Push( Coord );

	//Propagate();
}

void AWFC::Propagate() {

	Updated.Empty();

	while ( Flag.Num() > 0 ) {
		FIntVector Coord = Flag.Pop();
		Updated.Add( Coord );

		for ( auto& Direction : UOrientations::OrientationUnitVectors ) {
			if ( Direction.Key == EOrientations::NONE ) continue;

			FIntVector NeighbourCoord = Coord + Direction.Value;

			if ( Wave.IsValidCoordinate( NeighbourCoord ) ) {
				if ( Wave.GetDataAt( NeighbourCoord ).AllowedPatterns.Num() != 1 ) {
					Constrain( NeighbourCoord );
				}
			}
		}
	}
}

void AWFC::Constrain( FIntVector Coord ) {
	for ( auto& Direction : UOrientations::OrientationUnitVectors ) {
		if ( Direction.Key == EOrientations::NONE ) continue;

		FIntVector NeighbourCoord = Coord + Direction.Value;

		if ( Wave.IsValidCoordinate( NeighbourCoord ) ) {
			for ( auto& AllowedPattern : Wave.GetDataAt( NeighbourCoord ).AllowedPatterns ) {
				if ( AllowedPattern.Value ) {
					FModulePropagator AllowedPatternByPropagator = ModuleAssignee->Patterns.FindRef( AllowedPattern.Key ).Propagator.FindRef( Direction.Value * -1 );

					// Line 209 In C#
				}
			}
		}
	}
}

void AWFC::SpawnMod( FIntVector Coord, int32 Selected ) {
	FModuleMatrix SelectedPattern = ModuleAssignee->Patterns.FindRef( Selected );
	FModuleData SelectedPatternData = SelectedPattern.GetDataAt( FIntVector( 0, 0, 0 ) );

	UChildActorComponent* NewChild = UWaveFunctionLibrary::CreateModule( GetWorld(), SelectedPatternData, this, GetActorLocation() + FVector( Coord ) * 1000 );
	SpawnedComponents.Add( NewChild );

}

int32 AWFC::GetWeightedPattern( TMap<int32, bool> InPatterns ) {
	TArray<int32> WeightedPatterns;

	for ( auto& Pair : InPatterns ) {
		if ( Pair.Value ) {
			int32 WeightCount = ModuleAssignee->Weights.FindRef( Pair.Key );

			for ( int32 i = 0; i < WeightCount; i++ ) {
				WeightedPatterns.Add( Pair.Key );
			}
		}
	}

	if ( WeightedPatterns.Num() > 0 ) {

		return WeightedPatterns[FMath::RandRange( 0, WeightedPatterns.Num() - 1 )];
	}

	return -1;
}

FIntVector AWFC::MinEntropyCoords() {
	float MinEntropy = 0;

	FRandomStream RandomStream = FRandomStream( FMath::RandRange( 0.F, MAX_FLT ) );
	FIntVector RMinEntropyCoords = FIntVector();

	for3( OutputSize.X, OutputSize.Y, OutputSize.Z,
		  {
			  FIntVector CurrentCoordinates = FIntVector( X, Y, Z );

			  if ( Wave.GetDataAt( CurrentCoordinates ).AllowedPatterns.Num() > 1 ) {
				  float Entropy = ShannonEntropy( CurrentCoordinates );
				  float EntropyPlusNoise = Entropy - RandomStream.FRand();

				  if ( MinEntropy == 0 || EntropyPlusNoise < MinEntropy ) {
					  MinEntropy = EntropyPlusNoise;
					  RMinEntropyCoords = FIntVector( X, Y, Z );
				  }
			  }
		  }
	)

		return RMinEntropyCoords;
}

float AWFC::ShannonEntropy( FIntVector CurrentCoordinates ) {
	int32 SumOfWeights = 0;
	int32 SumOfWeightsLogWeights = 0;

	for ( auto& Pair : Wave.GetDataAt( CurrentCoordinates ).AllowedPatterns ) {

		int32 Weight = ModuleAssignee->Weights.FindRef( Pair.Key );

		SumOfWeights += Weight;
		SumOfWeightsLogWeights += Weight * ( float ) FMath::Loge( Weight );
	}

	return FMath::Loge( SumOfWeights ) - ( SumOfWeightsLogWeights / SumOfWeights );
}
