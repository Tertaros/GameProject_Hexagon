﻿using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh_Script : MonoBehaviour
{

    Mesh hexMesh;

    [NonSerialized] List<Vector3> vertices;
    [NonSerialized] List<Color> colors;
    [NonSerialized] List<int> triangles;
    [NonSerialized] List<Vector2> uvs;
    [NonSerialized] List<Vector2> uv2s;
    [NonSerialized] List<Vector3> terrainTypes;

    MeshCollider meshCollider;
    public Material vertexColorMat = null;

    public bool useCollider;
    public bool useColors;
    public bool useUVCoordinates;
    public bool useUV2Coordinates;
    public bool useTerrainTypes;

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        if (useCollider)
            meshCollider = gameObject.AddComponent<MeshCollider>();

        hexMesh.name = "Hex_Mesh";

        //if (vertexColorMat == null)
        //{
        //    Debug.LogError("Hex Mesh Material not assigned!");
        //}
        //else
        //    this.GetComponent<MeshRenderer>().material = vertexColorMat;
    }

    public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(HexMetrics_Script.Perturb(v1));
        vertices.Add(HexMetrics_Script.Perturb(v2));
        vertices.Add(HexMetrics_Script.Perturb(v3));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);

    }
    public void AddTriangleColor(Color color1, Color color2, Color color3)
    {
        colors.Add(color1);
        colors.Add(color2);
        colors.Add(color3);
    }

    public void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }
    public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(HexMetrics_Script.Perturb(v1));
        vertices.Add(HexMetrics_Script.Perturb(v2));
        vertices.Add(HexMetrics_Script.Perturb(v3));
        vertices.Add(HexMetrics_Script.Perturb(v4));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    public void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }

    public void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }

    public void AddQuadColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector3 uv3)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
    }

    public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
        uvs.Add(uv4);
    }
    public void Clear()
    {
        hexMesh.Clear();
        vertices = ListPool<Vector3>.Get();
        if (useColors)
            colors = ListPool<Color>.Get();
        if (useUVCoordinates)
            uvs = ListPool<Vector2>.Get();
        if (useUV2Coordinates)
            uv2s = ListPool<Vector2>.Get();
        if (useTerrainTypes)
            terrainTypes = ListPool<Vector3>.Get();
        triangles = ListPool<int>.Get();
    }

    public void Apply()
    {
        hexMesh.SetVertices(vertices);
        ListPool<Vector3>.Add(vertices);
        if (useColors)
        {
            hexMesh.SetColors(colors);
            ListPool<Color>.Add(colors);
        }
        if (useUVCoordinates)
        {
            hexMesh.SetUVs(0, uvs);
            ListPool<Vector2>.Add(uvs);
        }
        if (useUV2Coordinates)
        {
            hexMesh.SetUVs(1, uv2s);
            ListPool<Vector2>.Add(uv2s);
        }
        if(useTerrainTypes)
        {
            hexMesh.SetUVs(2, terrainTypes);
            ListPool<Vector3>.Add(terrainTypes);
        }
        hexMesh.SetTriangles(triangles, 0);
        ListPool<int>.Add(triangles);
        hexMesh.RecalculateNormals();

        if (useCollider)
            meshCollider.sharedMesh = hexMesh;
    }

    public void AddQuadUV(float uMin, float uMax, float vMin, float vMax)
    {
        uvs.Add(new Vector2(uMin, vMin));
        uvs.Add(new Vector2(uMax, vMin));
        uvs.Add(new Vector2(uMin, vMax));
        uvs.Add(new Vector2(uMax, vMax));
    }

    public void AddQuadUnperturbed(Vector3 v1,Vector3 v2,Vector3 v3,Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    public void AddTriangleUV2(Vector2 uv1, Vector2 uv2, Vector3 uv3)
    {
        uv2s.Add(uv1);
        uv2s.Add(uv2);
        uv2s.Add(uv3);
    }

    public void AddQuadUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
    {
        uv2s.Add(uv1);
        uv2s.Add(uv2);
        uv2s.Add(uv3);
        uv2s.Add(uv4);
    }

    public void AddQuadUV2(float uMin, float uMax, float vMin, float vMax)
    {
        uv2s.Add(new Vector2(uMin, vMin));
        uv2s.Add(new Vector2(uMax, vMin));
        uv2s.Add(new Vector2(uMin, vMax));
        uv2s.Add(new Vector2(uMax, vMax));
    }

    public void AddTriangleTerrainTypes(Vector3 types)
    {
        terrainTypes.Add(types);
        terrainTypes.Add(types);
        terrainTypes.Add(types);
    }

    public void AddQuadTerrainTypes(Vector3 types)
    {
        terrainTypes.Add(types);
        terrainTypes.Add(types);
        terrainTypes.Add(types);
        terrainTypes.Add(types);
    }
}

