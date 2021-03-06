﻿using UnityEngine;
using System.Collections;
using System;

public class UIPurchase : UIWindow 
{
	UILabel m_msgLabel;
	public delegate void OnPurchaseFunc();
	
	public OnPurchaseFunc OnPurchase; 
	
    public override void OnCreate()
    {
        base.OnCreate();
        AddChildComponentMouseClick("ConfirmBtn", OnConfirmClicked);
        AddChildComponentMouseClick("CancelBtn", OnCancelClicked);
		m_msgLabel = GetChildComponent<UILabel>("MessageLabel");
    }
    public override void OnShow()
    {
        base.OnShow();
        GameLogic.Singleton.HideHelp();
		m_msgLabel.text = string.Format("You have {0} coins now,\nThe item will take you 1 coin,\nAre you sure about the purchasing?", GlobalVars.Coins);
    }

    private void OnConfirmClicked()
    {
        HideWindow(delegate()
        {
            GameLogic.Singleton.ShowHelpAnim();
        });
		OnPurchase();
    }

    private void OnCancelClicked()
    {
        HideWindow(delegate()
        {
            GameLogic.Singleton.ShowHelpAnim();
        });
        if (GameLogic.Singleton.GetGameFlow() == TGameFlow.EGameState_End)
        {
            UIWindowManager.Singleton.GetUIWindow<UIGameEnd>().ShowWindow();
        }
    }
}
