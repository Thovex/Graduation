// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Engine/GameViewportClient.h"
#include "AndromedaViewport.generated.h"

/**
 * 
 */
UCLASS()
class ANDROMEDARESONANCE_API UAndromedaViewport : public UGameViewportClient
{
	GENERATED_BODY()
public:

	/**
	 * Called after rendering the player views and HUDs to render menus, the console, etc.
	 * This is the last rendering call in the render loop
	 *
	 * @param Canvas        The canvas to use for rendering.
	 */
	virtual void PostRender( UCanvas* Canvas ) override;
	/** Clear fading state */
	virtual void ClearFade();
	/** Used for Fade to and from black */

	UFUNCTION(BlueprintCallable, BlueprintPure, Category = "Viewport")
		static UAndromedaViewport* GetViewport( AActor* WorldActor );

	UFUNCTION(BlueprintCallable, Category = "Viewport")
		void Fade( const float Duration, const bool bToBlack );

	/** Does the actual screen fading */
	void DrawScreenFade( UCanvas* Canvas );
private:

	// Values used by our screen fading
	uint32 bFading : 1;
	uint32 bToBlack : 1; // Fading to black will be applied even if alpha is 1
	float FadeAlpha;
	float FadeStartTime;
	float FadeDuration;


};