using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Game/Pathfinding/GridManager")]
public class GridManager : MonoBehaviour {

    protected static GridManager m_instance = null;

    protected List<GridArea> m_areaList = null;
    public GridArea GetArea(int id)
    {
        GridArea area = m_areaList.Find((x) => x.id == id);
        return area;
    }

    public void AddArea(GridArea area)
    {
        if (GetArea(area.id)!=null)
        {
            //Logger.Out("area (" + area.id + ") is already added");
            return;
        }
        m_areaList.Add(area);
    }

    public static GridManager Get
    {
        get
        {
            if (null == m_instance)
            {
                GameObject go = new GameObject();
                m_instance = go.AddComponent<GridManager>();
                m_instance.Init();
            }

            return m_instance;
        }
    }

    protected void Init()
    {
        m_areaList = new List<GridArea>();
        this.gameObject.name = "GridManager";
    }


}
