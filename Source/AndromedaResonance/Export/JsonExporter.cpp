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

FString AJsonExporter::RetrieveDataFileName( FString FileName ) {
	
	FString LoadDirectory = FPaths::ProjectDir() + FString( "WFC_Data/" );
	FString FileData = "";

	FFileHelper::LoadFileToString( FileData, *FString( FPaths::ProjectDir() + FString( "WFC_Data/" ) + FileName ) );

	return FileData;

	
}

FString AJsonExporter::RetrieveDataRandom() {
	return FString();
}

void AJsonExporter::RetrieveData() {

}


