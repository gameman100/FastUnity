using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;

public class LevelEditMenu : EditorWindow
{
    Vector2 scroll = new Vector2();

    // Add menu named "My Window" to the Window menu
    [MenuItem("LevelEditor/Help")]
    static void Init()
    {
       EditorWindow.GetWindow(typeof(LevelEditMenu), false, "Help");
    }

    void OnGUI()
    {
        scroll=EditorGUILayout.BeginScrollView(scroll);
        
        GUILayout.Space(10);
        GUILayout.Label("---创建关卡美术工作注册事项---");
        GUILayout.Label("所有地面碰撞模型命名以gnav_开头");
        GUILayout.Label("所有墙壁碰撞模型命名以onav_开头");
        GUILayout.Label("所有摄像机碰撞模型命名以cnav_开头，前面碰撞的层设为camera");
        GUILayout.Label("所有摄像机碰撞模型命名以cnav2_开头，侧面碰撞的层设为camera_side");
        GUILayout.Label("选择LevelEditor->Create->LevelRoot创建LevelRoot节点");
        GUILayout.Label("(一定要先选择LevelRoot)选择LevelEditor->Create->Zone创建Zone节点");
        GUILayout.Label("将不同区域的相关模型添加到相应Zone节点之下");
        GUILayout.Label("将需要投影的模型的层设为shadow");
        GUILayout.Label("选择LevelEditor->BuildLevel生成地形信息");

        GUILayout.Space(10);
        GUILayout.Label("---设置关卡的步骤---");
        GUILayout.Label("1. (确定已经创建了LevelRoot，并只有一个)选择LevelEditor->Create->LevelRoot创建LevelRoot节点");
        GUILayout.Label("2. (确定已经创建了相应的Zone，可以有若干个)选择LevelEditor->Create->Zone创建Zone节点");
        GUILayout.Label("3. (一定要先选择Zone)选择LevelEditor->Create->PlayerSpawn创建Player生成节点");
        GUILayout.Label("4. (一定要先选择Zone)选择LevelEditor->Create->EnemySpawn创建Enemy生成节点");
        GUILayout.Label("5. (一定要先选择Zone)选择LevelEditor->Create->Transporter创建Transporter节点");
        GUILayout.Label("6. 选择LevelEditor->Statistic设置节点");

        EditorGUILayout.EndScrollView();
       
    }

    // 菜单命令 创建LevelRoot
    [MenuItem("LevelEditor/Create/LevelRoot")]
    public static LevelRoot CreateLevelRoot()
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

    static bool CheckSelection(string tag)
    {
        if (Selection.activeTransform == null)
            return false;

        Transform selected = Selection.activeTransform;

        if (tag.Contains("root"))
        {
            LevelRoot root = selected.GetComponent<LevelRoot>();
            if (root != null)
                return true;
        }
        else if (tag.Contains("zone"))
        {
            Zone zone = selected.GetComponent<Zone>();
            if (zone != null)
                return true;
        }

        return false;
    }



    // 菜单命令 创建Zone
    [MenuItem("LevelEditor/Create/Zone")]
    public static GameObject CreateZone()
    {
        if (!CheckSelection("root"))
        {
            Debug.LogError("必须先选择LevelRoot");
            return null;
        }

        GameObject go = Zone.Create();
        go.transform.parent = Selection.activeTransform;
        return go;
    }

    // 菜单命令 创建主角生成点
    [MenuItem("LevelEditor/Create/PlayerSpawn")]
    static void CreatePlayerSpawn()
    {
        if (!CheckSelection("zone"))
        {
            Debug.LogError("必须先选择zone");
            return;
        }
        GameObject spawn = PlayerSpawn.Create();
        spawn.transform.parent = Selection.activeTransform;
    }

    // 菜单命令 创建敌人生成点
    [MenuItem("LevelEditor/Create/EnemySpawn")]
    static void CreateEnemySpawn()
    {
        if (!CheckSelection("zone"))
        {
            Debug.LogError("必须先选择zone");
            return;
        }
        GameObject spawn = EnemySpawn.Create();
        spawn.transform.parent = Selection.activeTransform;
    }


    // 菜单命令 创建传送点
    [MenuItem("LevelEditor/Create/Transporter")]
    static void CreateTransporter()
    {
        if (!CheckSelection("zone"))
        {
            Debug.LogError("必须先选择zone");
            return;
        }
        GameObject go = LevelTransporter.Create();
        go.transform.parent = Selection.activeTransform;
    }

    // 菜单命令 创建LevelRoot
    [MenuItem("LevelEditor/Create/GridArea")]
    static void CreateGridArea()
    {
        if (!CheckSelection("zone"))
        {
            Debug.LogError("必须先选择zone");
            return;
        }

        GridArea.Create(Selection.activeTransform);

    }

