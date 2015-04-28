using UnityEngine;
using System.Collections;

[AddComponentMenu("Test/TestHttp")]
public class TestHttp : MonoBehaviour {

    private string addr = "http://localhost:8000/polls/unitytest/";

	// Use this for initialization
	void Start () {

        StartCoroutine(RawEcho());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator RawEcho() // 发送一个用于测试的post请求
    {
        WWWForm form = new WWWForm();
        form.AddField("message", "hello,world");

        WWW www = new WWW(addr,form);

        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("error:" + www.error);
        }
        else
            Debug.Log("response:" + www.text);
    }
}
