// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Kismet/BlueprintFunctionLibrary.h"
#include "Data/ModuleData.h"
#include "Data/ModuleMatrix.h"
#include "WaveFunctionCollapse/Module.h"
#include "Runtime/CoreUObject/Public/UObject/NoExportTypes.h"
#include "Runtime/Engine/Classes/Engine/World.h"
#include "Data/WaveMatrix.h"
#include "WaveFunctionLibrary.generated.h"

UENUM( BlueprintType )
enum class ERotationQuery : uint8 {
	ERQ_Forward		UMETA( "0/360 Degrees" ),
	ERQ_Left		UMETA( "-90/270 Degrees" ),
	ERQ_Right		UMETA( "90/-270 Degrees" ),
	ERQ_Back		UMETA( "180/-180 Degrees" )
};

/**
 *
 */
UCLASS()
class ANDROMEDARESONANCE_API UWaveFunctionLibrary : public UBlueprintFunctionLibrary {
	GENERATED_BODY()

public:

	template <typename ObjClass>
	static FORCEINLINE ObjClass* LoadObjFromPath( const FName& Path ) {
		if ( Path == NAME_None ) return NULL;
		return Cast<ObjClass>( StaticLoadObject( ObjClass::StaticClass(), NULL, *Path.ToString() ) );
	}

	UFUNCTION( BlueprintCallable, Category = "WFC | HelpingLibrary", meta = ( WorldContext = "WorldContextObject" ) )
		static UChildActorComponent * CreateModule( UObject * WorldContextObject, FModuleData ModuleData, AActor * ParentActor, FVector Location );

	UFUNCTION( BlueprintCallable, Category = "WFC | HelpingLibrary", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray< UChildActorComponent*> CreatePatternIndex( UObject * WorldContextObject, AActor * ParentActor, AModuleAssignee * ModuleAssignee, int32 PatternIndex, FVector Location );

	UFUNCTION( BlueprintCallable, Category = "WFC | HelpingLibrary", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray< UChildActorComponent*> CreatePatternData( UObject * WorldContextObject, AActor * ParentActor, AModuleAssignee * ModuleAssignee, FModuleMatrix Pattern, FVector Location );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC | HelpingLibrary", meta = ( WorldContext = "WorldContextObject" ) )
		static FModuleData CreateModuleDataFromBit( FName Bit, AModuleAssignee * ModuleAssignee );

public:
	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Module", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesByEmptyQuery( bool& HasResults, int32 & ResultCount, const FWaveMatrix Wave, const bool EmptyQuery );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Module", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesByModuleClassQuery( bool& HasResults, int32 & ResultCount, const FWaveMatrix Wave, const TSubclassOf<AModule> ModuleQuery );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Module", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesByZRotationQuery( bool& HasResults, int32 & ResultCount, FWaveMatrix Wave, ERotationQuery ZRotation );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Module", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesByModuleIdQuery( bool& HasResults, int32 & ResultCount, const FWaveMatrix Wave, const FName ModuleIDQuery );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Coord", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesByXRowQuery( bool& HasResults, int32 & ResultCount, const FWaveMatrix Wave,
		const bool Equal, const bool BiggerThan, const bool SmallerThan, const int32 XRowQuery );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Coord", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesByYRowQuery( bool& HasResults, int32 & ResultCount, const FWaveMatrix Wave,
		const bool Equal, const bool BiggerThan, const bool SmallerThan, const int32 YRowQuery );
	
	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Coord", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesByZRowQuery( bool& HasResults, int32 & ResultCount, const FWaveMatrix Wave, 
		const bool Equal, const bool BiggerThan, const bool SmallerThan, const int32 ZRowQuery );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries", meta = ( WorldContext = "WorldContextObject" ) )
		static AActor* GetActorByCoordinate( bool& HasResults, TSubclassOf<AActor>& ResultClass, const FIntVector Coord, const TMap<FIntVector, UChildActorComponent*> SpawnedComponents );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Appending", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesAppendedUniqueTwo( bool& HasResults, int32 & ResultCount,
															  const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Validations", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesValidatedTwo( bool& HasResults, int32& ResultCount, 
		const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Validations", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesValidatedThree( bool& HasResults, int32 & ResultCount, 
		const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo, 
		const TArray<FIntVector> CoordinateSetThree );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Validations", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesValidatedFour( bool& HasResults, int32 & ResultCount, 
		const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo, 
		const TArray<FIntVector> CoordinateSetThree, const TArray<FIntVector> CoordinateSetFour );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Validations", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesValidatedFive( bool& HasResults, int32 & ResultCount, 
		const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo, 
		const TArray<FIntVector> CoordinateSetThree, const TArray<FIntVector> CoordinateSetFour,
		const TArray<FIntVector> CoordinateSetFive );

	UFUNCTION( BlueprintCallable, BlueprintPure, Category = "WFC|Queries|Validations", meta = ( WorldContext = "WorldContextObject" ) )
		static TArray<FIntVector> GetCoordinatesValidatedSix( bool& HasResults, int32 & ResultCount, 
		const TArray<FIntVector> CoordinateSetOne, const TArray<FIntVector> CoordinateSetTwo, 
		const TArray<FIntVector> CoordinateSetThree, const TArray<FIntVector> CoordinateSetFour,
		const TArray<FIntVector> CoordinateSetFive, const TArray<FIntVector> CoordinateSetSix );

	static void QuerySetReferenceValues( bool& HasResults, int32& ResultCount, const TArray<FIntVector> Coordinates );

};
