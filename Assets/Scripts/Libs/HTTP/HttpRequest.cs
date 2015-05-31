using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// HTTP 请求实例
/// </summary>
public class HttpRequest 
{
    public WWW www { get; set; }
    public string url { get; set; } 
    public string error_msg { get; set; }
    public WWWForm form_post { get; set; }

    private System.Action<JPParser> m_callback;

    /// <summary>
    /// 请求开始时间
    /// </summary>
    public System.DateTime request_start{get;set;}

    /// <summary>
    /// 返回Http状态 Read only
    /// </summary>
    public HttpStatus status
    {
        get
        {
            bool timeout = false;
            System.TimeSpan span = System.DateTime.Now.Subtract(request_start);
            if (span.Seconds > HttpSettings.time_out)
            {
                timeout = true;
            }

            if (timeout)
            {
                return HttpStatus.Timeout;
            }
            else if (!string.IsNullOrEmpty(www.error))
            {
                error_msg = www.error;
                return HttpStatus.HttpError;
            }
            else if (www.isDone)
            {
                return HttpStatus.Finish;
            }
            else
            {
                return HttpStatus.Waiting;
            }
        }
    }

    /// <summary>
    /// Http请求
    /// </summary>
    public HttpRequest(string request_url, WWWForm form, System.Action<JPParser> callback)
    {
        //记录时间
        request_start = System.DateTime.Now;

        url = request_url;
        www = new WWW(url, form);

        form_post = form;

        // 获得回调
        m_callback = callback;
    }

}
