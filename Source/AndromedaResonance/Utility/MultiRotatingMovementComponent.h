// Copyright 2018 Kio All Rights Reserved.
// Class UMultiRotatingMovementComponent created by Jesse van Vliet

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/RotatingMovementComponent.h"
#include "MultiRotatingMovementComponent.generated.h"

/**
 *
 */
UCLASS( ClassGroup = Movement, meta = ( BlueprintSpawnableComponent ), HideCategories = ( Velocity ) )
class ANDROMEDARESONANCE_API UMultiRotatingMovementComponent : public URotatingMovementComponent
{
	GENERATED_BODY()

public:

	UPROPERTY( EditAnywhere, BlueprintReadWrite )
		TArray <USceneComponent*> UpdatedComponents;

private:

	virtual void TickComponent( float DeltaTime, enum ELevelTick TickType, FActorComponentTickFunction *ThisTickFunction ) override;



};