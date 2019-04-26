// Fill out your copyright notice in the Description page of Project Settings.


#include "UtilityLibrary.h"

FIntVector UUtilityLibrary::Conv_VectorToIntVector( const FVector& InVector ) {
	return FIntVector( InVector );
}

float UUtilityLibrary::SetScale( float CurrentValue, float OldMinScale, float OldMaxScale, float NewMinScale, float NewMaxScale ) {

	return ( ( ( CurrentValue - OldMinScale ) * ( NewMaxScale - NewMinScale ) ) / ( OldMaxScale - OldMinScale ) ) + NewMinScale;
}

float UUtilityLibrary::SetScalePure( float CurrentValue, float OldMinScale, float OldMaxScale, float NewMinScale, float NewMaxScale ) {

	return SetScale( CurrentValue, OldMinScale, OldMaxScale, NewMinScale, NewMaxScale );
}
