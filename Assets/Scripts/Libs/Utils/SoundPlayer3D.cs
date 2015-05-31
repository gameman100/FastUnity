using UnityEngine;
using System.Collections;

[AddComponentMenu("Game/Misc/SoundPlayer3D")]
public class SoundPlayer3D : MonoBehaviour {

    AudioSource m_audioPlayer;
    AudioClip mClip = null;

    static Transform sound3DGroup = null;
    
    /// <summary>
    /// 创建3D音效
    /// </summary>
    public static void Create ( string clipname, Vector3 pos ) {

        if (sound3DGroup == null)
        {
            GameObject group = new GameObject();
            group.name = "Sound3DGroup";
            sound3DGroup = group.transform;
        }

        GameObject go = new GameObject();
        go.name = "audio " + clipname;
        go.transform.parent = sound3DGroup;

        SoundPlayer3D soundx = go.AddComponent<SoundPlayer3D>();
        soundx.m_audioPlayer = go.AddComponent<AudioSource>();
        soundx.transform.position = pos;
        soundx.GetComponent<AudioSource>().minDistance = 200;
        soundx.mClip = ResManager.LoadSound(clipname);
        
        soundx.PlayOnce();

        Destroy(go, 3);
    }

    /// <summary>
    /// 播放一次
    /// </summary>
    protected void PlayOnce()
    {
        m_audioPlayer.volume = SystemSettings.Sound_Volume;
        m_audioPlayer.minDistance = 10;
        m_audioPlayer.clip = mClip;
        m_audioPlayer.Play();
       
    }
}
