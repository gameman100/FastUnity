using UnityEngine;
using System.Collections;

public class LevelTransporter : Spawn
{
    [HideInInspector]
    //public int zone_id = 1;
    /// <summary>
    /// 传送的zone id
    /// </summary>
    public int next_zone_id = 1;
    /// <summary>
    /// 传送的playerspawn id
    /// </summary>
    public int next_spawn_id = 1;

    /// <summary>
    /// 是否开启
    /// </summary>
    bool isOn = false;

    public enum TransporterType
    {
        None =0,
        Main = 1,
        Branch = 2
    }

    /// <summary>
    /// 1 主线 2 支线
    /// </summary>
    public TransporterType type = TransporterType.Main;

    private GameObject m_fx = null;

    void Start()
    {
        BoxCollider box = this.gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0, 0.5f, 0);
        box.size = new Vector3(1.0f, 1, 1.0f);
        box.isTrigger = true;

    }


    /// <summary>
    /// 创建
    /// </summary>
    public static GameObject Create()
    {
        GameObject go = new GameObject();
        go.name = "Transporter";
        go.AddComponent<LevelTransporter>();

        return go;
    }

    public void SetOn()
    {
        if (isOn || next_zone_id<=0 || next_spawn_id<=0 )
            return;

        isOn = true;
    }

    void OnTriggerEnter(Collider go)
    {
        if (!isOn)
            return;

        if (this.next_zone_id == 0 || this.next_spawn_id == 0)
            return;

        isOn = false;

        Destroy(m_fx, 1.0f);
    }
   
    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "x.PNG", true);

        Gizmos.color = new Color(0, 0, 1);

        Gizmos.DrawWireSphere(this.transform.position, 1);

        Gizmos.DrawRay(transform.position, transform.TransformDirection(0, 0, 10));
    }
}
