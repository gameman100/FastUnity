using UnityEngine;
using System.Collections;

/// <summary>
/// Play 3D Sound
/// </summary>
[AddComponentMenu("Game/Misc/SoundPlayer3D")]
public class SoundPlayer3D : MonoBehaviour {

    private AudioSource m_audioPlayer;

    private static Transform sound3DGroup = null;
    
    /// <summary>
    /// Play Once
    /// </summary>
    public static void PlayOnce( string clipname, Vector3 pos ) {

        if (sound3DGroup == null)
        {
            GameObject group = new GameObject("Sound3DGroup");
            sound3DGroup = group.transform;
        }

        GameObject go = new GameObject("audio " + clipname);
        go.transform.parent = sound3DGroup;
        go.transform.position = pos;

        AudioSource sound = go.AddComponent<AudioSource>();
        sound.minDistance = 200;
        sound.clip = ResManager.LoadSound(clipname);
        sound.Play();

        sound.volume = SystemSettings.Sound_Volume;

        Destroy(go, 3);
    }

    /// <summary>
    /// Create a handler
    /// </summary>
    public static SoundPlayer3D Create(Transform parentobj, Vector3 pos)
    {
        GameObject go = new GameObject("audio3d");
        go.transform.position = pos;
        go.transform.parent = parentobj;
        
        SoundPlayer3D soundx = go.AddComponent<SoundPlayer3D>();
        soundx.m_audioPlayer = go.AddComponent<AudioSource>();
        soundx.GetComponent<AudioSource>().minDistance = 200;

        return soundx;
    }

    /// <summary>
    /// play once
    /// </summary>
    protected void PlayOnce(string clipname)
    {
        m_audioPlayer.volume = SystemSettings.Sound_Volume;
        m_audioPlayer.minDistance = 10;
        m_audioPlayer.clip = ResManager.LoadSound(clipname);
        m_audioPlayer.Play();
    }
}
