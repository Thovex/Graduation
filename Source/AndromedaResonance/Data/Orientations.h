// Fuck vibe

#pragma once

#include "CoreMinimal.h"
#include "Orientations.generated.h"

UENUM( BlueprintType )
enum class EOrientations : uint8 {
	FORWARD                UMETA( DisplayName = "Forward (X)" ),
	BACK                   UMETA( DisplayName = "Back (-X)" ),

	RIGHT                  UMETA( DisplayName = "Right (Y)" ),
	LEFT                   UMETA( DisplayName = "Left (-Y)" ),

	UP                     UMETA( DisplayName = "Up (Z)" ),
	DOWN                   UMETA( DisplayName = "Down (-Z)" ),

	RIGHT_UP               UMETA( DisplayName = "Right Up (YZ)" ),
	RIGHT_DOWN             UMETA( DisplayName = "Right Down (Y-Z)" ),

	LEFT_UP                UMETA( DisplayName = "Left Up (-YZ" ),
	LEFT_DOWN              UMETA( DisplayName = "Left Down (-Y-Z)" ),

	FORWARD_RIGHT          UMETA( DisplayName = "Forward Right (XY)" ),
	FORWARD_LEFT           UMETA( DisplayName = "Forward Left (X-Y)" ),

	FORWARD_UP             UMETA( DisplayName = "Forward Up (XZ)" ),
	FORWARD_DOWN           UMETA( DisplayName = "Forward Down (X-Z)" ),

	BACK_UP                UMETA( DisplayName = "Forward (-XZ" ),
	BACK_DOWN              UMETA( DisplayName = "Back Down (-X-Z)" ),

	FORWARD_RIGHT_UP       UMETA( DisplayName = "Forward Right Up (XYZ)" ),
	FORWARD_RIGHT_DOWN     UMETA( DisplayName = "Forward Right Down (XY-Z)" ),

	FORWARD_LEFT_UP        UMETA( DisplayName = "Forward Left Up (X-YZ)" ),
	FORWARD_LEFT_DOWN      UMETA( DisplayName = "Forward Left Down (X-Y-Z)" ),

	BACK_RIGHT             UMETA( DisplayName = "Back Right (-XY)" ),
	BACK_LEFT              UMETA( DisplayName = "Back Left (-X-Y)" ),

	BACK_RIGHT_UP          UMETA( DisplayName = "Back Right Up (-XYZ)" ),
	BACK_RIGHT_DOWN        UMETA( DisplayName = "Back Right Down (-XY-Z)" ),

	BACK_LEFT_UP           UMETA( DisplayName = "Back Left Up (-X-YZ)" ),
	BACK_LEFT_DOWN         UMETA( DisplayName = "Back Left Down (-X-Y-Z)" ),

	NONE                   UMETA( DisplayName = "None" ),
};

UCLASS()
class UOrientations : public UObject {
	GENERATED_BODY()

public:
	static const TMap<EOrientations, FIntVector> OrientationEulers;
	static const TMap<FName, EOrientations> OrientationByFName;
	static const TMap<EOrientations, FIntVector> OrientationUnitVectors;

public:
	static EOrientations EulerToOrientation( FIntVector RotationVector );

};