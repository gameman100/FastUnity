using UnityEngine;
using System.Collections;

[AddComponentMenu("Game/Common/LookAtCamera")]
public class LookAtCamera : MonoBehaviour
{

    protected Transform m_mytransform;
    protected Transform m_camera_transform;

    public static LookAtCamera Attach(GameObject go, Transform cameratransform)
    {
        LookAtCamera la = go.AddComponent<LookAtCamera>();
        la.m_camera_transform = cameratransform;
        la.Init();
        return la;
    }

    protected void Init()
    {
        m_mytransform = this.transform;

        m_mytransform.eulerAngles = m_camera_transform.eulerAngles;
    }

    void Update()
    {
        if (m_camera_transform == null)
            return;
        m_mytransform.eulerAngles = m_camera_transform.eulerAngles;
    }
}
