using UnityEngine;
using System.Collections;

[AddComponentMenu("Game/Misc/SoundPlayer2D")]
/// <summary>
/// 2D声音播放器
/// </summary>
public class SoundPlayer2D : MonoBehaviour {
    // 播放源
    AudioSource mAudioPlayer;
    // 音效
    AudioClip mClip;

    bool mIsLoop = false;

    private float m_playingTime = 0;

    public bool IsPlaying
    {
        get
        {
            bool bRet = false;
            if (m_playingTime > 0)
                bRet = true;
            return bRet;
        }
    }

    [SerializeField]
    bool m_disable = false;

    protected static SoundPlayer2D mShare;
    public static SoundPlayer2D Get
    {
        get {

            if (mShare == null)
            {
                GameObject go = new GameObject();
                go.name = "sound player 2d";
                mShare = go.AddComponent<SoundPlayer2D>();
                mShare.mAudioPlayer = go.AddComponent<AudioSource>();
                mShare.name = "SoundPlayer2D";
                mShare.mAudioPlayer.volume = SystemSettings.Music_Volume;
            }
            
            return mShare; } 
    
    }

    public void UpdateVolume()
    {
        mAudioPlayer.volume = SystemSettings.Music_Volume;
    }

    /// <summary>
    /// 播放一次
    /// </summary>
    public void PlayOnce( AudioClip clip )
    {
        mAudioPlayer.PlayOneShot(clip);
        m_playingTime = clip.length;
    }
    /// <summary>
    /// 设置当前clip
    /// </summary>
    public void SetClip(AudioClip clip)
    {
        mClip = clip;
    }
    /// <summary>
    /// 循环播放
    /// </summary>
    public void Loop()
    {
        mIsLoop = true;
    }
    /// <summary>
    /// 循环播放
    /// </summary>
    public void Loop(AudioClip clip)
    {
        if (mClip == clip && mIsLoop)
            return;
        mAudioPlayer.Stop();
        mClip = clip;
        mIsLoop = true;
    }
    /// <summary>
    /// 停止播放
    /// </summary>
    public void Stop()
    {
        mIsLoop = false;
        mAudioPlayer.Stop();
        mClip = null;
        m_playingTime = 0;
    }

    void Update()
    {
        if (m_disable)
        {
            if (mClip != null && mIsLoop)
            {
                mAudioPlayer.clip = null;
                mAudioPlayer.Stop();
            }
        }
        else if (mClip != null && mIsLoop)
        {
			if (!mAudioPlayer.isPlaying)
			{
				mAudioPlayer.clip = mClip;
				mAudioPlayer.Play();
			}
        }
		else
		{
			m_playingTime -= Time.deltaTime;
			if (m_playingTime < 0)
				m_playingTime = 0;
		}
    }

}
