// Fill out your copyright notice in the Description page of Project Settings.


#include "WFC.h"
#include "DrawDebugHelpers.h"
#include "Data/Orientations.h"
#include "Data/ModulePropagator.h"
#include "Utility/WaveFunctionLibrary.h"

DECLARE_CYCLE_STAT( TEXT( "WFC - OBSERVE" ), STAT_Observe, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - PROPAGATE" ), STAT_Propagate, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - CONSTRAIN" ), STAT_Constrain, STATGROUP_WFC );

DECLARE_CYCLE_STAT( TEXT( "WFC - CONSTRAIN - Internal 1" ), STAT_Constrain_Internal_One, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - CONSTRAIN - Internal 2" ), STAT_Constrain_Internal_Two, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - CONSTRAIN - Internal 3" ), STAT_Constrain_Internal_Three, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - CONSTRAIN - Internal 4" ), STAT_Constrain_Internal_Four, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - CONSTRAIN - Internal 5" ), STAT_Constrain_Internal_Five, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - CONSTRAIN - Internal 6" ), STAT_Constrain_Internal_Six, STATGROUP_WFC );

DECLARE_CYCLE_STAT( TEXT( "WFC - CONSTRAIN - Internal 1 - Internal 1" ), STAT_Constrain_Internal_One_One, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - CONSTRAIN - Internal 1 - Internal 2" ), STAT_Constrain_Internal_One_Two, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - CONSTRAIN - Internal 1 - Internal 3" ), STAT_Constrain_Internal_One_Three, STATGROUP_WFC );

DECLARE_CYCLE_STAT( TEXT( "WFC - ENTROPY" ), STAT_Entropy, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - SHANNONENTROPY" ), STAT_ShannonEntropy, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - WHILENOTDONE" ), STAT_WhileNotDone, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - INITIALIZE" ), STAT_Initialize, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - SPAWNMOD" ), STAT_SpawnMod, STATGROUP_WFC );
DECLARE_CYCLE_STAT( TEXT( "WFC - WEIGHTEDPATTERN" ), STAT_WeightedPattern, STATGROUP_WFC );

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
					const FIntVector Coord = FIntVector( X, Y, Z );
					const int32 CurrentAllowedCount = Wave.GetDataAt( Coord ).AllowedCount();

					DrawDebugBox( World, GetActorLocation() + ( FVector( X, Y, Z ) * 1000 ), ( FVector::OneVector * 25 ) * CurrentAllowedCount, FColor::Red, false, -1.F, 0, 10.F );
			  }
		)
	}
}


