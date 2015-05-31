using UnityEngine;
using System.Collections;

[AddComponentMenu("Game/State/StateManager")]
public class StateManager : SceneState {

    private static StateManager m_instance = null;
    public static StateManager Get { get {
        if (m_instance == null)
        {
            GameObject go = new GameObject("StateManager");
            StateManager sm = go.AddComponent<StateManager>();
            sm.Init();
        }
        return m_instance; 
    } }

	protected GameObject m_getObject;

    private bool m_isInit = false;

    /// <summary>
	/// 开始（启动）界面
	/// </summary>
	protected StateSplash m_splashstate;
	/// <summary>
	/// 登陆游戏界面
	/// </summary>
	protected StateLoginMenu m_loginmenu;
	/// <summary>
	/// 主界面
	/// </summary>
    protected StateMainMenu m_mainmenu;
	/// <summary>
	/// 游戏（战斗）界面
	/// </summary>
    protected StateGameplay m_gameplay;


	public void ToStateSplash()
	{
        ChangeState(m_splashstate);
	}

	public void ToLoginMenu()
	{
        ChangeState(m_loginmenu);
	}

    public void ToMainMenu()
    {
        ChangeState(m_mainmenu);
    }

    public void ToGameplay()
    {
        ChangeState(m_gameplay);
    }
    
    void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (m_isInit)
            return;
        m_isInit = true;

        m_instance = this;
		m_getObject = this.gameObject;
        GameObject.DontDestroyOnLoad(this);
    }

	// Use this for initialization
	void Start () {

        m_splashstate = AddObject<StateSplash>(m_getObject);
        m_loginmenu = AddObject<StateLoginMenu>(m_getObject);
		m_mainmenu = AddObject<StateMainMenu>(m_getObject);
        m_gameplay = AddObject<StateGameplay>(m_getObject);

        ChangeState(m_splashstate);

		Debug.Log( "Game Start" );
	}
	
	// Update is called once per frame
	void Update () {

		if ( m_current!= null )
		{
			m_current.OnUpdate();
		}
	
	}

    /// <summary>
    /// 初始化当前状态
    /// </summary>
    public override void OnEnter()
    {
    }

    /// <summary>
    /// 更新循环
    /// </summary>
    public override void OnUpdate()
    {
    }

    /// <summary>
    /// 离开当前状态
    /// </summary>
    public override void OnLeave()
    {
    }
}
