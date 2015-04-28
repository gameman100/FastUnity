using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// HTTP管理器
/// </summary>
public class HttpManager
{
    protected static HttpManager m_instance = null; // 单例
    public static HttpManager Get
    {
        get
        {
            if (m_instance == null)
                m_instance = new HttpManager();

            return m_instance;
        }
    }

    private Queue<HttpRequest> m_requests = new Queue<HttpRequest>();     //消息队列
    public int request_cout { get { return m_requests.Count; } } // 消息队列个数

    /// <summary>
    /// 创建HttpRequest
    /// </summary>
    public static HttpRequest CreateRequest( string URL, WWWForm form, System.Action<JPParser> callback )
    {
        HttpRequest request = new HttpRequest(URL, form, callback);

        m_instance.m_requests.Enqueue(request);

        return request;
    }

	// Update is called once per frame
	public void OnUpdate () 
    {
        if (m_requests.Count == 0)
            return;

        HttpRequest request = m_requests.Peek();
        if (request.status == HttpStatus.Waiting)
            return;

        else if (request.status == HttpStatus.Timeout)
        {
        }
        else if (request.status == HttpStatus.HttpError)
        {
        }
        else if (request.status == HttpStatus.ReponseError)
        {
        }
        else if (request.status == HttpStatus.Finish)
        {
        }
        m_requests.Dequeue();

	} // end OnUpdate

}
