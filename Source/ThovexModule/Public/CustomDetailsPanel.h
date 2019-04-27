// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Input/Reply.h"
#include "IDetailCustomization.h"

class FCustomDetailsPanel : public IDetailCustomization
{

private:

	/* Contains references to all selected objects inside in the viewport */
	TArray<TWeakObjectPtr<UObject>> SelectedObjects;

public:

	static TSharedRef<IDetailCustomization> MakeInstance();

	virtual void CustomizeDetails(IDetailLayoutBuilder& DetailBuilder) override;

	FReply ClickedOnButton();

};