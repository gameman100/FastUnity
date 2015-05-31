using UnityEngine;
using System.Collections;

[AddComponentMenu("Game/Pathfinding/TestGameManager")]

public class TestGameManager : MonoBehaviour {

    public static TestGameManager Get { get; set; }

    public TestFinder m_player;

    void Awake()
    {
        Get = this;
    }

	// Use this for initialization
	void Start () {
        
        if (m_player != null)
            m_player.isControl = true;

	}
	
	// Update is called once per frame
	void Update () {
        MyInput();
	}

    void MyInput()
    {
        if (m_player == null)
            return;
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

            RaycastHit hit;
            bool b = Physics.Raycast(ray, out hit, 1000);

            if (b)
            {
                Vector3 target = hit.point;
                m_player.FindPath(target);
            }
        }
    }
}
