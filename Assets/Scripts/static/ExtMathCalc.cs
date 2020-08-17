using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtMathCalc
{
    //外積を返す
    public static Vector3 crossVector(Vector3 a, Vector3 b)
    {
        Vector3 c = new Vector3();
        c.x = a.y * b.z - a.z * b.y;
        c.y = a.z * b.x - a.x * b.z;
        c.z = a.x * b.y - a.y * b.x;
        return c;
    }

}
