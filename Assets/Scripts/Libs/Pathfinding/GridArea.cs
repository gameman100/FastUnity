using UnityEngine;
using System.Collections;

/// <summary>
/// 对PathMap的Unity封装
/// </summary>
[AddComponentMenu("Game/Pathfinding/GridArea")]
public class GridArea : MonoBehaviour
{

    // Debug mode
    public bool m_debug = false;
    /// <summary>
    /// ID (和zone的ID一致)
    /// </summary>
    public int id = 0;

    // Max Map size
    [SerializeField]
    protected int m_mapSizeX = 32;
    [SerializeField]
    protected int m_mapSizeZ = 32;

    // Max Map size
    public int mapSizeX { get { return m_mapSizeX; } }
    public int mapSizeZ { get { return m_mapSizeZ; } }

    [SerializeField]
    protected float m_quadSize = 1;

    /// <summary>
    /// Map handle
    /// </summary>
    [SerializeField]
    protected PathMap m_map;
    public PathMap map { get { return m_map; } }

    public int[] blocks;
    [SerializeField]
    protected int old_width = 1;
    [SerializeField]
    protected int old_height = 1;

    /// <summary>
    /// Node tag
    /// </summary>
    [SerializeField]
    protected string m_NodeTag = "gridnode";

    private bool m_isInGamePlay =false;
    public static GridArea Create( Transform parent )
    {
        GameObject go = new GameObject();
        go.name = "GridArea";
        go.transform.parent = parent;
        GridArea gridarea = go.AddComponent<GridArea>();

        return gridarea;
    }

    public void Init()
    {
        // 确定和zone的id一致
        //Zone zone = this.transform.parent.GetComponent<Zone>();
        this.id = 1;
        m_isInGamePlay = true;
        GridManager.Get.AddArea(this);
        this.Build();
    }

    void Awake()
    {
        Init();
    }

    public void SetBlock(Vector3 pos, int data)
    {
        if (m_map == null || m_map.m_Data==null || m_map.m_Data.Length==0)
        {
            Build();
            Debug.Log("map is null");
            return;
        }
        PathVector3 pv3 = GridUtility.VectorToPath(pos);
        int tx = pv3.tx(m_map);
        int tz = pv3.tz(m_map);
        bool ok = m_map.CheckValid( pv3 );
        if (!ok)
        {
            Debug.Log("position error:" + pos);
            return;
        }

        blocks[ tx + tz * m_mapSizeX  ] = data;
    }

