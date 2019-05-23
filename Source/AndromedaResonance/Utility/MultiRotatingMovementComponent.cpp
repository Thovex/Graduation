// Copyright 2018 Kio All Rights Reserved.

#include "CoreMinimal.h"
#include "MultiRotatingMovementComponent.h"


void UMultiRotatingMovementComponent::TickComponent( float DeltaTime, enum ELevelTick TickType, FActorComponentTickFunction *ThisTickFunction )
{

	if ( UpdatedComponents.Num() <= 0 )
	{
		return;
	}

	for ( int32 i = 0; i < UpdatedComponents.Num(); i++ )
	{

		// Compute new rotation
		const FQuat OldRotation = UpdatedComponents[i]->GetComponentQuat();
		const FQuat DeltaRotation = ( RotationRate * DeltaTime ).Quaternion();
		const FQuat NewRotation = bRotationInLocalSpace ? ( OldRotation * DeltaRotation ) : ( DeltaRotation * OldRotation );

		// Compute new location
		FVector DeltaLocation = FVector::ZeroVector;
		if ( !PivotTranslation.IsZero() )
		{
			const FVector OldPivot = OldRotation.RotateVector( PivotTranslation );
			const FVector NewPivot = NewRotation.RotateVector( PivotTranslation );
			DeltaLocation = ( OldPivot - NewPivot ); // ConstrainDirectionToPlane() not necessary because it's done by MoveUpdatedComponent() below.
		}

		const bool bEnableCollision = false;

		FHitResult* HitResult = new FHitResult;

		const FVector NewDelta = ConstrainDirectionToPlane( DeltaLocation );
		UpdatedComponents[i]->MoveComponent( NewDelta, NewRotation, bEnableCollision, HitResult, MoveComponentFlags, ETeleportType::None );
	}

}
