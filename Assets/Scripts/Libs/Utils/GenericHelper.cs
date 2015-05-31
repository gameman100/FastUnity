using UnityEngine;
using System.Collections;

/// <summary>
/// 实用功能
/// </summary>
public class GenericHelper : MonoBehaviour {

    /// <summary>
    /// 播放粒子特效
    /// </summary>
    public static void PlayParticles(ParticleSystem[] particles, bool play)
    {
        if (particles == null)
            return;

        foreach (ParticleSystem ps in particles)
        {
            if (ps == null)
                continue;
            if (play)
                ps.Play(true);
            else
                ps.Stop(true);
        }
    }

    /// <summary>
    /// 通过层名称获得层
    /// </summary>
    public static LayerMask GetLayerMask(string ln)
    {
        return (LayerMask)Mathf.Pow(2.0f, (float)LayerMask.NameToLayer(ln));
    }
}
