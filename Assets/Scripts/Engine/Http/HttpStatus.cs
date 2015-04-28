using UnityEngine;
using System.Collections;

/// <summary>
/// Http通信状态
/// </summary>
public enum HttpStatus
{
    None = 0,
    /// <summary>
    /// 等待
    /// </summary>
    Waiting = 1,
    /// <summary>
    /// Http错误(目标主机地址不存在)
    /// </summary>
    HttpError = 2,
    /// <summary>
    /// 响应错误(协议问题等)
    /// </summary>
    ReponseError = 3,
    /// <summary>
    /// 连接超时
    /// </summary>
    Timeout = 4,
    /// <summary>
    /// 成功返回
    /// </summary>
    Finish = 5,
	
}
