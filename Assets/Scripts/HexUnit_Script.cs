﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HexUnit_Script : MonoBehaviour {

    HexCell_Script location;
    float orientation;
    public static HexUnit_Script unitPrefab;

    List<HexCell_Script> pathToTravel;
    const float travelSpeed = 4f;

    public HexCell_Script Location
    {
        get { return location; }
        set
        {
            if (location)
                location.Unit = null;

            location = value;
            value.Unit = this;
            transform.localPosition = value.Position;
        }
    }

    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public void Die()
    {
        location.Unit = null;
        Destroy(gameObject);
    }

    public void Save(BinaryWriter writer)
    {
        location.coordinates.Save(writer);
        writer.Write(orientation);
    }

    public static void Load(BinaryReader reader,HexGrid_Script grid)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();
        grid.AddUnit(Instantiate(unitPrefab), grid.GetCell(coordinates), orientation);
    }
    
    public bool IsValidDestination(HexCell_Script cell)
    {
        return !cell.IsUnderwater&&!cell.Unit;
    }

    public void Travel(List<HexCell_Script>path)
    {
        Location = path[path.Count - 1];
        pathToTravel = path;
        StopAllCoroutines();
        StartCoroutine(TravelPath());
    }

    void OnDrawGizmos()
    {
        if(pathToTravel==null||pathToTravel.Count==0)
        {
            return;
        }

        Vector3 a, b,c = pathToTravel[0].Position;

        for(int i=1;i<pathToTravel.Count;i++)
        {
            a = c;
            b = pathToTravel[i - 1].Position;
            c = (b + pathToTravel[i].Position) * 0.5f;
           
            for(float t = 0f; t<1f;t+=0.1f)
            {
                Gizmos.DrawSphere(Bezier.GetPoint(a,b,c,t), 2f);
            }

        }

        a = c;
        b = pathToTravel[pathToTravel.Count - 1].Position;
        c = b;
        for (float t = 0f; t < 1f; t += 0.1f)
        {
            Gizmos.DrawSphere(Bezier.GetPoint(a,b,c,t), 2f);
        }
    }

    IEnumerator TravelPath()
    {
        Vector3 a, b, c = pathToTravel[0].Position;
        for (int i = 1; i < pathToTravel.Count; i++)
        {
            a =c;
            b = pathToTravel[i - 1].Position;
            c = (b + pathToTravel[i].Position) * 0.5f; 
            for (float t = 0f; t < 1f; t += Time.deltaTime*travelSpeed)
            {
                transform.localPosition = Bezier.GetPoint(a, b, c, t);
                yield return null;
            }

        }

        a = c;
        b = pathToTravel[pathToTravel.Count - 1].Position;
        c = b;
        for (float t = 0f; t < 1f; t += 0.1f)
        {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            yield return null;
        }
    }

    private void OnEnable()
    {
        if (location)
            transform.localPosition = location.Position;
    }
}
