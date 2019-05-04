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

FRotator UUtilityLibrary::Conv_IntVectorToRotator( const FIntVector& InIntVector ) {
	FRotator ConvertedRotator;

	ConvertedRotator.Roll = InIntVector.X;
	ConvertedRotator.Pitch = InIntVector.Y;
	ConvertedRotator.Yaw = InIntVector.Z;

	return ConvertedRotator;

}

FVector UUtilityLibrary::Divide_VectorByIntVector( const FVector& InVector, const FIntVector& InIntVector ) {
	FVector DividedVector = InVector;

	DividedVector.X = InVector.X / InIntVector.X;
	DividedVector.Y = InVector.Y / InIntVector.Y;
	DividedVector.Z = InVector.Z / InIntVector.Z;

	return DividedVector;
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

