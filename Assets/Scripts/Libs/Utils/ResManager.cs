using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResManager : MonoBehaviour{
	/// <summary>
	/// GameObject prefab list
	/// </summary>
	private static Dictionary<string, GameObject> m_objectResList = new Dictionary<string, GameObject>();
    /// <summary>
    /// FX prefab list
    /// </summary>
    private static Dictionary<string, GameObject> m_fxList = new Dictionary<string, GameObject>();
    /// <summary>
    /// 音效列表
    /// </summary>
    private static Dictionary<string, AudioClip> m_audioClips = new Dictionary<string, AudioClip>();

	public static T Load<T>( string name ) where T: Object
	{
		T t = Resources.Load<T>( name );
		if ( null == t )
		{
			Debug.LogError("can not load resoucre " + name );
		}
		return t;
	}

	public static GameObject LoadObject( string name )
	{
		GameObject prefab = null;
		if ( m_objectResList.ContainsKey(name) )
		{
			m_objectResList.TryGetValue( name, out prefab );
		}
		else
		{
			prefab = Resources.Load<GameObject>(name);
            m_objectResList.Add(name, prefab);
		}

		if ( null == prefab)
		{
			Debug.LogError("can not load resoucre " + name );
		}
		return prefab;
	}

    /// <summary>
    /// 读取本地音源
    /// </summary>
    public static AudioClip LoadSound(string soundname)
    {
        AudioClip ac = null;
        if (m_audioClips.ContainsKey(soundname))
        {
            m_audioClips.TryGetValue(soundname, out ac);
        }
        else
        {
            ac = Resources.Load<AudioClip>("audio/" + soundname);
            m_audioClips.Add(soundname, ac);
        }

        if (ac == null)
        {
            Debug.LogError("can not find audio clip " + soundname);
        }
        return ac;
    }

    /// <summary>
    /// 读取FX资源
    /// </summary>
    public static GameObject LoadFXRes(string fxname)
    {
        if (string.IsNullOrEmpty(fxname))
        {
            Debug.LogError("can not load empty fx asset");
            return null;
        }

        GameObject prefab = null;
        m_fxList.TryGetValue(fxname, out prefab);
        if (prefab == null)
        {
            prefab = Load<GameObject>(fxname);
            if (prefab != null)
                m_fxList.Add(fxname, prefab);
        }

        if (prefab == null)
        {
            Debug.LogError("Can not find the fx asset: " + fxname);
        }

        return prefab;
    }

    public static GameObject LoadCharacter( string chname, string aniname )
    {
        GameObject go = null;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (string.IsNullOrEmpty(aniname)) // 只读取模型prefab
        {
            sb.Append("ch/").Append(chname).Append("/").Append("p_").Append(chname);
        }
        else // 读取动画模型文件
        {
            sb.Append("ch/").Append(chname).Append("/").Append(chname).Append("@").Append(aniname);
        }

        string path = sb.ToString();
        go = LoadObject(path);
        if (go == null)
            Debug.LogError("can not find " + path);

        return go;
    }

	public static void UnloadAll()
	{
		m_objectResList.Clear();
        m_fxList.Clear();
        m_audioClips.Clear();
	}

	/// <summary>
	/// 从Resources中实例化GameObject
	/// </summary>
	public static GameObject CreateObject ( string name  )
	{
		GameObject prefab = LoadObject(name);
		if ( null == prefab )
			return null;
		
		return (GameObject)Instantiate( prefab );
	}

	/// <summary>
	/// 从Resources中实例化GameObject
	/// </summary>
	public static GameObject CreateObject ( string name, Vector3 pos, Quaternion rot  )
	{
		GameObject prefab = LoadObject(name);
		if ( null == prefab )
			return null;

		return (GameObject)Instantiate( prefab, pos, rot );
	}
}