    // 菜单命令 创建寻路导航
    [MenuItem("LevelEditor/BuildLevel")]
    static void BuildMap()
    {
        LevelRoot root = CreateLevelRoot();
        string name = EditorApplication.currentScene;
        int index =  name.LastIndexOf('/');
        name = name.Substring(index + 1, name.Length - index-1);
        index = name.IndexOf('.');
        name = name.Substring(0, index);

        root.name = name;
        Transform[] children = root.GetComponentsInChildren<Transform>();

        int defaultlayer = GameObjectUtility.GetNavMeshLayerFromName("Default");
        //int olayer = GameObjectUtility.GetNavMeshLayerFromName("Not Walkable");

        foreach (Transform t in children)
        {
            if (t.name.StartsWith("gnav_")) // 地面
            {
                MeshRenderer r = t.GetComponent<MeshRenderer>();
                if (r != null)
                    r.enabled = true;
                t.gameObject.isStatic = true;
                //GameObjectUtility.SetNavMeshLayer(t.gameObject, defaultlayer);
                t.gameObject.layer = LayerMask.NameToLayer("ground");
                MeshCollider col = t.GetComponent<MeshCollider>();
                if (col == null)
                {
                    col = t.gameObject.AddComponent<MeshCollider>();
                    col.isTrigger = false;
                }
            }
            else if (t.name.StartsWith("onav_")) // 墙
            {
                MeshRenderer r = t.GetComponent<MeshRenderer>();
                if (r != null)
                    r.enabled = true;
                t.gameObject.isStatic = true;
                //GameObjectUtility.SetNavMeshLayer(t.gameObject, olayer);
                MeshCollider col = t.GetComponent<MeshCollider>();
                if (col == null)
                {
                    col = t.gameObject.AddComponent<MeshCollider>();
                    col.isTrigger = true;
                }
                t.gameObject.layer = LayerMask.NameToLayer("wall");
            }
            else if (t.name.StartsWith("cnav_")) // 摄像机
            {
                MeshRenderer r = t.GetComponent<MeshRenderer>();
                if (r != null)
                    r.enabled = true;
                t.gameObject.isStatic = true;
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, ~StaticEditorFlags.NavigationStatic);
                MeshCollider col = t.GetComponent<MeshCollider>();
                if (col == null)
                {
                    col = t.gameObject.AddComponent<MeshCollider>();
                    col.isTrigger = true;
                }
                //t.gameObject.layer = LayerMask.NameToLayer("camera");// 摄像机的层需要手工设置
            }
            else if (t.name.StartsWith("cnav2_")) // 摄像机
            {
                MeshRenderer r = t.GetComponent<MeshRenderer>();
                if (r != null)
                    r.enabled = true;
                t.gameObject.isStatic = true;
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, ~StaticEditorFlags.NavigationStatic);
                MeshCollider col = t.GetComponent<MeshCollider>();
                if (col == null)
                {
                    col = t.gameObject.AddComponent<MeshCollider>();
                    col.isTrigger = true;
                }
                t.gameObject.layer = LayerMask.NameToLayer("camera_side");
            }
            else
            {
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, ~StaticEditorFlags.NavigationStatic);
            }
        }

        //NavMeshBuilder.ClearAllNavMeshes();
        //NavMeshBuilder.BuildNavMesh();

        foreach (Transform t in children)
        {
            if (t.name.StartsWith("gnav_"))
            {
                MeshRenderer r = t.GetComponent<MeshRenderer>();
                if (r != null)
                    r.enabled = false;
            }
            else if (t.name.StartsWith("onav_"))
            {
                MeshRenderer r = t.GetComponent<MeshRenderer>();
                if (r != null)
                    r.enabled = false;
            }
            else if (t.name.StartsWith("cnav_"))
            {
                MeshRenderer r = t.GetComponent<MeshRenderer>();
                if (r != null)
                    r.enabled = false;
            }
        }
    }

    /// <summary>
    /// 对关卡顺序重新排列
    /// </summary>
    [MenuItem("Project/UpdateBuildSettings")]
    static void UpdateBuildSettings()
    {
        string path = "ProjectSettings/EditorBuildSettings.asset";
        string rawtext = System.IO.File.ReadAllText(path);

        StringReader sr = new StringReader(rawtext);

        //保存基本的(前一部分固定的)文字
        Queue<string> simpletext = new Queue<string>();
        //保存关卡文字
        List<string> sorttext = new List<string>();

        // true 之后的文字需要排序
        bool startsortflag = false;
        while (true)
        {
            try
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                if (!startsortflag)
                {
                    if ( !simpletext.Contains(line) )
                        simpletext.Enqueue(line);

                    if (line.CompareTo("    path: Assets/Scenes/entry.unity") == 0) // 这个关卡必须放到最前面
                        startsortflag = true;
                }
                else
                {
                    if (line.StartsWith("  - enabled:"))
                        continue;

                    if (sorttext.Find((x) => x.CompareTo(line) == 0) == null &&
                       !simpletext.Contains(line))
                    {
                        sorttext.Add(line);
                    }
                }
                //Debug.Log(line);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
                break;
            }
        }//end while

        System.Text.StringBuilder sb = new StringBuilder();
        while (simpletext.Count > 0)
        {
            sb.Append(simpletext.Dequeue());
            sb.Append("\n");
        }

        sorttext.Sort((x, y) => x.CompareTo(y));
        foreach ( string line in sorttext)
        {
            sb.Append("  - enabled: 1\n");
            sb.Append(line);
            sb.Append("\n");
        }
        Debug.Log(sb.ToString());

        System.IO.File.WriteAllText(path, sb.ToString());
    }

    [MenuItem("Tools/SetPlayerPhysic")]
    static void SetPlayerPhysic()
    {
        if (Selection.activeTransform == null)
            return;

        Transform selected = Selection.activeTransform;

        Rigidbody r = selected.gameObject.AddComponent<Rigidbody>();
        r.isKinematic = true;
        r.useGravity = false;

        BoxCollider box = selected.gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(0.8f, 1.6f, 0.8f);
        box.center = new Vector3(0, 0.8f, 0);
    }
}

