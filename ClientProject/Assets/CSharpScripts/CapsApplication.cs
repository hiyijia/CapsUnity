﻿using UnityEngine;
using System.Collections;
using System;
using System.Threading;

public enum StateEnum
{
    Login,
    Game,
}

enum LanguageType
{
	English,
	Chinese,
}

public enum UIDrawPrefab
{
    DefaultLabel,
    OutlineTextLabel,
    ShadowTextLabel,
    BaseSpriteCommonAtlas,
    BaseNumber,
}

public class CapsApplication : S5Application
{
    public StateEnum CurStateEnum { get; set; }
    #region Singleton
    public static CapsApplication Singleton { get; private set; }
    public CapsApplication()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            throw new System.Exception();			//if singleton exist, throw a Exception
        }
    }
    #endregion
    public bool HasSeenSplash { get; set; }

    float m_startAppTime;                           //开始app的时间
    float m_playTime;

    protected override void DoInit()
    {
        //根据平台开关数据分析
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            CapsConfig.EnableGA = true;
            //CapsConfig.EnableTalkingData = true;
        }
        else
        {
            CapsConfig.EnableGA = false;
            CapsConfig.EnableTalkingData = false;
        }
		if (CapsConfig.EnableTalkingData)
        {
            TalkingDataPlugin.SessionStarted("8F604653A8CC694E6954B51FE6D26127", "Test");
        }
        m_startAppTime = Time.realtimeSinceStartup;
        m_playTime = PlayerPrefs.GetFloat("PlayTime");

        if (!PlayerPrefs.HasKey("Music"))        //第一次运行
        {
            PlayerPrefs.SetInt("Music", 1);
            PlayerPrefs.SetInt("SFX", 1);
            GlobalVars.UseMusic = true;
            GlobalVars.UseSFX = true;
        }
        else
        {
            GlobalVars.UseMusic = (PlayerPrefs.GetInt("Music") == 1);
            GlobalVars.UseSFX = (PlayerPrefs.GetInt("SFX") == 1);
        }

		Application.targetFrameRate = 1000;			//
		
        new CapsConfig();
        new ResourceManager();

        UIDrawer.Singleton.AddPrefab(ResourceManager.Singleton.GetUIPrefabByName("BaseTextLabel"));
        UIDrawer.Singleton.fontDefaultPrefabID = UIDrawer.Singleton.AddPrefab(ResourceManager.Singleton.GetUIPrefabByName("OutlineTextLabel"));
        UIDrawer.Singleton.AddPrefab(ResourceManager.Singleton.GetUIPrefabByName("ShadowTextLabel"));
        UIDrawer.Singleton.spriteDefaultPrefabID = UIDrawer.Singleton.AddPrefab(ResourceManager.Singleton.GetUIPrefabByName("BaseSpriteCommonAtlas"));
        UIDrawer.Singleton.numDefaultPrefabID = UIDrawer.Singleton.AddPrefab(ResourceManager.Singleton.GetUIPrefabByName("BaseNumber"));

        TextTable.Singleton.AddTextMap(@"baseText");
        TextTable.Singleton.AddTextMap(@"errorcode");

        ChangeState((int)StateEnum.Login);

        GlobalVars.TotalStageCount = CapsConfig.Instance.TotalStageCount;
        if (CapsConfig.Instance.MoveTime > 0)
        {
            GameLogic.MOVE_TIME = CapsConfig.Instance.MoveTime;
        }
        if (CapsConfig.Instance.EatTime > 0)
        {
            GameLogic.EATBLOCK_TIME = CapsConfig.Instance.EatTime;
        }
        if (CapsConfig.Instance.DropAcc > 0)
        {
            GameLogic.DROP_ACC = CapsConfig.Instance.DropAcc;
        }
        if (CapsConfig.Instance.DropSpeed > 0)
        {
            GameLogic.DROP_SPEED = CapsConfig.Instance.DropSpeed;
        }
        if (CapsConfig.Instance.DropSpeed > 0)
        {
            GameLogic.SLIDE_SPEED = CapsConfig.Instance.SlideSpeed;
        }

        //读取心数相关
        if (PlayerPrefs.HasKey("HeartCount"))
        {
            GlobalVars.HeartCount = PlayerPrefs.GetInt("HeartCount");
            string heartTimeString = PlayerPrefs.GetString("GetHeartTime");
            GlobalVars.GetHeartTime = Convert.ToDateTime(heartTimeString);
        }

        UIWindowManager.Singleton.CreateWindow<UILogin>().ShowWindow();
        UIWindowManager.Singleton.GetUIWindow<UILogin>().ShowWindow(delegate()
        {
            GameObject obj = GameObject.Find("FirstTimeBackground");           //为了让第一次进游戏的图平滑变化没有闪烁，先在场景里垫了一张图，现在用完了，把图删除
            GameObject.Destroy(obj);

        });
    }

    protected override void DoUpdate()
    {
        base.DoUpdate();

		if (GlobalVars.HeartCount < 5)          //若心没有满，处理心数量变化
		{
			int ticks = (int)((System.DateTime.Now.Ticks - GlobalVars.GetHeartTime.Ticks) / 10000);
			int GetHeartCount = 0;
			if (ticks > CapsConfig.Instance.GetHeartInterval * 1000)        //若已经到了得心时间
			{
				GetHeartCount = (ticks / (CapsConfig.Instance.GetHeartInterval * 1000));
				GlobalVars.HeartCount += (int)GetHeartCount;                     //增加心数
				GlobalVars.GetHeartTime = GlobalVars.GetHeartTime.AddSeconds(GetHeartCount * CapsConfig.Instance.GetHeartInterval);          //更改获取心的时间记录
			}
			
			if (GlobalVars.HeartCount > 5)
			{
				GlobalVars.HeartCount = 5;
			}
		}
    }

    public override void OnApplicationPause(bool bPause)
    {
        base.OnApplicationPause(bPause);
        if (bPause)
        {
            PlayerPrefs.SetFloat("PlayTime", GetPlayTime());        //暂停时保存游戏时间
            if (CapsConfig.EnableTalkingData)
                TalkingDataPlugin.SessionStoped();
        }
        else
        {
            m_playTime = PlayerPrefs.GetFloat("PlayTime");          //恢复时读取游戏时间
            m_startAppTime = Time.realtimeSinceStartup;
			if (CapsConfig.EnableTalkingData)
                TalkingDataPlugin.SessionStarted("8F604653A8CC694E6954B51FE6D26127", "Test");
        }
    }

    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        Application.Quit();

        //保存心数相关
        PlayerPrefs.SetInt("HeartCount", GlobalVars.HeartCount);
        PlayerPrefs.SetString("GetHeartTime", Convert.ToString(GlobalVars.GetHeartTime));

        PlayerPrefs.SetFloat("PlayTime", GetPlayTime());

		if (CapsConfig.EnableTalkingData)
            TalkingDataPlugin.SessionStoped();
    }

    public float GetPlayTime()
    {
        float elapseTime = Time.realtimeSinceStartup - m_startAppTime;
        return m_playTime + elapseTime;
    }

    protected override State CreateState(int statEnum)
    {
        CurStateEnum = (StateEnum)statEnum;
        switch (statEnum)
        {
            case (int)StateEnum.Login:
                {
                    return new LoginState();
                }
            case (int)StateEnum.Game:
                {
                    return new GameState();
                }
        }
        return null;
    }
}
