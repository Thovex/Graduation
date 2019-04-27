// Fill out your copyright notice in the Description page of Project Settings.


#include "UtilityLibrary.h"
#include "Utility/DrawDebugText.h"
#include "Runtime/CoreUObject/Public/UObject/NoExportTypes.h"


FIntVector UUtilityLibrary::Conv_VectorToIntVector( const FVector& InVector ) {
	return FIntVector( InVector );
}

float UUtilityLibrary::SetScale( float CurrentValue, float OldMinScale, float OldMaxScale, float NewMinScale, float NewMaxScale ) {

	return ( ( ( CurrentValue - OldMinScale ) * ( NewMaxScale - NewMinScale ) ) / ( OldMaxScale - OldMinScale ) ) + NewMinScale;
}

float UUtilityLibrary::SetScalePure( float CurrentValue, float OldMinScale, float OldMaxScale, float NewMinScale, float NewMaxScale ) {

	return SetScale( CurrentValue, OldMinScale, OldMaxScale, NewMinScale, NewMaxScale );
}

void UUtilityLibrary::DebugText( const UWorld * WorldContextObject, FString Text, FVector const& Location, FColor const& Color, bool bPersistentLines, float LifeTime, uint8 DepthPriority, float Thickness, float Size, bool bRotateToCamera, bool bHasMaxDistance, int32 MaxDistance, bool bCapitalize ) {
	if ( WorldContextObject ) {
		DrawDebugText( WorldContextObject, Text, Location, Color, bPersistentLines, LifeTime, DepthPriority, Thickness, Size, bRotateToCamera, bHasMaxDistance, MaxDistance, bCapitalize );
	}
}
