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

	if ( !bInitialized ) return;

	if ( World ) {

		for3( OutputSize.X, OutputSize.Y, OutputSize.Z,
			  {
					FIntVector Coord = FIntVector( X, Y, Z );
					int32 CurrentAllowedCount = Wave.GetDataAt( Coord ).AllowedCount();

					DrawDebugBox( World, GetActorLocation() + ( FVector( X, Y, Z ) * 1000 ), ( FVector::OneVector * 25 ) * CurrentAllowedCount, FColor::Red, false, -1.F, 0, 10.F );
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
	HasModule.Empty();

	for ( auto& Pattern : ModuleAssignee->Patterns ) {
		InitMap.Add( Pattern.Key, true );
	}

	for3( OutputSize.X, OutputSize.Y, OutputSize.Z,
		  {
			  Wave.AddData( FIntVector( X, Y, Z ), FCoefficient( InitMap ) );
		  }
	)

		for3( OutputSize.X, OutputSize.Y, OutputSize.Z,
			  {
				  FCoefficient WaveDataAtCoord = Wave.GetDataAt( FIntVector( X, Y, Z ) );

				  if ( WaveDataAtCoord.AllowedCount() == 0 ) {
					  UE_LOG( LogTemp, Error, TEXT( "Invalid construction in WFC... Retrying!" ) );
					  Initialize();
				  }
			  }
		)

			  bInitialized = true;
}

void AWFC::Observe( FIntVector ObserveValue, int32 Selected = -1 ) {
	FIntVector Coord = ObserveValue == FIntVector::ZeroValue ? ObserveValue : MinEntropyCoords();
	FCoefficient Coefficient = Wave.GetDataAt( Coord );

	int32 GetSelected = Selected;

	if ( Selected == -1 ) {
		GetSelected = GetWeightedPattern( Coefficient.AllowedPatterns );
	}

	TMap<int32, bool> NewAllowedPatterns;

	for ( auto& AllowedPattern : Coefficient.AllowedPatterns ) {
		if ( AllowedPattern.Key == GetSelected ) {
			NewAllowedPatterns.Add( AllowedPattern.Key, true );
		} else {
			NewAllowedPatterns.Add( AllowedPattern.Key, false );
		}
	}

	Coefficient.AllowedPatterns = NewAllowedPatterns;
	Wave.AddData( Coord, Coefficient );

	SpawnMod( Coord, GetSelected );
	Flag.Push( Coord );

	Propagate();
}

void AWFC::CreateFromJson( FWaveMatrix JsonWave )
{
	OutputSize = FIntVector(JsonWave.SizeX , JsonWave.SizeY , JsonWave.SizeZ  );
	Initialize();

	for3 ( OutputSize.X, OutputSize.Y, OutputSize.Z, {
		FIntVector Coord = FIntVector( X,Y,Z );
		SpawnMod(Coord, JsonWave.GetDataAt(Coord).LastAllowedPatternIndex());

		UE_LOG( LogTemp, Warning, TEXT( "%s" ), *Coord.ToString());
	})
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
				if ( Wave.GetDataAt( NeighbourCoord ).AllowedCount() != 1 ) {
					Constrain( NeighbourCoord );
				}
			}
		}
	}
}

