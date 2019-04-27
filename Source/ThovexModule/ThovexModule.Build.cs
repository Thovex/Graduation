using UnrealBuildTool;
 
public class ThovexModule : ModuleRules
{
	public ThovexModule(ReadOnlyTargetRules Target) : base(Target)
	{
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
 
		//Public module names that this module uses.
        PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "PropertyEditor", "Slate", "SlateCore", "AndromedaResonance"});

        //The path for the header files
        PublicIncludePaths.AddRange(new string[] {"ThovexModule/Public"});
 
		//The path for the source files
		PrivateIncludePaths.AddRange(new string[] {"ThovexModule/Private"});


	}
}