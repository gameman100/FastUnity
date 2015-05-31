using UnityEngine;
using System.Collections;

[AddComponentMenu("Game/State/SceneState")]
public abstract class SceneState : BaseState {

    protected GameObject m_uiroot;
    public GameObject uiroot { get { return m_uiroot; } }

}
