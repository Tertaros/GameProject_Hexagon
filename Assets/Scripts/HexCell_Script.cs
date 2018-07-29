
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class HexCell_Script : MonoBehaviour {

    public HexCoordinates coordinates;

    //Color color;
    int terrainTypeIndex;

    private int elevation = int.MinValue;

    public RectTransform uiRect;

    public HexGridChunk_Script chunk;

    [SerializeField]
    HexCell_Script[] neighbors;

    [SerializeField]
    bool[] roads;

    int waterLevel;

    bool hasIncomingRiver, hasOutgoingRiver;

    HexDirection incomingRiver;
    HexDirection outgoingRiver;

    int urbanLevel;
    int farmLevel;
    int plantLevel;

    bool walled;

    int specialIndex;

    int distance;

    public bool HasIncomingRiver
    {
        get { return hasIncomingRiver; }
    }
    public bool HasOutgoingRiver
    {
        get { return hasOutgoingRiver; }
    }

    public HexDirection IncomingRiver
    {
        get { return incomingRiver; }
    }

    public HexDirection OutgoingRiver
    {
        get { return outgoingRiver; }
    }

    public bool HasRiver
    {
        get { return hasIncomingRiver || hasOutgoingRiver; }
    }

    public bool HasRiverBeginOrEnd
    {
        get { return hasIncomingRiver != hasOutgoingRiver; }
    }

    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return hasIncomingRiver && incomingRiver == direction ||
               hasOutgoingRiver && outgoingRiver == direction;
    }
    public Vector3 Position
    {
        get { return transform.localPosition; }
    }

    //public Color Color
    //{
    //    get { return HexMetrics_Script.colors[terrainTypeIndex]; }

    //}


    public float StreamBedY
    {
        get { return(elevation+HexMetrics_Script.streamBedElevationOffset)*HexMetrics_Script.elevationStep;}
    }

    public float RiverSurfaceY
    {
        get
        {
            return (elevation + HexMetrics_Script.waterElevationOffset) * HexMetrics_Script.elevationStep;
        }
    }

    public float WaterSurfaceY
    {
        get
        {
            return (waterLevel + HexMetrics_Script.waterElevationOffset) * HexMetrics_Script.elevationStep;
        }
    }
    public int Elevation
    {
        get { return elevation; }
        set
        {
            if (elevation == value)
                return;
            elevation = value;
            RefreshPosition();

            //if (hasOutgoingRiver && elevation < GetNeighbor(outgoingRiver).elevation)
            //    RemoveOutgoingRiver();
            //if (hasIncomingRiver && elevation > GetNeighbor(incomingRiver).elevation)
            //    RemoveIncomingRiver();

            ValidateRivers();

            for(int i=0;i<roads.Length;i++)
            {
                if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                    SetRoad(i, false);
            }

            Refresh();
        }
    }

    public HexDirection RiverBeginOrEndDirection
    {
        get
        {
            return hasIncomingRiver ? incomingRiver : outgoingRiver;
        }
    }

    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if (waterLevel == value)
                return;

            waterLevel = value;
            ValidateRivers();
            Refresh();
        }
    }

    public bool IsUnderwater
    {
        get
        {
            return waterLevel > elevation;
        }
    }

    public HexCell_Script GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell_Script cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics_Script.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }

    public HexEdgeType GetEdgeType(HexCell_Script otherCell)
    {
        return HexMetrics_Script.GetEdgeType(elevation, otherCell.elevation);
    }

    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for(int i=0;i<neighbors.Length;i++)
            {
                HexCell_Script neighbor= neighbors[i];
                if (neighbor != null && neighbor.chunk!=chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }
    void RefreshSelfOnly()
    {
        chunk.Refresh();
    }
    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver)
            return;

        hasOutgoingRiver = false;
        RefreshSelfOnly();

        HexCell_Script neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver)
            return;
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell_Script neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoverRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    public void SetOutgoingRiver(HexDirection direction)
    {
        if (HasOutgoingRiver && outgoingRiver == direction)
            return;

        HexCell_Script neighbor = GetNeighbor(direction);
        if (!IsValidRiverDestination(neighbor))
            return;

        RemoveOutgoingRiver();
        if(hasIncomingRiver && incomingRiver==direction)
        {
            RemoveIncomingRiver();
        }

        hasOutgoingRiver = true;
        outgoingRiver = direction;
        specialIndex = 0;
        //RefreshSelfOnly();

        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        neighbor.specialIndex = 0;
        //neighbor.RefreshSelfOnly();

        SetRoad((int)direction, false);
    }

    public bool HasRoadThroughEdge(HexDirection direction)
    {
        return roads[(int)direction];
    }

    public bool HasRoads
    {
        get
        {
            for(int i=0;i<roads.Length;i++)
            {
                if (roads[i])
                    return true;
            }
            return false;
        }
    }

    public void RemoveRoads()
    {
        for(int i=0; i<neighbors.Length;i++)
        {
            if (roads[i])
            {
                SetRoad(i, false);
            }                
        }
    }

    public void AddRoad(HexDirection direction)
    {
        if (!roads[(int)direction]&&!HasRiverThroughEdge(direction)&&
            GetElevationDifference(direction)<=1&&
            !IsSpecial && !GetNeighbor(direction).IsSpecial)
            SetRoad((int)direction, true);
    }

    private void SetRoad(int index, bool state)
    {
        roads[index] = state;
        neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
        neighbors[index].RefreshSelfOnly();
        RefreshSelfOnly();
    }

    public int GetElevationDifference(HexDirection direction)
    {
        int difference = elevation - GetNeighbor(direction).elevation;
        return difference >= 0 ? difference : -difference;
    }

    bool IsValidRiverDestination(HexCell_Script neighbor)
    {
        return neighbor && (elevation >= neighbor.elevation || waterLevel == neighbor.elevation);

    }

    void ValidateRivers()
    {
        if(hasOutgoingRiver&&!IsValidRiverDestination(GetNeighbor(outgoingRiver)))
        {
            RemoveOutgoingRiver();
        }
        if(hasIncomingRiver&&!GetNeighbor(incomingRiver).IsValidRiverDestination(this))
        {
            RemoveIncomingRiver();
        }
    }

    public int UrbanLevel
    {
        get
        {
            return urbanLevel;
        }
        set
        {
            if(urbanLevel!=value)
            {
                urbanLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    public int FarmLevel
    {
        get
        {
            return farmLevel;
        }
        set
        {
            if(farmLevel!=value)
            {
                farmLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    public int PlantLevel
    {
        get
        {
            return plantLevel;
        }
        set
        {
            if (plantLevel != value)
            {
                plantLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    public bool Walled
    {
        get
        {
            return walled;
        }
        set
        {
            if (walled != value)
            {
                walled = value;
                Refresh();
            }
        }
    }

    public int SpecialIndex
    {
        get { return specialIndex; }
        set
        {
            if (specialIndex != value&&!HasRiver)
            {
                specialIndex = value;
                RemoveRoads();
                RefreshSelfOnly();
            }
        }
    }

    public bool IsSpecial
    {
        get { return specialIndex > 0; }
    }

    public int TerrainTypeIndex
    {
        get { return terrainTypeIndex; }
        set
        {
            if (terrainTypeIndex != value)
            {
                terrainTypeIndex = value;
                Refresh();
            }
        }
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)terrainTypeIndex);
        writer.Write((byte)elevation+127);
        writer.Write((byte)waterLevel);
        writer.Write((byte)urbanLevel);
        writer.Write((byte)farmLevel);
        writer.Write((byte)plantLevel);
        writer.Write((byte)specialIndex);

        writer.Write(walled);
        if (hasIncomingRiver)
            writer.Write((byte)(incomingRiver + 128));
        else
            writer.Write((byte)0);
        if (hasOutgoingRiver)
            writer.Write((byte)(outgoingRiver + 128));
        else
            writer.Write((byte)0);

        int roadFlags = 0;
        for (int i = 0; i < roads.Length; i++)
        {
            if (roads[i])
                roadFlags |= 1 << i;
        }
        writer.Write((byte)roadFlags);
    }

    public void Load(BinaryReader reader)
    {
        terrainTypeIndex = reader.ReadByte();
        elevation = reader.ReadByte();
        RefreshPosition();
        waterLevel = reader.ReadByte();
        urbanLevel = reader.ReadByte();
        farmLevel = reader.ReadByte();
        plantLevel = reader.ReadByte();
        specialIndex = reader.ReadByte();

        walled = reader.ReadBoolean();
        byte riverData = reader.ReadByte();
        if (riverData >= 128)
        {
            hasIncomingRiver = true;
            incomingRiver = (HexDirection)(riverData - 128);
        }
        else
            hasIncomingRiver = false;

        riverData = reader.ReadByte();
        if (riverData >= 128)
        {
            hasOutgoingRiver = true;
            outgoingRiver = (HexDirection)(riverData - 128);
        }
        else
            hasOutgoingRiver = false;

        int roadFlags = reader.ReadByte();
        for (int i =0; i<roads.Length;i++)
        {
            roads[i] = (roadFlags & (1 << i)) != 0;
        }
    }

    void RefreshPosition()
    {
        Vector3 position = transform.localPosition;
        position.y = elevation * HexMetrics_Script.elevationStep;
        position.y += (HexMetrics_Script.SampleNoise(position).y * 2f - 1f) *
                    HexMetrics_Script.elevationPerturbStrength;
        transform.localPosition = position;

        Vector3 uiPosition = uiRect.localPosition;
        uiPosition.z = -position.y;
        uiRect.localPosition = uiPosition;
    }

    //void UpdateDistanceLabel()
    //{
    //    Text label = uiRect.GetComponent<Text>();
    //    label.text = distance==int.MaxValue ?"":distance.ToString();
    //}

    public int Distance
    {
        get
        {
            return distance;
        }
        set
        {
            distance = value;
            //UpdateDistanceLabel();
        }
    }

    public void DisableHighlight()
    {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.enabled = false;
    }

    public void EnableHighlight(Color color)
    {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.color = color;
        highlight.enabled = true;
    }

    public HexCell_Script PathFrom { get; set; }

    public int SearchHeuristic { get; set; }

    public int SearchPriority
    {
        get
        {
            return distance + SearchHeuristic;
        }
    }

    public HexCell_Script NextWithSamePriority { get; set; }

    public void SetLabel(string text)
    {
        UnityEngine.UI.Text label = uiRect.GetComponent<Text>();
        label.text=text;
    }

    public int SearchPhase { get; set; }
}
