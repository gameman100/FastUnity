using UnityEngine;
using System.Collections;

[System.Serializable]
public class PathMap
{
    /// <summary>
    /// ÿ�������ڿɳ��ܵ�����С,ʵ��Ĵ�С0,1,2,3,4
    /// </summary>
    public static int MAX_SIZE =1005;
    /// <summary>
    /// ÿ�����ӵĴ�С
    /// </summary>
    public float QUAD_SIZE { get { return m_quad_size; } }
    protected float m_quad_size = 1;

    private float m_startx = 0;
    public float startx { get { return m_startx; } }
    private float m_startz = 0;
    public float startz { get { return m_startz; } }

    private int m_sizeX = 128;
    public int SizeX{get{return m_sizeX;}}
    private int m_sizeZ = 128;
    public int SizeZ{get{return m_sizeZ;}}

    public float boundx { get { return startx + m_sizeX; } }
    public float boundz { get { return startz + m_sizeZ; } }
    /// <summary>
    /// ��ͼ��Ϣ
    /// </summary>
    public QuadData[,] m_Data;

        
    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="sizex"> ��ͼX�����С </param>
    /// <param name="sizez"> ��ͼZ�����С </param>
    public PathMap( int sizex ,int sizez, float start_x=0, float start_z=0, float quadsize =1 )
    {
        m_sizeX = sizex;
        m_sizeZ = sizez;
        m_startx = start_x;
        m_startz = start_z;
        m_quad_size = quadsize;

        m_Data = new QuadData[sizex, sizez];

        for (int i = 0; i < m_sizeX; i++)
        {
            for (int k = 0; k < m_sizeZ; k++)
            {
                m_Data[i, k] = new QuadData();
                m_Data[i, k].data = null;
                m_Data[i, k].size = 0;
                m_Data[i, k].block = 0;
                m_Data[i, k].dynamicID = 0;
                m_Data[i, k].movespeed = 0;  
            }
        }
    }

        /// <summary>
        /// �����ͼ��Ϣ
        /// </summary>
        public void Clear()
        {
            m_Data = null;
        }

        public void SetBlock( PathVector3 v, int entrySize, int dynamic, float movespeed)
        {
            SetBlock(v.x, v.z, entrySize, dynamic, movespeed);
        }

