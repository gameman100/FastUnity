using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[AddComponentMenu("Game/State/StateLoginMenu")]
public class StateLoginMenu : SceneState {

    // Depends on Lib Utils
	// AsyncSceneLoader m_sceneLoader;
	
	/// <summary>
	/// 初始化当前状态
	/// </summary>
	public override void OnEnter()
	{
        Debug.Log("Login Menu");

        /*
		m_sceneLoader = new AsyncSceneLoader();
		m_sceneLoader.LoadScene( "", OnLevelLoaded ); // You need to assgin a scenename
        */
	}
	
	// 更新循环
	public override void OnUpdate()
	{
		//m_sceneLoader.OnUpdate();
	}
	
	/// <summary>
	/// 离开当前状态
	/// </summary>
	public override void OnLeave()
	{
        if (m_uiroot != null)
            Destroy(m_uiroot);
	}
	
	private void OnLevelLoaded()
	{
        /*
        // 创建UI
        m_uiroot = ResManager.CreateObject("ui_startmenu");

        Button[] buttons = m_uiroot.GetComponentsInChildren<Button>();
        foreach ( Button but in buttons )
        {
            if (but.name.CompareTo("btn_startgame") == 0)
            {
                Text t = but.GetComponentInChildren<Text>();
                t.text = "开始游戏";
                but.onClick.AddListener(OnButtonStartGame);

                
            }
        }

        // 播放音乐
        AudioClip clip = ResManager.LoadSound(SoundName.bgm_login);
        SoundPlayer2D.Get.Loop(clip);

        */
	}
	
	private void OnButtonStartGame()
	{
		//StateManager.Get.ToMainMenu();
        //StateManager.Get.ToGameplay();
	}
}
