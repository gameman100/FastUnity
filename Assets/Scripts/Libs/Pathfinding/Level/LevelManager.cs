using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawn管理器
/// </summary>
public class LevelManager : MonoBehaviour {

    public static LevelManager m_share = null;
    public static LevelManager Get
    {
        get
        {
            if (m_share == null)
            {
                GameObject go = new GameObject();
                go.name = "SpawnManager";
                m_share = go.AddComponent<LevelManager>();
            }

            return m_share;
        }
    }

    LevelRoot m_root;
    public LevelRoot root { get { return m_root; } }
    [SerializeField]
    protected List<IDObject> m_zonelist;

    protected Dictionary<int, List<IDObject>> mPlayerSpawnList;
    protected Dictionary<int, List<IDObject>> mEnemySpawnList;
    protected Dictionary<int, List<IDObject>> mTransporterList; 

    /// <summary>
    /// 当前Player的SpawnID
    /// </summary>
    protected int playerSpawnID = 1;

    public void Init()
    {
        m_root = GameObject.FindObjectOfType<LevelRoot>();
        if (m_root == null)
        {
            Debug.LogError("There is no LevelRoot Object in the scene!");
            return;
        }

        if (m_zonelist == null || m_zonelist.Count == 0)
        {
            m_zonelist = new List<IDObject>();
            foreach (Zone zone in m_root.GetComponentsInChildren<Zone>())
            {
                m_zonelist.Add(zone);
            }
        }
        m_zonelist.Sort(new IDObjectComparer());

        
        mPlayerSpawnList = new Dictionary<int, List<IDObject>>();
        mEnemySpawnList = new Dictionary<int, List<IDObject>>();
        mTransporterList = new Dictionary<int, List<IDObject>>();
        foreach (Zone zone in m_zonelist)
        {
            // 查找PlayerSpawn
            PlayerSpawn[] playerspawns = zone.GetComponentsInChildren<PlayerSpawn>();
            List<IDObject> plist = new List<IDObject>(playerspawns);
            plist.Sort( new IDObjectComparer() );
            if (!mPlayerSpawnList.ContainsKey(zone.id))
                mPlayerSpawnList.Add(zone.id, plist);
            else
            {
                Debug.LogError("zone id key problem:" + zone.id);
            }
            foreach (PlayerSpawn spawn in playerspawns)
            {
                spawn.zone_id = zone.id;
            }

            // 查找EnemySpawn
            EnemySpawn[] enemyspawns = zone.GetComponentsInChildren<EnemySpawn>();
            List<IDObject> elist = new List<IDObject>(enemyspawns);
            elist.Sort( new IDObjectComparer() );
            mEnemySpawnList.Add(zone.id, elist);
            foreach (EnemySpawn spawn in enemyspawns)
            {
                spawn.zone_id = zone.id;
            }

            // 查找LevelTransporter
            LevelTransporter[] transporters = zone.GetComponentsInChildren<LevelTransporter>();
            List<IDObject> tlist = new List<IDObject>(transporters);
            tlist.Sort( new IDObjectComparer() );
            mTransporterList.Add(zone.id, tlist);
            foreach (LevelTransporter trans in transporters)
            {
                trans.zone_id = zone.id;// 更改了数据
                trans.next_zone_id = 0; // 更改了数据
                trans.next_spawn_id = 0;// 更改了数据
            }
        }
    }

    public void AddZone( Zone z )
    {
        if ( m_zonelist==null )
            m_zonelist = new List<IDObject>();
        m_zonelist.Add(z);
    }

    /// <summary>
    /// 查找zone
    /// </summary>
    public Zone FindZone(int zoneid)
    {
        IDObject found = m_zonelist.Find(
            delegate(IDObject zone)
            {
                return zone.id == zoneid;
            }
        );
        return (Zone)found;
    }

    /// <summary>
    /// 查找PlayerSpawn
    /// </summary>
    public PlayerSpawn FindPlayerSpawn(int zoneid, int playerSpawnid )
    {
        if (mPlayerSpawnList.ContainsKey(zoneid))
        {
            List<IDObject> spawns;
            mPlayerSpawnList.TryGetValue(zoneid, out spawns);
           
            IDObject spawn = spawns.Find(
                delegate(IDObject s)
                {
                    return s.id == playerSpawnid;
                }
            );

            return (PlayerSpawn)spawn;
        }

        return null;
    }

    /// <summary>
    /// 查找EnemySpawn
    /// </summary>
    public EnemySpawn FindEnemySpawn(int zoneid, int enemyspawnid)
    {
        if ( mEnemySpawnList.ContainsKey(zoneid))
        {
            List<IDObject> spawns;
            mEnemySpawnList.TryGetValue(zoneid, out spawns);

            IDObject spawn = spawns.Find(
                delegate(IDObject s)
                {
                    return s.id == enemyspawnid;
                }
            );

            return (EnemySpawn)spawn;
        }

        return null;
    }

    /// <summary>
    /// 查找Transporter
    /// </summary>
    public LevelTransporter FindTransporter(int zoneid, int tid )
    {
        if ( this.mTransporterList.ContainsKey(zoneid))
        {
            List<IDObject> spawns;
            mTransporterList.TryGetValue(zoneid, out spawns);

            IDObject spawn = spawns.Find(
                delegate(IDObject s)
                {
                    return s.id == tid;
                }
            );

            return (LevelTransporter)spawn;
        }

        return null;
    }

    /// <summary>
    /// 查找Transporter
    /// </summary>
    public LevelTransporter FindTransporter(int zoneid, LevelTransporter.TransporterType type = LevelTransporter.TransporterType.Main)
    {
        if (this.mTransporterList.ContainsKey(zoneid))
        {
            List<IDObject> spawns;
            mTransporterList.TryGetValue(zoneid, out spawns);

            IDObject spawn = spawns.Find(
                delegate(IDObject s)
                {
                    return ((LevelTransporter)s).next_zone_id != 0 && ((LevelTransporter)s).type == type;
                }
            );

            return (LevelTransporter)spawn;
        }

        return null;
    }

    /// <summary>
    /// 查找Transporter
    /// </summary>
    public List<LevelTransporter> FindTransporters(int zoneid)
    {
        List<LevelTransporter> list = new List<LevelTransporter>();

        if (this.mTransporterList.ContainsKey(zoneid))
        {
            List<IDObject> spawns;
            mTransporterList.TryGetValue(zoneid, out spawns);


            foreach (IDObject lt in spawns)
            {
                LevelTransporter trasporter = (LevelTransporter)lt;
                if (trasporter.zone_id == zoneid)
                    list.Add(trasporter);
            }
        }

        return list;
    }
}
