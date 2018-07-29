using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class HexGrid_Script : MonoBehaviour {

    public int cellCountX = 20;
    public int cellCountZ = 15;

    int chunkCountX;
    int chunkCountZ;

    public HexCell_Script cellPrefab;
    private HexCell_Script[] cells;

    public Text cellLabelPrefab;
    //Canvas gridCanvas;

    //HexMesh_Script hexMesh;

    //public Color defaultColor = Color.white;

    public Texture2D noiseSource;

    public HexGridChunk_Script chunkPrefab;
    HexGridChunk_Script[] chunks;

    public int seed;

    HexCellPriorityQueue searchFrontier;
    int searchFrontierPhase;
    HexCell_Script currentPathFrom, currentPathTo;
    bool currenPathExists;

    //public Color[] colors;

    private void Awake()
    {
        HexMetrics_Script.noiseSource = noiseSource;
        HexMetrics_Script.InitializeHashGrid(seed);
       // HexMetrics_Script.colors = colors;

        //gridCanvas = GetComponentInChildren<Canvas>();
        //hexMesh = GetComponentInChildren<HexMesh_Script>();

        CreateMap(cellCountX,cellCountZ);
    }

    public bool CreateMap(int x, int z)
    {
        if (x <= 0 || x % HexMetrics_Script.chunkSizeX != 0 ||
            z <= 0 || z % HexMetrics_Script.chunkSizeZ != 0)
        {
            Debug.LogError("Unsupported map size.");
            return false;
        }

        ClearPath();

        if (chunks!=null)
        {
            for(int i=0;i<chunks.Length;i++)
            {
                Destroy(chunks[i].gameObject);
            }
        }

        cellCountX = x;
        cellCountZ = z;

        chunkCountX = cellCountX / HexMetrics_Script.chunkSizeX;
        chunkCountZ = cellCountZ / HexMetrics_Script.chunkSizeZ;

        CreateChunks();
        CreateCells();

        return true;
    }
    void CreateCells()
    {
        cells = new HexCell_Script[cellCountZ * cellCountX];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    void CreateChunks()
    {
        chunks = new HexGridChunk_Script[chunkCountX * chunkCountZ];

        for(int z=0, i=0;z<chunkCountZ;z++)
        {
            for(int x=0;x<chunkCountX;x++)
            {
                HexGridChunk_Script chunk = chunks[i++]= Instantiate(chunkPrefab);

                chunk.transform.SetParent(transform);

            }
        }
    }
    private void OnEnable()
    {
        if(!HexMetrics_Script.noiseSource)
        {
            HexMetrics_Script.noiseSource = noiseSource;
            HexMetrics_Script.InitializeHashGrid(seed);
            //HexMetrics_Script.colors = colors;
        }

    }
    //private void Start()
    //{
    //    hexMesh.Triangulate(cells);
    //}

    public HexCell_Script GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        return cells[index];
    }

    //public void Refresh()
    //{
    //    hexMesh.Triangulate(cells);
    //}

    void CreateCell(int x,int z, int i)
    {
        Vector3 position;
        position.x = (x+z*0.5f-z/2) * (HexMetrics_Script.innerRadius*2f);
        position.y = 0f;
        position.z =z * (HexMetrics_Script.outerRadius * 1.5f);

        HexCell_Script cell = cells[i] = Instantiate<HexCell_Script>(cellPrefab);
        //cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        //cell.Color = defaultColor;

        if(x>0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if(z>0)
        {
            if((z&1)==0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if(x>0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if(x<cellCountX-1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }

        Text label = Instantiate<Text>(cellLabelPrefab);
        //label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        //label.text = cell.coordinates.ToStringOnSeparateLines();

        cell.uiRect = label.rectTransform;

        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);

        //Test
        cell.DisableHighlight();
    }

    void AddCellToChunk(int x, int z,HexCell_Script cell)
    {
        int chunkX = x / HexMetrics_Script.chunkSizeX;
        int chunkZ = z / HexMetrics_Script.chunkSizeZ;
        HexGridChunk_Script chunk = chunks[chunkX+chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics_Script.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics_Script.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics_Script.chunkSizeX, cell);
    }

    public HexCell_Script GetCell(HexCoordinates coordinates)
    {
        int z = coordinates.Z;
        if (z < 0 || z >= cellCountZ)
            return null;
        int x = coordinates.X + z / 2;
        if (x < 0 || x >= cellCountX)
            return null;
        return cells[x + z * cellCountX];
    }

    public void ShowUI(bool visible)
    {
        for(int i=0;i<chunks.Length;i++)
        {
            chunks[i].ShowUI(visible);
        }
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(cellCountX);
        writer.Write(cellCountZ);
        for(int i=0;i<cells.Length;i++)
        {
            cells[i].Save(writer);
        }
    }

    public void Load(BinaryReader reader)
    {
        ClearPath();
        int x, z;
        x = reader.ReadInt32();
        z = reader.ReadInt32();
        if (x != cellCountX || z != cellCountZ)
        {
            if (!CreateMap(x, z))
                return;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Load(reader);
        }
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Refresh();
        }
    }

    public HexCell_Script GetCell(int xOffset,int zOffset)
    {
        return cells[xOffset + zOffset * cellCountX];
    }

    public HexCell_Script GetCell(int cellIndex)
    {
        return cells[cellIndex];
    }

    public void FindPath(HexCell_Script fromCell,HexCell_Script toCell,int speed)
    {
        //StopAllCoroutines();
        //StartCoroutine(Search(fromCell,toCell,speed));
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        ClearPath();
        currentPathFrom = fromCell;
        currentPathTo = toCell;
        currenPathExists= Search(fromCell, toCell, speed);
        ShowPath(speed);
        sw.Stop();
        Debug.Log(sw.ElapsedMilliseconds);
    }

    bool Search(HexCell_Script fromCell,HexCell_Script toCell,int speed)
    {
        searchFrontierPhase += 2;

        if(searchFrontier==null)
        {
            searchFrontier = new HexCellPriorityQueue();
        }
        else
        {
            searchFrontier.Clear();
        }




        //WaitForSeconds delay = new WaitForSeconds(1 / 60f);
        fromCell.SearchPhase = searchFrontierPhase;
        fromCell.Distance = 0;
        searchFrontier.Enqueue(fromCell);
        while(searchFrontier.Count>0)
        {
            //yield return delay;
            HexCell_Script current = searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if(current.SearchPhase==10000)
            {
                Debug.LogError("Could not find Path after 10000 searches!");
                return false;
            }

            if (current == toCell)
            {
                return true;
            }


            int currentTurn = current.Distance / speed;

            for (HexDirection d = HexDirection.NE;d<=HexDirection.NW;d++)
            {
                
                HexCell_Script neighbor = current.GetNeighbor(d);
                if (neighbor == null||
                    neighbor.SearchPhase>searchFrontierPhase)
                {
                    continue;
                }
                if (neighbor.IsUnderwater)
                    continue;
                

                HexEdgeType edgeType = current.GetEdgeType(neighbor);
                if (edgeType == HexEdgeType.Cliff)
                    continue;


                int moveCost;
                if (current.HasRoadThroughEdge(d))
                {
                    moveCost = 1;
                }
                else if (current.Walled != neighbor.Walled)
                    continue;
                else
                {
                    moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
                    moveCost += neighbor.UrbanLevel + neighbor.FarmLevel + neighbor.PlantLevel;
                }

                int distance = current.Distance + moveCost;
                int turn = distance / speed;
                if (turn > currentTurn)
                    distance = turn * speed + moveCost;

                if(neighbor.SearchPhase<searchFrontierPhase)
                {
                    neighbor.Distance = distance;
                    //neighbor.SetLabel(turn.ToString());
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
                    searchFrontier.Enqueue(neighbor);
                }
                else if(distance<neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    //neighbor.SetLabel(turn.ToString());
                    neighbor.PathFrom = current;
                    searchFrontier.Change(neighbor, oldPriority);
                }

                //searchFrontier.Sort((x, y) => x.SearchPriority.CompareTo(y.SearchPriority));
            }
        }
        return false;
    }

    void ShowPath(int speed)
    {
        if(currenPathExists)
        {
            HexCell_Script current = currentPathTo;
            while(current!=currentPathFrom)
            {
                int turn = current.Distance / speed;
                current.SetLabel(turn.ToString());
                current.EnableHighlight(Color.white);
                current = current.PathFrom;
            }
        }
        currentPathFrom.EnableHighlight(Color.blue);
        currentPathTo.EnableHighlight(Color.red);
    }
    void ClearPath()
    {
        if (currenPathExists)
        {
            HexCell_Script current = currentPathTo;
            while (current != currentPathFrom)
            {
                current.SetLabel(null);
                current.DisableHighlight();
                current = current.PathFrom;
            }
            current.DisableHighlight();
            currenPathExists = false;
        }
        currentPathFrom = null;
        currentPathTo=null;
    }
}
