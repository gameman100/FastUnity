using UnityEngine;
using System.Collections;

/// <summary>
/// 关卡根物体
/// </summary>
public class LevelRoot : MonoBehaviour {

    static LevelRoot mShare;
    public static LevelRoot share
    {
        get
        {
            if (mShare == null)
            {
                mShare = GameObject.FindObjectOfType<LevelRoot>();

            }
            return mShare;
        }
    }
    
    /// <summary>
    /// 创建场景根物体
    /// </summary>
    static LevelRoot Create()
    {
        LevelRoot root = GameObject.FindObjectOfType<LevelRoot>();
        if (root != null)
        {
            return root;
        }

        GameObject go = new GameObject();
        go.name = "LevelRoot";
        go.isStatic = true;
        root = go.AddComponent<LevelRoot>();

        return root;
    }

  

}
