#include "ThovexModule.h"
#include "AndromedaResonance/Data/DataGrid.h"
#include "CustomDetailsPanel.h"
#include "PropertyEditorModule.h"
 
DEFINE_LOG_CATEGORY(ThovexModule);
 
#define LOCTEXT_NAMESPACE "FThovexModule"
 
void FThovexModule::StartupModule()
{
	UE_LOG(ThovexModule, Warning, TEXT("Thovex module has started!"));

	//Get the property module
	FPropertyEditorModule& PropertyModule = FModuleManager::LoadModuleChecked<FPropertyEditorModule>("PropertyEditor");
	//Register the custom details panel we have created
	PropertyModule.RegisterCustomClassLayout(ADataGrid::StaticClass()->GetFName(), FOnGetDetailCustomizationInstance::CreateStatic(&FCustomDetailsPanel::MakeInstance));
}
 
void FThovexModule::ShutdownModule()
{
	UE_LOG(ThovexModule, Warning, TEXT("Thovex module has shut down"));
}
 
#undef LOCTEXT_NAMESPACE
 
IMPLEMENT_MODULE(FThovexModule,ThovexModule)