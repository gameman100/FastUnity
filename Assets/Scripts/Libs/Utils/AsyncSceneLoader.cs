using UnityEngine;
using System.Collections;

/// <summary>
/// 场景加载器
/// </summary>
public class AsyncSceneLoader {

	/// <summary>
	/// 异步读取场景
	/// </summary>
	protected AsyncOperation m_async = null;
	/// <summary>
	/// 读取进度
	/// </summary>
	protected float m_progress = 0;

    public delegate void VoidDelegate();
	/// <summary>
	/// 读取关卡callback
	/// </summary>
    private event VoidDelegate m_asyncEvent = null;

    private string m_sceneName = string.Empty;

	public AsyncSceneLoader()
	{

	}

	public void OnUpdate()
	{
		if ( m_async !=null )
		{
			// todo: show some loading ui here...

			// update loading progress
			m_progress = m_async.progress;

			if (m_async.isDone)
			{
				m_progress = 1.0f;

				VoidDelegate temp = m_asyncEvent;
				if (m_asyncEvent != null)
					m_asyncEvent();
				
				if (m_asyncEvent == temp){ // if nothing change
					m_asyncEvent = null;
					m_async = null;
				}
			}
        }
        else if (m_asyncEvent!=null)
        {
            if (Application.loadedLevelName.CompareTo(m_sceneName) == 0)
            {
                m_asyncEvent();
                m_asyncEvent = null;
            }
        }
	}

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="scenename"></param>
    /// <param name="onLoaded"></param>
	public void LoadScene( string scenename, VoidDelegate onLoaded )
	{
        m_sceneName = scenename;

        m_progress = 0;
        m_asyncEvent = onLoaded;
        m_async = Application.LoadLevelAsync(scenename);
	}
}
