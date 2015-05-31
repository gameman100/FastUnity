using System.Collections;

public class PathHelper {

    /// <summary>
    /// 角度转换
    /// </summary>
    public const float PI = 3.1415926f;
    public const float RadToDeg = 57.2957795056f;
    public const float DegToRad = PI / 180;

    /// <summary>
    /// 距离（不包括y轴)
    /// </summary>
    public static float Distance3(PathVector3 vec1, PathVector3 vec2)
    {
        return (float)System.Math.Sqrt((vec1.x - vec2.x) * (vec1.x - vec2.x) + (vec1.z - vec2.z) * (vec1.z - vec2.z));
    }

    /// <summary>
    /// 距离（不包括y轴)
    /// </summary>
    public static float TileDistance(PathVector3 vec1, PathVector3 vec2, PathMap map)
    {
        /*
        vec1.x = (float)System.Math.Floor(vec1.x);
        vec1.y = 0;
        vec1.z = (float)System.Math.Floor(vec1.z);

        vec2.x = (float)System.Math.Floor(vec2.x);
        vec2.y = 0;
        vec2.z = (float)System.Math.Floor(vec2.z);
        */
        //return (float)System.Math.Sqrt((vec1.x - vec2.x) * (vec1.x - vec2.x) + (vec1.z - vec2.z) * (vec1.z - vec2.z));
        int tx1 = vec1.tx(map);
        int tz1 = vec1.tz(map);
        int tx2 = vec2.tx(map);
        int tz2 = vec2.tz(map);
        return System.Math.Max(System.Math.Abs(tx1 - tx2), System.Math.Abs(tz1 - tz2));
    }

    /// <summary>
    /// 距离（不包括y轴),float version, only for compare
    /// </summary>
    public static float Distance3Fast(PathVector3 vec1, PathVector3 vec2)
    {
        return (vec1.x - vec2.x) * (vec1.x - vec2.x) + (vec1.z - vec2.z) * (vec1.z - vec2.z);
    }

    /// <summary>
    /// 两个矢量位置间的角度
    /// </summary>
    /// <param name="pos1">位置坐标1</param>
    /// <param name="pos2">位置坐标2</param>
    /// <returns>返回角度</returns>
    public static float PolarAngle2(PathVector3 pos1, PathVector3 pos2)
    {
        float vx = pos2.x - pos1.x;
        float vy = pos2.z - pos1.z;

        float mag = (float)System.Math.Sqrt(vx * vx + vy * vy);

        if (mag == 0)
            return 0;

        float angle = RadToDeg * (float)System.Math.Asin(vx / mag);

        if (vy < 0)
        {
            angle -= 180;
            angle *= -1;

        }
        else if (vx < 0 && vy > 0)
        {
            angle += 360;
        }

        return angle;
    }


    /// <summary>
    /// 计算两个角度间的最小角度
    /// </summary>
    /// <param name="current">当前角度</param>
    /// <param name="target">目标角度</param>
    /// <returns>返回角度</returns>
    public static float DeltaAngle(float current, float target)
    {
        float angle = 0;

        if (current > target)
        {
            angle = current - target;
            if (angle < 180)
            {
                return -angle;
            }
            else
            {
                return (360 - angle);
            }
        }
        else
        {
            angle = target - current;
            if (angle < 180)
            {
                return angle;
            }
            else
            {
                return -(360 - angle);
            }
        }

    }

    /// <summary>
    /// 将角度转为0-360之间
    /// </summary>
    /// <param name="angle">输入值</param>
    /// <returns>返回0-360之间的角度</returns>
    public static float WrapAngle(float angle)
    {
        if (angle >= 360)
        {
            return angle - (360 * (int)(angle / 360));
        }
        else if (angle < 0)
        {
            return angle - (360 * (int)(angle / 360)) + 360;
        }
        return angle;
    }

    /// <summary>
    /// 计算两个角度之间的差(0-360)
    /// </summary>
    /// <param name="angle1">角度1</param>
    /// <param name="angle2">角度2</param>
    /// <returns>返回两个角度之间的夹角</returns>
    public float AngleBetween(float angle1, float angle2)
    {
        return (WrapAngle(angle1 + 180 - angle2)) - 180;
    }
}
