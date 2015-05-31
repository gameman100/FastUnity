using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[AddComponentMenu("Game/State/StateMainMenu")]
public class StateMainMenu : SceneState {

	/// <summary>
	/// 异步读取场景
	/// </summary>
	protected AsyncSceneLoader m_sceneLoader;

    public override void OnEnter()
    {
		//m_sceneLoader = new AsyncSceneLoader();
		//m_sceneLoader.LoadScene(, OnLevelLoaded);
    }

    public override void OnUpdate()
    {
		m_sceneLoader.OnUpdate();
    }

    public override void OnLeave()
    {
    }

	private void OnLevelLoaded()
	{
	}
}
