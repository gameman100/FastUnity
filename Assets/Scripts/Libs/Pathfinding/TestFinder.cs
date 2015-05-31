using UnityEngine;
using System.Collections;


[AddComponentMenu("Game/Pathfinding/TestFinder")]
public class TestFinder : MonoBehaviour {

    public bool isControl = false;

    // cache
    protected Transform m_transform;
    protected Animation m_animation;

    // 移动速度
    protected float m_speed = 3.0f;

    protected float m_idle_timer = 1.0f;

    // pathfinder
    protected PathFinder m_finder;

    // 场地
    protected GridArea m_area;

    delegate void VoidDelegate();
    event VoidDelegate m_currentAction;

    Vector3 m_destination;


    // debug
    public int nodeCount = 0;
    public Vector3 nodePosition;


    void Awake()
    {
        m_transform = this.transform;
    }

	// Use this for initialization
	void Start () {

        m_area = GridManager.Get.GetArea(1);

        m_destination = this.transform.position;

        // 创建寻路器
        m_finder = new PathFinder(m_area.map, this.m_transform.position.x, this.m_transform.position.z, true, 0, 1, 10);

        m_animation = this.GetComponent<Animation>();
        this.m_animation["idle"].wrapMode = WrapMode.Loop;
        this.m_animation["run"].wrapMode = WrapMode.Loop;
        m_animation.Play("idle");

        m_currentAction = Idle;

	}
	
	// Update is called once per frame
	void Update () {


        if (m_currentAction != null)
            m_currentAction();

        // get debug info
        nodeCount = m_finder.NodesCount;
        PathVector3 next = m_finder.GetCurrentNodePosition();
        if ( next!=null )
            nodePosition = GridUtility.FloatToVector(next);
	}

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "Player.tif");
    }


    void Idle()
    {
        if (!m_animation.IsPlaying("idle"))
            m_animation.CrossFade("idle");



        if (isControl)
        {
        //    m_finder.BuildPath(GridUtility.VectorToPath(m_transform.position), GridUtility.VectorToPath(m_destination), 60, true, false, false);
        //    if (m_finder.HasPath())
        //    {
        //        m_currentAction = Run;
        //    }
            
            return;

        }

        m_idle_timer -= Time.deltaTime;
        if (m_idle_timer > 0)
            return;

        m_idle_timer = 1;
        // search
        Vector3 target = this.transform.position;
        float rx = Random.value  - 0.5f;
        float rz = Random.value  - 0.5f;
        target.x += rx * 15.0f;
        target.z += rz * 15.0f;

        FindPath(target);
    }

    void Run()
    {
        if (!m_animation.IsPlaying("run"))
            m_animation.CrossFade("run");

        bool ok =GridUtility.MoveToTarget(m_finder, ref m_transform, m_speed * Time.deltaTime, 30);
        if (ok)
        {
            if (!m_finder.HasPath())
                m_currentAction = Idle;
            return;
        }

        if ( !isControl )
            m_currentAction = Idle;
        else
        {
            ok = m_finder.BuildPath(GridUtility.VectorToPath(m_transform.position), GridUtility.VectorToPath(m_destination), 60, true, false, false);
            if (!m_finder.HasPath() || !ok)
                m_currentAction = Idle;
        }
    }

    public void FindPath( Vector3 targetpos )
    {
        m_finder.BuildPath(GridUtility.VectorToPath(m_transform.position), GridUtility.VectorToPath(targetpos), 60, true, false, false);
        if (m_finder.HasPath())
        {
            m_destination = GridUtility.FloatToVector( m_finder.destination );
            m_currentAction = Run;
        }
    }
   

}
