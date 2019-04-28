// Fill out your copyright notice in the Description page of Project Settings.


#include "UtilityLibrary.h"
#include "Utility/DrawDebugText.h"
#include "Runtime/CoreUObject/Public/UObject/NoExportTypes.h"


FIntVector UUtilityLibrary::Conv_VectorToIntVector( const FVector& InVector ) {
	FIntVector ConvertedVector;

	ConvertedVector.X = FMath::RoundToInt( InVector.X );
	ConvertedVector.Y = FMath::RoundToInt( InVector.Y );
	ConvertedVector.Z = FMath::RoundToInt( InVector.Z );

	return ConvertedVector;
}

FIntVector UUtilityLibrary::Conv_RotatorToIntVector( const FRotator& InRotator ) {
	FIntVector ConvertedVector;

	ConvertedVector.X = FMath::RoundToInt( InRotator.Roll );
	ConvertedVector.Y = FMath::RoundToInt( InRotator.Pitch );
	ConvertedVector.Z = FMath::RoundToInt( InRotator.Yaw );

	return ConvertedVector;

}

FVector UUtilityLibrary::Vector_AddIntVector( const FVector& InVector, const FIntVector& InIntVector ) {
	FVector AddVector = InVector;

	AddVector.X += InIntVector.X;
	AddVector.Y += InIntVector.Y;
	AddVector.Z += InIntVector.Z;

	return AddVector;
}

float UUtilityLibrary::SetScale( float CurrentValue, float OldMinScale, float OldMaxScale, float NewMinScale, float NewMaxScale ) {

	return ( ( ( CurrentValue - OldMinScale ) * ( NewMaxScale - NewMinScale ) ) / ( OldMaxScale - OldMinScale ) ) + NewMinScale;
}

float UUtilityLibrary::SetScalePure( float CurrentValue, float OldMinScale, float OldMaxScale, float NewMinScale, float NewMaxScale ) {

	return SetScale( CurrentValue, OldMinScale, OldMaxScale, NewMinScale, NewMaxScale );
}