void AWFC::Constrain( FIntVector Coord ) {
	TMap<EOrientations, FPatternIndexArray> NewValidData;

	for ( auto& Direction : UOrientations::OrientationUnitVectors ) {

		if ( Direction.Key == EOrientations::NONE ) continue;

		FIntVector NeighbourCoord = Coord + Direction.Value;

		if ( Wave.IsValidCoordinate( NeighbourCoord ) ) {
			for ( auto& AllowedPattern : Wave.GetDataAt( NeighbourCoord ).AllowedPatterns ) {
				if ( AllowedPattern.Value ) {
					FModulePropagator AllowedPatternByPropagator = ModuleAssignee->Patterns.FindRef( AllowedPattern.Key ).Propagator.FindRef( Direction.Value * -1 );

					if ( NewValidData.Contains( Direction.Key ) ) {
						FPatternIndexArray ValidHashSet = NewValidData.FindRef( Direction.Key );

						for ( auto& Pattern : AllowedPatternByPropagator.Array ) {
							ValidHashSet.Array.AddUnique( Pattern );
						}

						NewValidData.Add( Direction.Key, ValidHashSet );
					} else {
						FPatternIndexArray ValidHashSet = FPatternIndexArray();

						for ( auto& Pattern : AllowedPatternByPropagator.Array ) {
							ValidHashSet.Array.AddUnique( Pattern );
						}

						NewValidData.Add( Direction.Key, ValidHashSet );
					}
				}
			}
		}
	}

	TMap<int32, int32> PatternCounts;

	for ( auto& ValidData : NewValidData ) {
		for ( int32 PatternIndex : ValidData.Value.Array ) {
			if ( PatternCounts.Contains( PatternIndex ) ) {
				int32 Count = PatternCounts.FindRef( PatternIndex );
				Count++;
				PatternCounts.Add( PatternIndex, Count );
			} else {
				PatternCounts.Add( PatternIndex, 0 );
			}
		}
	}

	TMap<int32, bool> NewAllowedPatterns = Wave.GetDataAt( Coord ).AllowedPatterns;
	TMap<int32, bool> TempNewAllowedPatterns = NewAllowedPatterns;

	int32 ChangedCount = 0;

	for ( auto& PatternAllowed : NewAllowedPatterns ) {
		if ( PatternAllowed.Value ) {
			ChangedCount++;
		}

		TempNewAllowedPatterns.Add( PatternAllowed.Key, false );
	}

	NewAllowedPatterns = TempNewAllowedPatterns;

	for ( auto& PatternCount : PatternCounts ) {
		if ( PatternCount.Value == NewValidData.Num() - 1 ) {
			NewAllowedPatterns.Add( PatternCount.Key, true );
			ChangedCount--;
		}
	}

	for ( auto& Direction : UOrientations::OrientationUnitVectors ) {
		if ( Direction.Key == EOrientations::NONE ) continue;

		FIntVector NeighbourCoord = Coord + Direction.Value;

		if ( Wave.IsValidCoordinate( NeighbourCoord ) ) {
			if ( ChangedCount > 0 && !Updated.Contains( NeighbourCoord ) ) {
				Flag.Push( NeighbourCoord );
			}
		}
	}

	FCoefficient Coefficient = Wave.GetDataAt( Coord );
	Coefficient.AllowedPatterns = NewAllowedPatterns;
	Wave.AddData( Coord, Coefficient );

	if ( Wave.GetDataAt( Coord ).AllowedCount() == 1 ) {
		int32 PatternIndexSelected = Wave.GetDataAt( Coord ).LastAllowedPatternIndex();
		SpawnMod( Coord, PatternIndexSelected );
	}

	Updated.Add( Coord );

}

void AWFC::SpawnMod( FIntVector Coord, int32 Selected ) {
	if ( ModuleAssignee ) {
		if ( ModuleAssignee->Patterns.Contains( Selected ) ) {

			FModuleMatrix SelectedPattern = ModuleAssignee->Patterns.FindRef( Selected );


			for3( SelectedPattern.SizeX, SelectedPattern.SizeY, SelectedPattern.SizeZ,
				  {
					  const FIntVector CoordAndPatternCoord = Coord + FIntVector( X,Y,Z );
					  if ( Wave.IsValidCoordinate( CoordAndPatternCoord ) && !HasModule.Contains( CoordAndPatternCoord ) ) {
							const FModuleData SelectedPatternData = SelectedPattern.GetDataAt( FIntVector( X, Y, Z ) );

							UChildActorComponent* NewChild = UWaveFunctionLibrary::CreateModule( GetWorld(), SelectedPatternData, this, GetActorLocation() + ( FVector( Coord ) * 1000 ) + ( FVector( X,Y,Z ) * 1000 ) );
							SpawnedComponents.Add( NewChild );
							HasModule.Add( CoordAndPatternCoord, true );
					  }
				  }
		} )
	}
}

bool AWFC::IsFullyCollapsed() {
	int32 AllowedCount = 0;

	for3( OutputSize.X, OutputSize.Y, OutputSize.Z,
		  {
				FCoefficient WaveDataAtCoord = Wave.GetDataAt( FIntVector( X, Y, Z ) );

				if ( WaveDataAtCoord.AllowedCount() == 0 ) {
					UE_LOG( LogTemp, Error, TEXT( "Invalid construction in WFC... Retrying!" ) );

					Initialize();
					StartWFC();
				}

				if ( WaveDataAtCoord.AllowedCount() > AllowedCount ) {
					AllowedCount = Wave.GetDataAt( FIntVector( X, Y, Z ) ).AllowedCount();
				}
		  }
	)

		return AllowedCount > 1 ? false : true;
}

void AWFC::StartWFC() {
	//Observe( FIntVector( 1, 0, 0 ) );


	while ( !IsFullyCollapsed() ) {
		Observe( MinEntropyCoords() );
	}
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

			  if ( Wave.GetDataAt( CurrentCoordinates ).AllowedCount() > 1 ) {
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
