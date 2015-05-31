using UnityEngine;
using System.Collections;

/// <summary>
/// 系统设置
/// </summary>
public static class SystemSettings {

    /// <summary>
    /// 是否开启背景音乐
    /// </summary>
    public static int Music_Volume
    {
        get
        {
            int ret = PlayerPrefs.GetInt("Music_Volume", 1);
            return ret;
        }
        set
        {
            PlayerPrefs.SetInt("Music_Volume", value);
        }
    }

    /// <summary>
    /// 是否开启音效
    /// </summary>
    public static int Sound_Volume
    {
        get
        {
            int ret = PlayerPrefs.GetInt("Sound_Volume", 1);
            return ret;
        }
        set
        {
            PlayerPrefs.SetInt("Sound_Volume", value);
        }
    }
}
