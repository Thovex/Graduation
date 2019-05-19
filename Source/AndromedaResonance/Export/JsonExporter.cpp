// Fill out your copyright notice in the Description page of Project Settings.


#include "JsonExporter.h"
#include "Runtime/JsonUtilities/Public/JsonObjectConverter.h"
#include "Runtime/Core/Public/Misc/FileHelper.h"
#include "Runtime/Core/Public/Misc/DateTime.h"
#include "Runtime/Core/Public/GenericPlatform/GenericPlatformFile.h"
#include "Macros.h"
#include "PlatformFilemanager.h"
#include "Paths.h"

AJsonExporter::AJsonExporter() {

}

void AJsonExporter::ParseData( FWaveMatrix Wave, AModuleAssignee* ModuleAssignee ) {
	if ( !ModuleAssignee ) return;

	FWFCJsonStruct JsonStruct = FWFCJsonStruct();

	for3( Wave.SizeX, Wave.SizeY, Wave.SizeZ, {
		FCoefficient Coefficient = Wave.GetDataAt( FIntVector( X,Y,Z ) );

		FWFCJsonModuleCoordPattern NewCoordMap = FWFCJsonModuleCoordPattern();
		NewCoordMap.Coord = FIntVector( X,Y,Z );
		NewCoordMap.Value = Coefficient.LastAllowedPatternIndex();

		JsonStruct.Data.Add( NewCoordMap );
		  } )

		FString JsonString;

		  FJsonObjectConverter::UStructToJsonObjectString( FWFCJsonStruct::StaticStruct(), &JsonStruct, JsonString, 0, 0 );

		  FString SaveDirectory = FPaths::ProjectDir() + FString( "WFC_Data/" );

		  FString FileName = FString( "WFC_" + FString::FromInt( Wave.SizeX ) + "x" + FString::FromInt( Wave.SizeY ) + "x" + FString::FromInt( Wave.SizeZ ) + "_" + FDateTime::Now().ToString() + ".json" );
		  FString TextToSave = JsonString;

		  bool AllowOverwriting = false;

		  IPlatformFile & PlatformFile = FPlatformFileManager::Get().GetPlatformFile();

		  if ( PlatformFile.CreateDirectoryTree( *SaveDirectory ) ) {
			  FString AbsoluteFilePath = SaveDirectory + "/" + FileName;

			  if ( AllowOverwriting || !PlatformFile.FileExists( *AbsoluteFilePath ) ) {
				  FFileHelper::SaveStringToFile( TextToSave, *AbsoluteFilePath );
			  }
		  }
}

FWaveMatrix AJsonExporter::RetrieveDataFileName( FString FileName ) {

	const FString LoadDirectory = FPaths::ProjectDir() + FString( "WFC_Data/" );
	FString FileData = "";

	FFileHelper::LoadFileToString( FileData, *FString( FPaths::ProjectDir() + FString( "WFC_Data/" ) + FileName ) );

	FWFCJsonStruct JsonStruct = FWFCJsonStruct();

	FJsonObjectConverter::JsonObjectStringToUStruct( FileData, &JsonStruct, 0, 0 );

	FWaveMatrix Wave = FWaveMatrix();

	for ( auto& Elem : JsonStruct.Data ) {
		TMap<int32, bool> AllowedPatterns;
		AllowedPatterns.Add( Elem.Value, true );

		Wave.AddData( Elem.Coord, FCoefficient( AllowedPatterns ) );
	}

	Wave.SetSize( Wave.CalculateSize() );

	return Wave;

}

FWaveMatrix AJsonExporter::RetrieveDataRandom( FIntVector Size, FString & OutFileName ) {
	const FString LoadDirectory = FPaths::ProjectDir() + FString( "WFC_Data/" );
	FString FileData = "";

	TArray<FString> FileNames;

	IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();
	PlatformFile.FindFiles( FileNames, *LoadDirectory, NULL );

	FString FileName = "";

	if ( Size.X > 0 || Size.Y > 0 || Size.Z > 0 ) {
		TArray<FString> FileNamesMatchingSize;

		for ( auto& LocalFileName : FileNames ) {
			if ( LocalFileName.Contains( FString::FromInt( Size.X ) + "x" + FString::FromInt( Size.Y ) + "X" + FString::FromInt( Size.Z ) ) ) {
				FileNamesMatchingSize.Add( LocalFileName );
			}
		}

		if ( FileNamesMatchingSize.Num() > 0 ) {
			FileName = FileNamesMatchingSize[FMath::RandRange( 0, FileNamesMatchingSize.Num() - 1 )];
		} else {
			UE_LOG( LogTemp, Error, TEXT( "No json found with size: %s" ), *Size.ToString() );
		}
	} else {
		FileName = FileNames[FMath::RandRange( 0, FileNames.Num() - 1 )];
	}

	OutFileName = FileName;
	FFileHelper::LoadFileToString( FileData, *FString( FileName ) );

	FWFCJsonStruct JsonStruct = FWFCJsonStruct();

	FJsonObjectConverter::JsonObjectStringToUStruct( FileData, &JsonStruct, 0, 0 );

	FWaveMatrix Wave = FWaveMatrix();

	for ( auto& Elem : JsonStruct.Data ) {
		TMap<int32, bool> AllowedPatterns;
		AllowedPatterns.Add( Elem.Value, true );

		Wave.AddData( Elem.Coord, FCoefficient( AllowedPatterns ) );
	}

	Wave.SetSize( Wave.CalculateSize() );

	return Wave;
}
