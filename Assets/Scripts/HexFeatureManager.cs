using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexFeatureManager : MonoBehaviour {


    public HexFeatureCollection[] urbanPrefabs;
    public HexFeatureCollection[] farmPrefabs;
    public HexFeatureCollection[] plantPrefabs;

    public HexMesh_Script walls;
    Transform container;

    public Transform wallTower;
    public Transform bridge;
    public Transform[] specials;

    public void Clear()
    {
        if (container)
            Destroy(container.gameObject);

        container = new GameObject("Features Container").transform;
        container.SetParent(transform, false);
        walls.Clear();
    }

    public void Apply()
    {
        walls.Apply();
    }

    public void AddFeature(HexCell_Script cell, Vector3 position)
    {
        if (cell.IsSpecial)
            return;

        HexHash hash = HexMetrics_Script.SampleHashGrid(position);
        //if (hash.a >= cell.UrbanLevel*0.25f)
        //    return;
        //Transform instance = Instantiate(urbanPrefabs[cell.UrbanLevel-1]);
        Transform prefab = PickPrefab(urbanPrefabs, cell.UrbanLevel, hash.a,hash.d);
        Transform otherprefab = PickPrefab(farmPrefabs, cell.FarmLevel, hash.b, hash.d);

        float usedHash = hash.a;
        if (prefab)
        {
            if (otherprefab && hash.b < hash.a)
            {
                prefab = otherprefab;
                usedHash = hash.b;
            }
               
        }
        else if (otherprefab)
        {
            prefab = otherprefab;
            usedHash = hash.b;
        }
        otherprefab = PickPrefab(plantPrefabs, cell.PlantLevel, hash.c, hash.d);
        if (prefab)
        {
            if (otherprefab && hash.c < usedHash)
            {
                prefab = otherprefab;
            }
        }
        else if (otherprefab)
        {
            prefab = otherprefab;
        }
        else
            return;

        Transform instance = Instantiate(prefab);
        
        position.y += instance.localScale.y * 0.5f;
        instance.localPosition = HexMetrics_Script.Perturb(position);
        instance.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
        instance.SetParent(container, false);
    }

    Transform PickPrefab(HexFeatureCollection[] collection,int level, float hash,float choice)
    {
        if(level>0)
        {
            float[] tresholds = HexMetrics_Script.GetFeatureTresholds(level - 1);
            for(int i=0;i<tresholds.Length;i++)
            {
                if(hash<tresholds[i])
                {
                    return collection[i].Pick(choice);
                }
            }
        }
        return null;
    }

    public void AddWall(EdgeVertices near, HexCell_Script nearCell,
                    EdgeVertices far, HexCell_Script farCell,
                    bool hasRiver,bool hasRoad)
    {
        if(nearCell.Walled!=farCell.Walled&&!nearCell.IsUnderwater&&
            !farCell.IsUnderwater&&nearCell.GetEdgeType(farCell)!=HexEdgeType.Cliff)
        {
            AddWallSegment(near.v1, far.v1, near.v2, far.v2);
            if(hasRiver||hasRoad)
            {
                AddWallCap(near.v2, far.v2);
                AddWallCap(far.v4, near.v4);
            }
            else
            {
                AddWallSegment(near.v2, far.v2, near.v3, far.v3);
                AddWallSegment(near.v3, far.v3, near.v4, far.v4);
            }
            AddWallSegment(near.v4, far.v4, near.v5, far.v5);
        }
    }

    void AddWallSegment(Vector3 nearLeft, Vector3 farLeft, Vector3 nearRight, Vector3 farRight,
                        bool addTower=false)
    {

        nearLeft = HexMetrics_Script.Perturb(nearLeft);
        farLeft = HexMetrics_Script.Perturb(farLeft);
        nearRight = HexMetrics_Script.Perturb(nearRight);
        farRight = HexMetrics_Script.Perturb(farRight);

        Vector3 left = HexMetrics_Script.WallLerp(nearLeft, farLeft);
        Vector3 right = HexMetrics_Script.WallLerp(nearRight, farRight);

        Vector3 leftThicknessOffset = HexMetrics_Script.WallThicknessOffset(nearLeft, farLeft);

        Vector3 rightThicknessOffset = HexMetrics_Script.WallThicknessOffset(nearRight, farRight);

        float leftTop = left.y + HexMetrics_Script.wallHeight;
        float rightTop = right.y + HexMetrics_Script.wallHeight;

        Vector3 v1, v2, v3, v4;
        v1 = left - leftThicknessOffset ;
        v2 = right- rightThicknessOffset;
        v3 = left- leftThicknessOffset;
        v4 = right- rightThicknessOffset;

        v3.y = leftTop;
        v4.y = rightTop;

        walls.AddQuadUnperturbed(v1, v2, v3, v4);
        Vector3 t1 = v3;
        Vector3 t2 = v4;

        v1 = left + leftThicknessOffset;
        v2 = right + rightThicknessOffset;
        v3 = left + leftThicknessOffset;
        v4 = right + rightThicknessOffset;

        v3.y = leftTop;
        v4.y = rightTop;

        walls.AddQuadUnperturbed(v2, v1, v4, v3);
        walls.AddQuadUnperturbed(t1, t2, v3, v4);

        if(addTower)
        {
            Transform towerInstance = Instantiate(wallTower);
            towerInstance.transform.localPosition = (left + right) * 0.5f;
            Vector3 rightDirection = right - left;
            rightDirection.y = 0f;
            towerInstance.transform.right = rightDirection;
            towerInstance.SetParent(container, false);
        }
    }

    void AddWallSegment(Vector3 pivot,HexCell_Script pivotCell,
                        Vector3 left, HexCell_Script leftCell,
                        Vector3 right, HexCell_Script rightCell)
    {
        if (pivotCell.IsUnderwater)
            return;

        bool hasLeftWall = !leftCell.IsUnderwater &&
            pivotCell.GetEdgeType(leftCell) != HexEdgeType.Cliff;
        bool hasRightWall = !rightCell.IsUnderwater &&
            pivotCell.GetEdgeType(rightCell) != HexEdgeType.Cliff;


        if (hasLeftWall)
        {
            if (hasRightWall)
            {
                bool hasTower = false;
                if (leftCell.Elevation == rightCell.Elevation)
                {
                    HexHash hash = HexMetrics_Script.SampleHashGrid((pivot + left + right) * (1f / 3f));

                    hasTower = hash.e < HexMetrics_Script.wallTowerTreshold;
                }
                AddWallSegment(pivot, left, pivot, right, hasTower);
            }
            else if (leftCell.Elevation < rightCell.Elevation)
                AddWallWedge(pivot, left, right);
            else
                AddWallCap(pivot, left);
        }
        else if (hasRightWall)
        {
            if (rightCell.Elevation < leftCell.Elevation)
            {
                AddWallWedge(right, pivot, left);
            }
            else
                AddWallCap(right, pivot);
        }
    }

    public void AddWall(Vector3 c1,HexCell_Script cell1,
                        Vector3 c2,HexCell_Script cell2,
                        Vector3 c3,HexCell_Script cell3)
    {
        if (cell1.Walled)
        {
            if (cell2.Walled)
            {
                if (!cell3.Walled)
                    AddWallSegment(c3, cell3, c1, cell1, c2, cell2);
            }
            else if (cell3.Walled)
                AddWallSegment(c2, cell2, c3, cell3, c1, cell1);
            else
                AddWallSegment(c1, cell1, c2, cell2, c3, cell3);
        }
        else if (cell2.Walled)
        {
            if (cell3.Walled)
                AddWallSegment(c1, cell1, c2, cell2, c3, cell3);
            else
                AddWallSegment(c2, cell2, c3, cell3, c1, cell1);
        }
        else if (cell3.Walled)
            AddWallSegment(c3, cell3, c1, cell1, c2, cell2);
    }

    void AddWallCap(Vector3 near, Vector3 far)
    {
        near = HexMetrics_Script.Perturb(near);
        far = HexMetrics_Script.Perturb(far);

        Vector3 center = HexMetrics_Script.WallLerp(near, far);
        Vector3 thickness = HexMetrics_Script.WallThicknessOffset(near, far);

        Vector3 v1, v2, v3, v4;

        v1 = center - thickness;
        v2 = center + thickness;
        v3 = center - thickness;
        v4 = center + thickness;

        v3.y = center.y + HexMetrics_Script.wallHeight;
        v4.y = center.y + HexMetrics_Script.wallHeight;

        walls.AddQuadUnperturbed(v1, v2, v3, v4);

    }

    void AddWallWedge(Vector3 near, Vector3 far, Vector3 point)
    {
        near = HexMetrics_Script.Perturb(near);
        far = HexMetrics_Script.Perturb(far);
        point = HexMetrics_Script.Perturb(point);

        Vector3 center = HexMetrics_Script.WallLerp(near, far);
        Vector3 thickness = HexMetrics_Script.WallThicknessOffset(near, far);

        Vector3 v1, v2, v3, v4;
        Vector3 pointTop = point;
        point.y = center.y;

        v1 = center - thickness;
        v2 = center + thickness;
        v3 = center - thickness;
        v4 = center + thickness;

        v3.y = center.y + HexMetrics_Script.wallHeight;
        v4.y = center.y + HexMetrics_Script.wallHeight;
        pointTop.y = center.y + HexMetrics_Script.wallHeight;
        
        walls.AddQuadUnperturbed(v1, point, v3, pointTop);
        walls.AddQuadUnperturbed(point, v2, pointTop, v4);
        walls.AddTriangleUnperturbed(pointTop, v3, v4);
    }

    public void AddBridge(Vector3 roadCenter1, Vector3 roadCenter2)
    {
        roadCenter1 = HexMetrics_Script.Perturb(roadCenter1);
        roadCenter2 = HexMetrics_Script.Perturb(roadCenter2);

        Transform instance = Instantiate(bridge);
        instance.localPosition = (roadCenter1 + roadCenter2) * 0.5f;
        instance.forward = roadCenter2 - roadCenter1;
        float length = Vector3.Distance(roadCenter1, roadCenter2);
        instance.localScale = new Vector3(1, 1, length * (1f / HexMetrics_Script.bridgeDesignLength));
        instance.SetParent(container, false);
    }

    public void AddSpecialFeature(HexCell_Script cell, Vector3 position)
    {
        Transform instance = Instantiate(specials[cell.SpecialIndex - 1]);
        instance.localPosition = HexMetrics_Script.Perturb(position);
        HexHash hash = HexMetrics_Script.SampleHashGrid(position);
        instance.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
        instance.SetParent(container, false);
    }
}
