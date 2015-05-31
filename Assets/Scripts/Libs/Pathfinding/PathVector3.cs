using UnityEngine;
using System.Collections;

/// <summary>
/// custom vector3
/// </summary>
public class PathVector3
{
    public float x;
    public float y;
    public float z;

    public PathVector3( float x_=0, float y_=0, float z_=0)
    {
        x = x_;
        y = y_;
        z = z_;
    }

    public float[] ToFloat()
    {
        float[] v = new float[3];
        v[0] = x;
        v[1] = y;
        v[2] = z;
        return v;
    }

    public void Set ( float x_, float y_, float z_)
    {
        x = x_;
        y = y_;
        z = z_;
    }

    public void Set( float x_, float z_ )
    {
        x = x_;
        z = z_;
    }

    public void Set(int tilex, int tilez, PathMap map)
    {
        x = map.startx + tilex * map.QUAD_SIZE + map.QUAD_SIZE * 0.5f;
        y = 0;
        z = map.startz + tilez * map.QUAD_SIZE + map.QUAD_SIZE * 0.5f;
    }

    public void Set(float[] vecto3)
    {
        if (vecto3==null || vecto3.Length != 3)
            return;
        x = vecto3[0];
        y = vecto3[1];
        z = vecto3[2];
    }

    // 复制
    public void Set(PathVector3 pv)
    {
        x = pv.x;
        y = pv.y;
        z = pv.z;
    }

    /// <summary>
    /// 0 based tile x
    /// </summary>
    public int tx(PathMap map)
    {
        return Mathf.FloorToInt( (x - map.startx)/ map.QUAD_SIZE ) ;
    }
    /// <summary>
    /// 0 based tile z
    /// </summary>
    public int tz(PathMap map)
    {
        return Mathf.FloorToInt( (z - map.startz) / map.QUAD_SIZE);
    }
    /// <summary>
    /// 中心点
    /// </summary>
    public void center(PathMap map)
    {
        int tilex = this.tx(map);
        int tilez = this.tz(map);
        x = map.startx + tilex * map.QUAD_SIZE + map.QUAD_SIZE * 0.5f;
        z = map.startz + tilez * map.QUAD_SIZE + map.QUAD_SIZE * 0.5f;
    }

    public float distanceTo(PathVector3 pv, PathMap map)
    {
        return PathHelper.TileDistance(this, pv, map);
    }

}