    [ContextMenu("Clear Data")]
    public void Clear()
    {
        if (blocks == null)
            return;
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i] = 0;
        }
    }

    /// <summary>
    /// Build Map
    /// </summary>
    //[ContextMenu("Build")]
    public void Build()
    {
        if (blocks == null)
        {
            blocks = new int[m_mapSizeX*m_mapSizeZ];
            old_width = m_mapSizeX;
            old_height = m_mapSizeZ;
        }
        else
        {
            if (blocks.Length != m_mapSizeX * m_mapSizeZ)
            {
                int[] newblock = new int[m_mapSizeX*m_mapSizeZ];
                for (int i = 0; i < m_mapSizeX * m_mapSizeZ; i++)
                {
                    if (blocks.Length <= i)
                        continue;

                    int tx = i - ( ( i / m_mapSizeX ) * m_mapSizeX );
                    int tz = i / m_mapSizeX;
                    if (tx >= old_width || tz >= old_height)
                        continue;
                    int index = tx + tz * old_width;
                    if (index >= blocks.Length)
                        continue;

                    newblock[i] = blocks[index];
                }
                blocks = newblock;
                old_width = m_mapSizeX;
                old_height = m_mapSizeZ;
            }

        }

        Vector3 startpos = this.transform.position;
        // create map
        m_map = new PathMap(m_mapSizeX, m_mapSizeZ, startpos.x, startpos.z, m_quadSize);

        // build a empty grid map
        for (int i = 0; i < m_mapSizeX; i++)
        {
            for (int k = 0; k < m_mapSizeZ; k++)
            {
                //MapData data = new MapData();
                //data.fieldtype = MapData.FieldTypeID.FREE;
                //m_map.SetData(i,  k, data);

                if (blocks[i + k * m_mapSizeX] > 0)
                {
                    PathVector3 pv3 = new PathVector3();
                    pv3.Set(i, k, m_map);
                    m_map.SetBlock(pv3, 0, 0, 0);
                }
            }
        }


        /*
        // Find obstacles;
        GameObject[] nodes = (GameObject[])GameObject.FindGameObjectsWithTag(m_NodeTag);
        foreach (GameObject nodeobj in nodes)
        {

            GridNode node = nodeobj.GetComponent<GridNode>();

            Vector3 pos = nodeobj.transform.position;
            if (node.m_IsBlocked == true)
            {
                m_map.SetBlock(pos.x, pos.z, node.m_NodeSize, 0, 0);
            }

            MapData data = this.GetData((int)pos.x, (int)pos.z);
            if (data != null)
            {

            }

        }
        */
    }


    public void SetBlock(Vector3 pos, int size, int dynamic, float movespeed)
    {
        m_map.SetBlock(pos.x, pos.z, size, dynamic, movespeed);
    }


    public Vector3 GetTilePos(float x, float z)
    {
        float[] vec = new float[3];

        m_map.GetTileCenter(x, z, ref vec);

        Vector3 pos = new Vector3(vec[0], vec[1], vec[2]);

        return pos;
    }


    public bool IsBlocked(float x, float z, int size)
    {
        return m_map.IsBlocked2(x, z, size);
    }

    public void SetData(float x, float z, MapData data)
    {
        m_map.SetData(x, z, data);
    }

    public MapData GetData(float x, float z)
    {
        return (MapData)m_map.GetData(x, z);
    }

    /// <summary>
    /// Debug
    /// </summary>
    void OnDrawGizmos()
    {
        if (m_isInGamePlay)
            ShowDebugInGamePlay();
        else
            ShowDebugInEditor();
    }

    void ShowDebugInEditor()
    {
        if (!m_debug || blocks == null || blocks.Length < m_mapSizeX * m_mapSizeZ)
            return;

        Vector3 startpos = this.transform.position;

        Gizmos.color = Color.blue;

        // draw lines
        float height = 0;

        for (int i = 0; i < mapSizeX; i++)
        {
            Gizmos.DrawLine(new Vector3(startpos.x + i * m_quadSize, height, startpos.z), new Vector3(startpos.x + i * m_quadSize, height, startpos.z + mapSizeZ * m_quadSize));
        }
        for (int k = 0; k < mapSizeZ; k++)
        {
            Gizmos.DrawLine(new Vector3(startpos.x, height, startpos.z + k * m_quadSize), new Vector3(startpos.x + mapSizeX * m_quadSize, height, startpos.z + k * m_quadSize));
        }

        // draw red area
        Gizmos.color = Color.red;

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int k = 0; k < mapSizeZ; k++)
            {
                //if (m_map.IsBlocked2(i * m_quadSize + startpos.x, k * m_quadSize + startpos.z, 0))
                if (blocks[i + k * m_mapSizeX] > 0)
                {
                    Gizmos.color = new Color(1, 0, 0, 0.5f);

                    Gizmos.DrawCube(new Vector3(startpos.x + i * m_quadSize + m_quadSize * 0.5f, height, startpos.z + k * m_quadSize + m_quadSize * 0.5f), new Vector3(m_quadSize, height + 0.1f, m_quadSize));

                }
            }
        }
    }

    void ShowDebugInGamePlay()
    {
        if (!m_debug || m_map == null )
            return;

        Vector3 startpos = this.transform.position;
        Gizmos.color = Color.blue;

        // draw lines
        float height = 0;

        for (int i = 0; i < mapSizeX; i++)
        {
            Gizmos.DrawLine(new Vector3(startpos.x + i * m_quadSize, height, startpos.z), 
                new Vector3(startpos.x + i * m_quadSize, height, startpos.z + mapSizeZ * m_quadSize));
        }
        for (int k = 0; k < mapSizeZ; k++)
        {
            Gizmos.DrawLine(new Vector3(startpos.x, height, startpos.z + k * m_quadSize), 
                new Vector3(startpos.x + mapSizeX * m_quadSize, height, startpos.z + k * m_quadSize));
        }

        // draw red area
        Gizmos.color = Color.red;

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int k = 0; k < mapSizeZ; k++)
            {
                if (m_map.IsBlocked2( startpos.x + i * m_quadSize, startpos.z + k * m_quadSize, 0))
                //if (blocks[i + k * m_mapSizeX] > 0)
                {
                    Gizmos.color = new Color(1, 0, 0, 0.5f);

                    Gizmos.DrawCube(new Vector3(startpos.x + i * m_quadSize + m_quadSize * 0.5f, 
                        height, 
                        startpos.z + k * m_quadSize + m_quadSize * 0.5f), 
                        new Vector3(m_quadSize, height + 0.1f, m_quadSize));

                }
            }
        }
    }


    [System.Serializable]
    public class MapData
    {
        public enum FieldTypeID
        {
            FREE,
        }
        public FieldTypeID fieldtype = FieldTypeID.FREE;
    }
}
