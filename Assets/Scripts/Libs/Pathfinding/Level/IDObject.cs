using UnityEngine;
using System.Collections;

public class IDObject :  MonoBehaviour{

    public int id = 1;
}

public class IDObjectComparer : System.Collections.Generic.IComparer<IDObject>
{
    int System.Collections.Generic.IComparer<IDObject>.Compare(IDObject a, IDObject b)
    {
        return a.id.CompareTo(b.id);
    }
}