        /// <summary>
        /// ���õ�ͼ�ڵ�״̬
        /// </summary>
        /// <param name="x">����λ��X</param>
        /// <param name="z">����λ��Z</param>
        /// <param name="entrySize">�������Χ�γɵ�ѹ��</param>
        /// <param name="dynamic">���id</param>
        public void SetBlock( float x ,float z ,int entitysize ,int dynamic ,float movespeed)
        {
            int ix = Mathf.FloorToInt( (x - m_startx) / m_quad_size );
            int iz = Mathf.FloorToInt( (z - m_startz) / m_quad_size );

            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ )
            {
                m_Data[(int)ix, (int)iz].block ++;
                m_Data[(int)ix, (int)iz].dynamicID += dynamic;
                m_Data[(int)ix, (int)iz].size += entitysize;
                m_Data[(int)ix, (int)iz].movespeed += movespeed;

                if (entitysize == 0)
                {
                    return;
                }

                for (int i = ix - 1; i < ix + 2; i++)
                {
                    for (int k = iz - 1; k < iz + 2; k++)
                    {
                        if (i >= 0 && k >= 0 && i < SizeX && k < SizeZ)
                        {
                            if (i == ix && k == iz)
                                continue;
                            m_Data[i, k].size += entitysize;

                        }
                    }
                }
            }
        }

        public void RemoveBlock(PathVector3 v, int entitySize, int dy, float mv)
        {
            RemoveBlock(v.x, v.z, entitySize, dy, mv);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="entrySize"></param>
        public void RemoveBlock(float x, float z , int entitysize, int dy, float mv)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);

            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                // �⿪��ǰ������
                m_Data[ix, iz].block --;
                m_Data[ix, iz].dynamicID -= dy;
                m_Data[ix, iz].movespeed -= mv;
                // ��ǰ���С��Ϊ��
                m_Data[ix, iz].size -= entitysize;

                if (m_Data[ix, iz].block < 0)
                    Debug.LogError("<0");

                if (entitysize == 0)
                    return;

                // ���ҵ�ǰ����ܱ�8���� [i,k]
                for (int i = ix - 1; i < ix + 2; i++)
                {
                    for (int k = iz - 1; k < iz + 2; k++)
                    {
                        if (i >= 0 && k >= 0 && i < SizeX && k < SizeZ)
                        {
                            // �ų���ǰ��
                            if (i == ix && k == iz)
                                continue;

                            m_Data[i, k].size -= entitysize;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// [deprecated]�Ƿ�block
        /// </summary>
        /// <param name="tilex">0 base tile x</param>
        /// <param name="tilez">0 base tile z</param>
        public bool IsBlocked(int tilex, int tilez, int entrySize)
        {
            int ix = tilex;
            int iz = tilez;
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ) // ���߽�
            {
                if (m_Data[ix, iz].block>0 || m_Data[ix, iz].size + entrySize >= MAX_SIZE)
                {
                    return true;
                }
            }
            else
                return true;
            return false;
        }

        /// <summary>
        /// [deprecated]�жϵ�ͼ�ڵ��Ƿ����
        /// </summary>
        /// <param name="worldx">����λ��X(world position, may not 0 based)</param>
        /// <param name="worldz">����λ��Z</param>
        /// <param name="entitysize">ʵ������</param>
        /// <returns>true��ʾ����</returns>
        public bool IsBlocked2(float worldx, float worldz, int entitysize )
        {
            int ix = Mathf.FloorToInt((worldx - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((worldz - m_startz) / m_quad_size);

            return IsBlocked(ix, iz, entitysize);
        }

        /// <summary>
        /// �Ƿ����ͨ��
        /// </summary>
        /// <param name="tilex">0 base tile x</param>
        /// <param name="tilez">0 base tile z</param>
        public bool IsFree(int tilex, int tilez, int entrySize)
        {
            int ix = tilex;
            int iz = tilez;
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ) // ���߽�
            {
                if (m_Data[ix, iz].block > 0 || m_Data[ix, iz].size + entrySize >= MAX_SIZE)
                {
                    return false;
                }
            }
            else
                return false;
            return true;
        }

        /// <summary>
        /// �жϵ�ͼ�ڵ��Ƿ����ͨ��
        /// </summary>
        /// <param name="worldx">����λ��X(world position, may not 0 based)</param>
        /// <param name="worldz">����λ��Z</param>
        /// <param name="entitysize">ʵ������</param>
        /// <returns>false��ʾ����</returns>
        public bool IsFree2(float worldx, float worldz, int entitysize)
        {
            int ix = Mathf.FloorToInt((worldx - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((worldz - m_startz) / m_quad_size);

            return !IsBlocked(ix, iz, entitysize);
        }

        /// <summary>
        /// ���õ�ͼ�ڵ�����
        /// </summary>
        /// <param name="x">����λ��X</param>
        /// <param name="z">����λ��Z</param>
        /// <param name="data">����</param>
        public void SetData(float x, float z, System.Object data)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                m_Data[(int)ix, (int)iz].data = data;
            }
        }

        /// <summary>
        /// ȡ�õ�ͼ�ڵ������
        /// </summary>
        /// <param name="x">����λ��X</param>
        /// <param name="z">����λ��Z</param>
        /// <returns>����null��ʾ������Ч</returns>
        public System.Object GetData(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                return m_Data[(int)ix, (int)iz].data;
            }
            else
                return null;
        }

        /// <summary>
        /// ���ð���һ���ƶ���
        /// </summary>
        /// <param name="x">����λ��X</param>
        /// <param name="z">����λ��Z</param>
        /// <param name="targetx">����λ��X</param>
        /// <param name="targetz">����λ��Z</param>
        internal void SetDynamic(float x, float z, int dynamic)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                m_Data[(int)ix, (int)iz].dynamicID = dynamic;

             
            }
        }

        /// <summary>
        /// ȥ���ƶ������Ϣ
        /// </summary>
        /// <param name="x">����X</param>
        /// <param name="z">����Z</param>
        internal void RemoveDynamic(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                m_Data[(int)ix, (int)iz].dynamicID = 0;
            }
        }

        /// <summary>
        /// �Ƿ����һ���ƶ���
        /// </summary>
        /// <param name="x">����λ��X</param>
        /// <param name="z">����λ��Z</param>
        /// <returns>�Ƿ����</returns>
        public bool IsDynamic(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                if (m_Data[(int)ix, (int)iz].dynamicID > 0)
                    return true;
                else
                    return false;
            }

            return false;
        }

        /// <summary>
        /// �Ƿ����һ���ƶ���
        /// </summary>
        /// <param name="x">����λ��X</param>
        /// <param name="z">����λ��Z</param>
        /// <returns>�Ƿ����</returns>
        public int GetDynamic(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                return m_Data[(int)ix, (int)iz].dynamicID;
            }

            return 0;
        }


        public float GetSpeed(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                return m_Data[(int)ix, (int)iz].movespeed;
            }

            return 0;
        }

        /// <summary>
        /// ȡ�õ�ͼ�ڵ�Ĵ�С
        /// </summary>
        /// <param name="x">����λ��X</param>
        /// <param name="z">����λ��Z</param>
        /// <returns>����-1��ʾ������Ч</returns>
        public int GetSize(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                return m_Data[(int)ix, (int)iz].size;
            }
            else
                return -1;
        }



        /// <summary>
        /// ȡ�õ�ͼ�ڵ�����ĵ�����λ��
        /// </summary>
        /// <param name="x">λ��X</param>
        /// <param name="z">λ��Z</param>
        /// <param name="vec">�������Ϊ3�������ʾX Y Z����,����������겻�Ϸ�,���null</param>
        public void GetTileCenter(float x, float z, ref float[] vec)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);

            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                vec = new float[3];
                vec[0] = ix + QUAD_SIZE * 0.5f;
                vec[1] = 0;
                vec[2] = iz + QUAD_SIZE * 0.5f;
            }
            else
            {
                vec = null;
            }

        }

        public bool CheckValid(int tilex, int tilez)
        {
            if (tilex < 0 || tilez < 0 || tilex >= SizeX || tilez >= SizeZ)
                return false;
            else
                return true;

        }

        public bool CheckValid(PathVector3 v)
        {
            if (v.x < startx || v.z < startz || v.x >= boundx || v.z >= boundz)
                return false;
            else
                return true;

        }
  

  

    /// <summary>
    /// ��ͼ��Ϣ
    /// </summary>
    public struct QuadData
    {
        /// <summary>
        /// ����
        /// </summary>
        public System.Object data;
        /// <summary>
        /// ��С
        /// </summary>
        public int size;
        /// <summary>
        /// >0��ռ��
        /// </summary>
        public int block;
        /// <summary>
        /// ��ǰ�����ڰ���һ�����ƶ��������� ,0��ʾ���ƶ�,����0��ʾ�ƶ���С��0��ʾ��ʱֹͣ
        /// </summary>
        public int dynamicID;
        /// <summary>
        /// ��ǰ��������������ƶ��ٶ�
        /// </summary>
        public float movespeed;

 
    }

}
