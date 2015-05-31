using UnityEngine;
using System.Collections;

[AddComponentMenu("Game/State/StateSplash")]
public class StateSplash : SceneState {

    private float m_logoTimer = 3.0f;
	/// <summary>
	/// 初始化当前状态
	/// </summary>
	public override void OnEnter()
	{
        // 初始化游戏

        // Login gameconfig...

        // 显示游戏logo
        // GameObject go = ResManager.Load<GameObject>("ui_startlogo");
        // Instantiate(go);
	}
	
	// 更新循环
	public override void OnUpdate()
	{
        m_logoTimer -= Time.deltaTime;
        if ( m_logoTimer<0 )
		    StateManager.Get.ToLoginMenu();
	}
	
	/// <summary>
	/// 离开当前状态
	/// </summary>
	public override void OnLeave()
	{
	}
}
