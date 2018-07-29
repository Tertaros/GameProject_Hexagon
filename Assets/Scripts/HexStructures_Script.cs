using UnityEngine;
using System.Collections.Generic;
public struct EdgeVertices
{
    public Vector3 v1, v2,v3, v4, v5;

    public EdgeVertices(Vector3 corner1,Vector3 corner2)
    {
        v1 = corner1;
        v2 = Vector3.Lerp(corner1, corner2, 0.25f);
        v3 = Vector3.Lerp(corner1, corner2, 0.5f);
        v4 = Vector3.Lerp(corner1, corner2, 0.75f);
        v5 = corner2;
    }

    public static EdgeVertices TerraceLerp(EdgeVertices a,EdgeVertices b,int step)
    {
        EdgeVertices result;

        result.v1 = HexMetrics_Script.TerraceLerp(a.v1, b.v1, step);
        result.v2 = HexMetrics_Script.TerraceLerp(a.v2, b.v2, step);
        result.v3 = HexMetrics_Script.TerraceLerp(a.v3, b.v3, step);
        result.v4 = HexMetrics_Script.TerraceLerp(a.v4, b.v4, step);
        result.v5 = HexMetrics_Script.TerraceLerp(a.v5, b.v5, step);
        return result;
    }

    public EdgeVertices(Vector3 corner1,Vector3 corner2,float outerStep)
    {
        v1 = corner1;
        v2 = Vector3.Lerp(corner1, corner2, outerStep);
        v3 = Vector3.Lerp(corner1, corner2, 0.5f);
        v4 = Vector3.Lerp(corner1, corner2, 1f- outerStep);
        v5 = corner2;
    }
}

public static class ListPool<T>
{
    static Stack<List<T>> stack = new Stack<List<T>>();

    public static List<T> Get()
    {
        if (stack.Count > 0)
            return stack.Pop();
        return new List<T>();
    }

    public static void Add(List<T>list)
    {
        list.Clear();
        stack.Push(list);
    }
}

public struct HexHash
{
    public float a, b,c,d,e;
    public static HexHash Create()
    {
        HexHash hash;
        hash.a = Random.value*0.999f;
        hash.b = Random.value*0.999f;
        hash.c = Random.value*0.999f;
        hash.d = Random.value * 0.999f;
        hash.e = Random.value * 0.999f;
        return hash;
    }
}

[System.Serializable]
public struct HexFeatureCollection
{
    public Transform[] prefabs;

    public Transform Pick(float choice)
    {
        return prefabs[(int)(choice * prefabs.Length)];
    }
}