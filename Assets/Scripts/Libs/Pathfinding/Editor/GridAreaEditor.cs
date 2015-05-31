using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GridArea))]
public class GridAreaEditor : Editor
{

    GridArea m_area;
    //bool m_isEdit = false;
    int m_block = 1; //block id
    int m_toolbarid = 0;

    void OnEnable()
    {
        m_area = (GridArea)this.target;
    }

    void OnSceneGUI()
    {
        if (0 == m_toolbarid)
        {
            return;
        }
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        int id = GUIUtility.GetControlID(FocusType.Passive);
        EventType type = Event.current.GetTypeForControl(id);

        if ((type == EventType.MouseDown || type == EventType.MouseDrag) && Event.current.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );
            RaycastHit hit;

            LayerMask lm = (LayerMask)Mathf.Pow(2.0f, (float)LayerMask.NameToLayer("ground"));
            bool ok =Physics.Raycast( ray,  out hit, 1000, lm );
            if ( ok ){

                if (m_toolbarid == 1)
                {
                    //Debug.Log("paint");
                    m_area.SetBlock(hit.point, 1);
                }
                else if (m_toolbarid == 2)
                    m_area.SetBlock(hit.point, 0);

            }
        }

        HandleUtility.Repaint();
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Map Editor");
        if (GUILayout.Button("Build"))
            m_area.Build();
        if (GUILayout.Button("Save"))
        {
            // 保存文件
        }
        if (GUILayout.Button("Load"))
        {
            // 读取文件
        }
       
        string[] toolbar = {"off","obstacle", "free"};
        m_toolbarid = GUILayout.Toolbar(m_toolbarid, toolbar);

        //m_isEdit = GUILayout.Toggle(m_isEdit, "Edit");
       
        DrawDefaultInspector();
    }
}
