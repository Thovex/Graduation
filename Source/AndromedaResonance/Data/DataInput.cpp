// Fill out your copyright notice in the Description page of Project Settings.


#include "DataInput.h"
#include "Runtime/Engine/Public/DrawDebugHelpers.h"
#include "Utility/DrawDebugText.h"
#include "Utility/WaveFunctionLibrary.h"
#include "Utility/UtilityLibrary.h"
#include "WaveFunctionCollapse/Module.h"
#include "Data/ModuleAssignee.h"

// Sets default values
ADataInput::ADataInput( const FObjectInitializer& ObjectInitializer ) {
	Transform = ObjectInitializer.CreateDefaultSubobject<USceneComponent>( this, TEXT( "Transform" ) );
	RootComponent = Transform;

	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ADataInput::BeginPlay() {
	Super::BeginPlay();

}

// Called every frame
void ADataInput::Tick( float DeltaTime ) {
	Super::Tick( DeltaTime );

	if ( !SelectedCheck() ) return;

	Errors.Empty();

	Patterns.Empty();
	ModulesMap.Empty();

	ValidateInput();

	//Training();

	PatternStatus = Errors.Num() == 0 ? FColor::Green : FColor::Red;

	const UWorld* World = GetWorld();

	if ( World ) {
		for ( auto& ErrorPair : Errors ) {
			DrawDebugLine( World, GetActorLocation() + ( FVector::UpVector * GridElementSize ), ErrorPair.Key->GetActorLocation(), FColor::Red, false, -1.F, 0, 10.F );
			DrawDebugBox( World, ErrorPair.Key->GetActorLocation(), ErrorPair.Key->GetActorScale3D() * UUtilityLibrary::Divide_VectorByIntVector( FVector( GridElementSize ), InputSize ), FColor::Red, false, -1.F, 0, 10.F );
		}

		DrawDebugBox( World, GetActorLocation() + FVector( InputSize * GridElementSize / 2 ) - FVector( 500 ), FVector( InputSize * GridElementSize / 2 ), PatternStatus, false, -1.F, 0, PatternStatus == FColor::Green ? 50.F : Errors.Num() * 150.F );
	}

}

void ADataInput::ValidateLocalLocation( AActor * &Child, const FIntVector LocalLocationInt, FIntVector & Coord ) {
	if ( LocalLocationInt.X % GridElementSize != 0 ) {
		Errors.Add( Child, TEXT( "Invalid X Location" ) );
	} else {
		Coord.X = LocalLocationInt.X / GridElementSize;
	}

	if ( LocalLocationInt.Y % GridElementSize != 0 ) {
		Errors.Add( Child, TEXT( "Invalid Y Location" ) );
	} else {
		Coord.Y = LocalLocationInt.Y / GridElementSize;
	}

	if ( LocalLocationInt.Z % GridElementSize != 0 ) {
		Errors.Add( Child, TEXT( "Invalid Z Location" ) );
	} else {
		Coord.Z = LocalLocationInt.Z / GridElementSize;
	}
}

void ADataInput::ValidateLocalRotation( AActor * &Child, const FIntVector LocalRotationInt ) {
	if ( LocalRotationInt.Z % 90 != 0 ) {
		Errors.Add( Child, TEXT( "Invalid Z Rotation" ) );
	}

	if ( LocalRotationInt.X != 0 || LocalRotationInt.Y != 0 ) {
		Errors.Add( Child, TEXT( "Invalid X or Y Rotation. Must be 0" ) );
	}
}

void ADataInput::ValidateCoordCount( AActor * &Child, FIntVector Coord ) {
	if ( PatternLocations.Contains( Coord ) ) {
		const int32 Count = PatternLocations.FindRef( Coord ) + 1;
		PatternLocations.Add( Coord, Count );

		if ( Count > 1 ) {
			Errors.Add( Child, TEXT( "Multiple Modules on One Location" ) );
		}
	}
}

void ADataInput::DrawCoord( AActor * &Child, FIntVector Coord, FString & DisplayText ) const {
	DisplayText = FString::Printf( TEXT( "(%d, %d, %d)" ), Coord.X, Coord.Y, Coord.Z );

	DrawDebugText( GetWorld(), DisplayText, Child->GetActorLocation(), FColor::White, false, .1F, 1, 8, 2, true, false, 0, false );
	DrawDebugText( GetWorld(), TEXT( " X, Y, Z " ), Child->GetActorLocation() + ( FVector::UpVector * 150 ), FColor::White, false, .1F, 1, 10, 2, true, false, 0, false );
}

void ADataInput::DrawNameAndBit( AActor * &Child, const FString DisplayText, FModuleData Module ) const {
	const FString BitString = *Module.GenerateBit().ToString();

	const FString ChildName = FString::Printf( TEXT( "%s | %s | %s" ), *DisplayText, *BitString, *Child->GetClass()->GetFName().ToString() );

	if ( Child->GetActorLabel() != ChildName ) {
		Child->SetActorLabel( ChildName );
	}

	DrawDebugText( GetWorld(), BitString, Child->GetActorLocation() + ( FVector::UpVector * 300 ), FColor::White, false, .1F, 1, 10, 2, true, false, 0, false );
}

void ADataInput::ValidateInput() {
	TArray<AActor*> AttachedActors;
	GetAttachedActors( AttachedActors );

	for ( auto& Child : AttachedActors ) {
		const FIntVector LocalLocationInt = UUtilityLibrary::Conv_VectorToIntVector( Child->GetActorLocation() - GetActorLocation() );
		const FIntVector LocalRotationInt = UUtilityLibrary::Conv_RotatorToIntVector( Child->GetActorRotation() );

		FIntVector Coord = FIntVector( -1 );

		ValidateLocalLocation( Child, LocalLocationInt, Coord );
		ValidateLocalRotation( Child, LocalRotationInt );
		ValidateCoordCount( Child, Coord );

		FString DisplayText;
		DrawCoord( Child, Coord, DisplayText );

		if ( !ModuleAssignee ) return;

		AModule * ModuleChild = Cast<AModule>( Child );

		if ( !ModuleAssignee->AssignedNames.Contains( ModuleChild->GetClass() ) ) {
			ModuleAssignee->Training();
		}

		FModuleData Module = FModuleData( ModuleChild, ModuleAssignee->AssignedNames.FindRef( ModuleChild->GetClass() ) );
		DrawNameAndBit( Child, DisplayText, Module );

		if ( Errors.Num() == 0 ) {
			ModulesMap.Add( Coord, Module );
		}
	}

	if ( !ModuleAssignee ) return;

	for ( int32 X = 0; X < InputSize.X; X++ ) {
		for ( int32 Y = 0; Y < InputSize.Y; Y++ ) {
			for ( int32 Z = 0; Z < InputSize.Z; Z++ ) {
				if ( !ModulesMap.Contains( FIntVector( X, Y, Z ) ) ) {
					ModulesMap.Add( FIntVector( X, Y, Z ), FModuleData( true ) );
				}
			}
		}
	}
}

void ADataInput::Training() {
	if ( Errors.Num() == 0 ) {
		SetMatrix( InputSize );
		DefinePatterns();
	}
}

void ADataInput::SetMatrix( FIntVector Size ) {

	FModuleMatrix NewModuleMatrix = FModuleMatrix( Size );
	NewModuleMatrix.Initialize( ModulesMap );
	ModuleData = NewModuleMatrix;
}

FModuleMatrix ADataInput::DefineNewPatternData( int32 InitX, int32 InitY, int32 InitZ ) {
	FModuleMatrix NewPatternData = FModuleMatrix( NSize );

	for3( NSize.X, NSize.Y, NSize.Z,
		  {
			  FIntVector Coord = FIntVector( InitX + X, InitY + Y, InitZ + Z );

			  if ( ModuleData.IsValidCoordinate( Coord ) ) {
				  NewPatternData.SetDataAt( FIntVector( X, Y ,Z ), ModuleData.GetDataAt( Coord ) );
			  }
		  }
	)

		return NewPatternData;
}

void ADataInput::DefinePatterns() {
	int32 CurrentPattern = 0;

	for3( InputSize.X, InputSize.Y, InputSize.Z,
		  {
			  FModuleMatrix NewPattern = DefineNewPatternData( X, Y, Z );
			  FModuleMatrix & PatternRef = NewPattern;

			  for ( int32 i = 0; i < 4; i++ ) {
					FModuleMatrix Pattern = PatternRef;
					Pattern.RotateCounterClockwise( i );
					Patterns.Add( Pattern );
			   }
		  }
	)
}

bool ADataInput::SelectedCheck() {

	if ( Errors.Num() > 0 ) return true;
	if ( IsSelectedInEditor() ) return true;

	TArray<AActor*> AttachedActors;
	GetAttachedActors( AttachedActors );

	for ( AActor* ChildActor : AttachedActors ) {
		if ( ChildActor->IsSelectedInEditor() ) return true;
	}

	return false;
}