void AWFC::Initialize() {
	SCOPE_CYCLE_COUNTER( STAT_Initialize );

	Cycle = 0;

	if ( !DoOnceInitialize ) {
		Tries = 0;
		DoOnceInitialize = true;
	}

	for ( auto& Spawned : SpawnedComponents ) {
		if ( Spawned.Value ) {
			if ( Spawned.Value->IsValidLowLevel() ) {

				TArray<USceneComponent*> ChildComponents;
				Spawned.Value->GetChildrenComponents( true, ChildComponents );

				for ( USceneComponent* ChildComponent : ChildComponents ) {
					ChildComponent->DestroyComponent();
				}

				Spawned.Value->DestroyComponent();
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

		bInitialized = true;

		  if ( HasPreInitializedData ) {
			  FillInitialData( PreInitializedWave );
		  }
}

void AWFC::Observe( FIntVector ObserveValue, int32 Selected = -1 ) {
	SCOPE_CYCLE_COUNTER( STAT_Observe );

	const FIntVector Coord = ObserveValue == FIntVector::ZeroValue ? ObserveValue : MinEntropyCoords();
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

void AWFC::CreateFromJson( FWaveMatrix JsonWave ) {
	OutputSize = FIntVector( JsonWave.SizeX, JsonWave.SizeY, JsonWave.SizeZ );
	Initialize();

	for3( OutputSize.X, OutputSize.Y, OutputSize.Z,
		  {
			  const FIntVector Coord = FIntVector( X,Y,Z );
			  SpawnMod( Coord, JsonWave.GetDataAt( Coord ).LastAllowedPatternIndex() );
		  }
	)
}

void AWFC::CreateFromJsonIndex( FWaveMatrix JsonWave, int32 Index ) {

	const FIntVector Coord = *JsonWave.WaveCycle.FindKey( Index );

	for ( auto& Pair : JsonWave.WaveCycle ) {
		if ( Pair.Key == Coord ) {
			SpawnMod( Coord, JsonWave.GetDataAt( Coord ).LastAllowedPatternIndex() );
		}
	}
}

void AWFC::FillInitialData( FWaveMatrix JsonWave ) {
	HasPreInitializedData = true;
	PreInitializedWave = JsonWave;

	for3( JsonWave.SizeX, JsonWave.SizeY, JsonWave.SizeZ,
		  {
			  FIntVector Coord = FIntVector( X,Y,Z );

			  if ( JsonWave.GetDataAt( Coord ).LastAllowedPatternIndex() != -1 ) {
				  Wave.AddData( Coord, FCoefficient( JsonWave.GetDataAt( Coord ).LastAllowedPatternIndex() ) );
				  SpawnMod( Coord, JsonWave.GetDataAt( Coord ).LastAllowedPatternIndex() );
				  Flag.Push( Coord );
			  }
		  }
	)

	Propagate();
}

void AWFC::Propagate() {
	SCOPE_CYCLE_COUNTER( STAT_Propagate );

	Updated.Empty();

	while ( Flag.Num() > 0 ) {
		FIntVector Coord = Flag.Pop();
		Updated.Add( Coord );

		for ( auto& Direction : UOrientations::OrientationUnitVectors ) {
			if ( Direction.Key == EOrientations::NONE ) continue;

			const FIntVector NeighbourCoord = Coord + Direction.Value;

			if ( Wave.IsValidCoordinate( NeighbourCoord ) ) {
				if ( Wave.GetDataAt( NeighbourCoord ).AllowedCount() != 1 ) {
					Constrain( NeighbourCoord );
				}
			}
		}
	}
}

void AWFC::Constrain( FIntVector Coord ) {
	SCOPE_CYCLE_COUNTER( STAT_Constrain );

	TMap<EOrientations, FPatternIndexArray> NewValidData;

	{
		SCOPE_CYCLE_COUNTER( STAT_Constrain_Internal_One );

		for ( auto& Direction : UOrientations::OrientationUnitVectors ) {

			if ( Direction.Key == EOrientations::NONE ) continue;

			FIntVector NeighbourCoord = Coord + Direction.Value;

			if ( Wave.IsValidCoordinate( NeighbourCoord ) ) {
				for ( auto& AllowedPattern : Wave.GetDataAt( NeighbourCoord ).AllowedPatterns ) {
					if ( AllowedPattern.Value ) {
						FModulePropagator AllowedPatternByPropagator;
						{
							SCOPE_CYCLE_COUNTER( STAT_Constrain_Internal_One_One );

							AllowedPatternByPropagator = ModuleAssignee->Patterns.FindRef( AllowedPattern.Key ).Propagator.FindRef( Direction.Value * -1 );
						}

						if ( NewValidData.Contains( Direction.Key ) ) {
							FPatternIndexArray ValidHashSet;

							{
								SCOPE_CYCLE_COUNTER( STAT_Constrain_Internal_One_Two );

								ValidHashSet = NewValidData.FindRef( Direction.Key );

								for ( auto& Pattern : AllowedPatternByPropagator.Array ) {
									ValidHashSet.Array.AddUnique( Pattern );
								}


								NewValidData.Add( Direction.Key, ValidHashSet );
							}
						} else {
							FPatternIndexArray ValidHashSet;
							{
								SCOPE_CYCLE_COUNTER( STAT_Constrain_Internal_One_Three );
								ValidHashSet = FPatternIndexArray();

								for ( auto& Pattern : AllowedPatternByPropagator.Array ) {
									ValidHashSet.Array.AddUnique( Pattern );
								}

								NewValidData.Add( Direction.Key, ValidHashSet );
							}
						}
					}
				}
			}
		}
	}

	TMap<int32, int32> PatternCounts;

	{
		SCOPE_CYCLE_COUNTER( STAT_Constrain_Internal_Two );

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
	}

	TMap<int32, bool> NewAllowedPatterns = Wave.GetDataAt( Coord ).AllowedPatterns;
	TMap<int32, bool> TempNewAllowedPatterns = NewAllowedPatterns;

	int32 ChangedCount = 0;

	{
		SCOPE_CYCLE_COUNTER( STAT_Constrain_Internal_Three );

		for ( auto& PatternAllowed : NewAllowedPatterns ) {
			if ( PatternAllowed.Value ) {
				ChangedCount++;
			}

			TempNewAllowedPatterns.Add( PatternAllowed.Key, false );
		}
	}


	{
		SCOPE_CYCLE_COUNTER( STAT_Constrain_Internal_Four );

		NewAllowedPatterns = TempNewAllowedPatterns;

		for ( auto& PatternCount : PatternCounts ) {
			if ( PatternCount.Value == NewValidData.Num() - 1 ) {
				NewAllowedPatterns.Add( PatternCount.Key, true );
				ChangedCount--;
			}
		}
	}

	{
		SCOPE_CYCLE_COUNTER( STAT_Constrain_Internal_Five );

		for ( auto& Direction : UOrientations::OrientationUnitVectors ) {
			if ( Direction.Key == EOrientations::NONE ) continue;

			FIntVector NeighbourCoord = Coord + Direction.Value;

			if ( Wave.IsValidCoordinate( NeighbourCoord ) ) {
				if ( ChangedCount > 0 && !Updated.Contains( NeighbourCoord ) ) {
					Flag.Push( NeighbourCoord );
				}
			}
		}
	}

	{
		SCOPE_CYCLE_COUNTER( STAT_Constrain_Internal_Six );

		FCoefficient Coefficient = Wave.GetDataAt( Coord );
		Coefficient.AllowedPatterns = NewAllowedPatterns;
		Wave.AddData( Coord, Coefficient );

		if ( Wave.GetDataAt( Coord ).AllowedCount() == 1 ) {
			int32 PatternIndexSelected = Wave.GetDataAt( Coord ).LastAllowedPatternIndex();
			SpawnMod( Coord, PatternIndexSelected );
		}

		Updated.Add( Coord );
	}
}

void AWFC::SpawnMod( FIntVector Coord, int32 Selected ) {
	SCOPE_CYCLE_COUNTER( STAT_SpawnMod );

	if ( ModuleAssignee ) {
		if ( ModuleAssignee->Patterns.Contains( Selected ) ) {

			FModuleMatrix SelectedPattern = ModuleAssignee->Patterns.FindRef( Selected );

			for3( SelectedPattern.SizeX, SelectedPattern.SizeY, SelectedPattern.SizeZ,
				  {
					  const FIntVector CoordAndPatternCoord = Coord + FIntVector( X,Y,Z );
					  if ( Wave.IsValidCoordinate( CoordAndPatternCoord ) && !HasModule.Contains( CoordAndPatternCoord ) ) {
							const FModuleData SelectedPatternData = SelectedPattern.GetDataAt( FIntVector( X, Y, Z ) );

							UChildActorComponent* NewChild = UWaveFunctionLibrary::CreateModule( GetWorld(), SelectedPatternData, this, GetActorLocation() + ( FVector( Coord ) * 1000 ) + ( FVector( X,Y,Z ) * 1000 ) );
							Wave.WaveValues.Add( CoordAndPatternCoord, SelectedPatternData );
							Wave.WaveCycle.Add( CoordAndPatternCoord, Cycle );
							SpawnedComponents.Add( CoordAndPatternCoord, NewChild );
							HasModule.Add( CoordAndPatternCoord, true );

							Cycle++;
					  }
				  }
		} )
	}
}

bool AWFC::IsFullyCollapsed() {
	SCOPE_CYCLE_COUNTER( STAT_WhileNotDone );

	int32 AllowedCount = 0;

	for3( OutputSize.X, OutputSize.Y, OutputSize.Z,
		  {
				FCoefficient WaveDataAtCoord = Wave.GetDataAt( FIntVector( X, Y, Z ) );

				if ( WaveDataAtCoord.AllowedCount() == 0 ) {
					UE_LOG( LogTemp, Error, TEXT( "Invalid construction in WFC... Retrying!" ) );
					Tries++;

					if ( Tries > 10 ) {
						UE_LOG( LogTemp, Error, TEXT( "10 invalid constructions... Stopping retry!" ) );
						return true;
					}

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

	while ( !IsFullyCollapsed() ) {
		Observe( MinEntropyCoords() );
	}

	//Tries = 0;
}

int32 AWFC::GetWeightedPattern( TMap<int32, bool> InPatterns ) {
	SCOPE_CYCLE_COUNTER( STAT_WeightedPattern );

	TArray<int32> WeightedPatterns;

	for ( auto& Pair : InPatterns ) {
		if ( Pair.Value ) {
			const int32 WeightCount = ModuleAssignee->Weights.FindRef( Pair.Key );

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
	SCOPE_CYCLE_COUNTER( STAT_Entropy );

	float MinEntropy = 0;

	const FRandomStream RandomStream = FRandomStream( FMath::RandRange( 0.F, MAX_FLT ) );
	FIntVector RMinEntropyCoords = FIntVector();

	for3( OutputSize.X, OutputSize.Y, OutputSize.Z,
		  {
			  const FIntVector CurrentCoordinates = FIntVector( X, Y, Z );

			  if ( Wave.GetDataAt( CurrentCoordinates ).AllowedCount() > 1 ) {
				  const float Entropy = ShannonEntropy( CurrentCoordinates );
				  const float EntropyPlusNoise = Entropy - RandomStream.FRand();

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
	SCOPE_CYCLE_COUNTER( STAT_ShannonEntropy );

	int32 SumOfWeights = 0;
	int32 SumOfWeightsLogWeights = 0;

	for ( auto& Pair : Wave.GetDataAt( CurrentCoordinates ).AllowedPatterns ) {

		const int32 Weight = ModuleAssignee->Weights.FindRef( Pair.Key );

		SumOfWeights += Weight;
		SumOfWeightsLogWeights += Weight * static_cast< float >( FMath::Loge( Weight ) );
	}

	return FMath::Loge( SumOfWeights ) - ( SumOfWeightsLogWeights / SumOfWeights );
}
