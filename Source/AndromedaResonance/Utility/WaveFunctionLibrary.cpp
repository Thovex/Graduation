// Fill out your copyright notice in the Description page of Project Settings.


#include "WaveFunctionLibrary.h"
#include "Runtime/Core/Public/Math/NumericLimits.h"
#include "Runtime/Engine/Classes/Components/TextRenderComponent.h"
#include "Runtime/Engine/Classes/Materials/MaterialInstanceDynamic.h"
#include "Data/ModuleAssignee.h"
#include "ConstructorHelpers.h"
#include "UObjectGlobals.h"


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