using UnityEngine;
using System.Collections;

/// <summary>
/// 基础状态类
/// </summary>
[AddComponentMenu("Game/State/BaseState")]
public abstract class BaseState : MonoBehaviour
{
	[SerializeField]
    protected BaseState m_current = null;
    /// <summary>
    /// 当前子状态
    /// </summary>
    public BaseState current { get { return m_current; }  }


    protected BaseState m_last = null;
    /// <summary>
    /// 前一个子状态
    /// </summary>
    public BaseState last { get { return m_last; } set { m_last = value; } }

    /// <summary>
    /// 父节点
    /// </summary>
    public BaseState stateParent { get; set; }

	// 更新循环
    public abstract void OnUpdate();
	
    /// <summary>
    /// 初始化当前状态
    /// </summary>
    public abstract void OnEnter();
	
    /// <summary>
    /// 离开当前状态
    /// </summary>
    public abstract void OnLeave();

    /// <summary>
    /// 添加子节点
    /// </summary>
    public T AddState<T>() where T : BaseState
    {
        T t = this.gameObject.GetComponent<T>();
        if (t == null)
        {
            t = this.gameObject.AddComponent<T>();
        }
        return t;
    }

    public T AddObject<T>(GameObject rootobject) where T : BaseState
    {
        GameObject go = new GameObject();
        go.name = typeof(T).ToString();
        T t = go.AddComponent<T>();
		t.transform.parent = rootobject.transform;

        return t;
    }

    /// <summary>
    /// 改变当前状态
    /// <summary>
    public virtual void ChangeState(BaseState state)
    {
        if (m_current != null)
            m_current.OnLeave();

        if (state == null)
        {
            m_current = null;
        }
        else
        {
            // 用于追溯到之前的状态
            state.last = m_current;
            m_current = state;
            m_current.OnEnter();
        }
    }

    /// <summary>
    /// 是否可以返回到前一个状态
    /// </summary>
    /// <returns></returns>
    public bool CanRollback()
    {
        if (m_current == null || m_current.last == null)
            return false;

        return true;
    }

    //返回到前一个状态
    public virtual void RollbackState()
    {
        if (m_current == null)
            return;

        m_current.OnLeave();

        m_current = m_current.last;
        if (m_current != null)
            m_current.OnEnter();
    }

}