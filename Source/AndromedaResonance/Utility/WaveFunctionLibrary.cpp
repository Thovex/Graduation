// Fill out your copyright notice in the Description page of Project Settings.


#include "WaveFunctionLibrary.h"
#include "Runtime/Core/Public/Math/NumericLimits.h"
#include "Data/ModuleAssignee.h"

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

		return ChildActor;
	}

	return nullptr;
}

TArray<UChildActorComponent*> UWaveFunctionLibrary::CreatePattern( UObject* WorldContextObject, AActor* ParentActor, AModuleAssignee* ModuleAssignee, int32 PatternIndex, FVector Location ) {
	TArray<UChildActorComponent*> Components;

	FModuleMatrix SelectedPatternMatrix = ModuleAssignee->Patterns.FindRef( PatternIndex );

	FIntVector Size = FIntVector( ForceInit );

	Size.X = SelectedPatternMatrix.SizeX;
	Size.Y = SelectedPatternMatrix.SizeY;
	Size.Z = SelectedPatternMatrix.SizeZ;

	for3( Size.X, Size.Y, Size.Z, {
		UChildActorComponent * NewModule = CreateModule( WorldContextObject, SelectedPatternMatrix.GetDataAt( FIntVector( X,Y,Z ) ), ParentActor, Location + FVector( X,Y,Z ) * 1000 );
		Components.Add( NewModule );
	})

	return  Components;
}

FModuleData UWaveFunctionLibrary::CreateModuleDataFromBit( FName Bit, AModuleAssignee * ModuleAssignee ) {
	if ( ModuleAssignee ) {
		TMap<TSubclassOf<AModule>, FName> Map = ModuleAssignee->AssignedNames;

		return FModuleData( Bit, Map );
	}

	return FModuleData();
}