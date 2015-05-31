using UnityEngine;
using System.Collections;

/// <summary>
/// Path工具类
/// </summary>
public static class GridUtility {

    /// <summary>
    /// 把向量转为长度为3的浮点数组
    /// </summary>
    /// <param name="vec">Unity向量</param>
    /// <returns>返回长度为3的浮点数组</returns>
    public static float[] VectorToFloat(Vector3 vec)
    {
        float[] var = new float[3];
        var[0] = vec.x;
        var[1] = vec.y;
        var[2] = vec.z;

        return var;
    }

    public static PathVector3 VectorToPath(Vector3 vec)
    {
        PathVector3 pv = new PathVector3(vec.x, vec.y, vec.z);
        return pv;
    }

    /// <summary>
    /// 把长度为3的浮点数组转为向量
    /// </summary>
    /// <param name="vec">长度为3的浮点数组</param>
    /// <returns>返回向量</returns>
    public static Vector3 FloatToVector(float[] var)
    {
        return new Vector3(var[0], var[1], var[2]);
    }

    /// <summary>
    /// 把长度为3的浮点数组转为向量
    /// </summary>
    /// <param name="vec">长度为3的浮点数组</param>
    /// <returns>返回向量</returns>
    public static Vector3 FloatToVector(PathVector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    /// <summary>
    /// 是否处于同一个tile中
    /// </summary>
    public static bool IsSameTile(Vector3 pos1, Vector3 pos2, PathMap map )
    {
        PathVector3 v1 = new PathVector3(pos1.x, pos1.y, pos1.z);
        PathVector3 v2 = new PathVector3(pos2.x, pos2.y, pos2.z);

        if (v1.tx(map) == v2.tx(map) && v2.tz(map) == v2.tz(map))
        {
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// 移动到目标
    /// </summary>
    /// <param name="finder"></param>
    /// <param name="entityTransform"></param>
    /// <param name="entityMoveSpeed"></param>
    /// <param name="entityRotSpeed"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public static bool MoveToTarget(PathFinder finder, ref Transform entityTransform, float entityMoveSpeed, float entityRotSpeed )
    {
        if (finder.NodesCount <= 0)
        {
            return false;
        }

        // current pos
        Vector3 mypos = entityTransform.position;
        float oldy = mypos.y;
        mypos.y = 0;
        PathVector3 f = GridUtility.VectorToPath(mypos);

        // current angle y
        float angley = entityTransform.eulerAngles.y;

        // move to
        bool b = finder.MoveToTargetNode(ref f, ref angley, entityMoveSpeed, entityRotSpeed);
        if (b)
        {
            // get new position
            entityTransform.position = FloatToVector(f);

            Vector3 pos = entityTransform.position;
            pos.y = oldy;
            entityTransform.position = pos;

            // get new angle y
            entityTransform.eulerAngles = new Vector3(0, angley, 0);
        }
    

        if (b)
            return true;
        else
            return false;

    }

    static public bool MoveToTargetNoRotation(PathFinder finder, ref Transform entityTransform, float entityMoveSpeed, float entityRotSpeed, float tolerance)
    {
        if (finder.NodesCount <= 0)
            return true;

        // current pos
        Vector3 mypos = entityTransform.position;
        float oldy = mypos.y;
        mypos.y = 0;
        PathVector3 f = GridUtility.VectorToPath(mypos);

        // current angle y
        float angley = entityTransform.eulerAngles.y;

        // move to
        bool b = finder.MoveToTargetNode(ref f, ref angley, entityMoveSpeed, entityRotSpeed);

        // get new position
        entityTransform.position = FloatToVector(f);


        Vector3 pos = entityTransform.position;
        pos.y = oldy;
        entityTransform.position = pos;

        if (b)
            return true;
        else
            return false;


    }
}