//    public bool useCollider, useColors, useUVCoordinates;

//    [NonSerialized] List<Vector3> vertices;
//    [NonSerialized] List<Color> colors;
//    [NonSerialized] List<Vector2> uvs;
//    [NonSerialized] List<int> triangles;

//    Mesh hexMesh;
//    MeshCollider meshCollider;

//    void Awake()
//    {
//        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
//        if (useCollider)
//        {
//            meshCollider = gameObject.AddComponent<MeshCollider>();
//        }
//        hexMesh.name = "Hex Mesh";
//    }

//    public void Clear()
//    {
//        hexMesh.Clear();
//        vertices = ListPool<Vector3>.Get();
//        if (useColors)
//        {
//            colors = ListPool<Color>.Get();
//        }
//        if (useUVCoordinates)
//        {
//            uvs = ListPool<Vector2>.Get();
//        }
//        triangles = ListPool<int>.Get();
//    }

//    public void Apply()
//    {
//        hexMesh.SetVertices(vertices);
//        ListPool<Vector3>.Add(vertices);
//        if (useColors)
//        {
//            hexMesh.SetColors(colors);
//            ListPool<Color>.Add(colors);
//        }
//        if (useUVCoordinates)
//        {
//            hexMesh.SetUVs(0, uvs);
//            ListPool<Vector2>.Add(uvs);
//        }
//        hexMesh.SetTriangles(triangles, 0);
//        ListPool<int>.Add(triangles);
//        hexMesh.RecalculateNormals();
//        if (useCollider)
//        {
//            meshCollider.sharedMesh = hexMesh;
//        }
//    }

//    public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
//    {
//        int vertexIndex = vertices.Count;
//        vertices.Add(HexMetrics_Script.Perturb(v1));
//        vertices.Add(HexMetrics_Script.Perturb(v2));
//        vertices.Add(HexMetrics_Script.Perturb(v3));
//        triangles.Add(vertexIndex);
//        triangles.Add(vertexIndex + 1);
//        triangles.Add(vertexIndex + 2);
//    }

//    public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
//    {
//        int vertexIndex = vertices.Count;
//        vertices.Add(v1);
//        vertices.Add(v2);
//        vertices.Add(v3);
//        triangles.Add(vertexIndex);
//        triangles.Add(vertexIndex + 1);
//        triangles.Add(vertexIndex + 2);
//    }

//    public void AddTriangleColor(Color color)
//    {
//        colors.Add(color);
//        colors.Add(color);
//        colors.Add(color);
//    }

//    public void AddTriangleColor(Color c1, Color c2, Color c3)
//    {
//        colors.Add(c1);
//        colors.Add(c2);
//        colors.Add(c3);
//    }

//    public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector3 uv3)
//    {
//        uvs.Add(uv1);
//        uvs.Add(uv2);
//        uvs.Add(uv3);
//    }

//    public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
//    {
//        int vertexIndex = vertices.Count;
//        vertices.Add(HexMetrics_Script.Perturb(v1));
//        vertices.Add(HexMetrics_Script.Perturb(v2));
//        vertices.Add(HexMetrics_Script.Perturb(v3));
//        vertices.Add(HexMetrics_Script.Perturb(v4));
//        triangles.Add(vertexIndex);
//        triangles.Add(vertexIndex + 2);
//        triangles.Add(vertexIndex + 1);
//        triangles.Add(vertexIndex + 1);
//        triangles.Add(vertexIndex + 2);
//        triangles.Add(vertexIndex + 3);
//    }

//    public void AddQuadColor(Color color)
//    {
//        colors.Add(color);
//        colors.Add(color);
//        colors.Add(color);
//        colors.Add(color);
//    }

//    public void AddQuadColor(Color c1, Color c2)
//    {
//        colors.Add(c1);
//        colors.Add(c1);
//        colors.Add(c2);
//        colors.Add(c2);
//    }

//    public void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
//    {
//        colors.Add(c1);
//        colors.Add(c2);
//        colors.Add(c3);
//        colors.Add(c4);
//    }

//    public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector3 uv3, Vector3 uv4)
//    {
//        uvs.Add(uv1);
//        uvs.Add(uv2);
//        uvs.Add(uv3);
//        uvs.Add(uv4);
//    }

//    public void AddQuadUV(float uMin, float uMax, float vMin, float vMax)
//    {
//        uvs.Add(new Vector2(uMin, vMin));
//        uvs.Add(new Vector2(uMax, vMin));
//        uvs.Add(new Vector2(uMin, vMax));
//        uvs.Add(new Vector2(uMax, vMax));
//    }
//}
