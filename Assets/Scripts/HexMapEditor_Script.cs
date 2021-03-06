﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

public class HexMapEditor_Script : MonoBehaviour {

    // Use this for initialization
    //public Color[] colors;
    public HexGrid_Script hexGrid;
    //private Color activeColor;
    int activeElevation;
    //bool applyColor;
    bool applyElevation = true;

    int brushSize;

    bool isDrag;
    HexDirection dragDirection;
    HexCell_Script previousCell;

    public  const int mapVersionNumber= 1;
    public Material terrainMaterial;


    enum OptionalToggle
    {
        Ignore,
        Yes,
        No
    }


    OptionalToggle riverMode;
    OptionalToggle roadMode;
    OptionalToggle walledMode;

    int activeWaterLevel;
    bool applyWaterLevel = true;

    int activeUrbanLevel;
    bool applyUrbanLevel = true;

    int activePlantLevel;
    bool applyPlantLevel = true;

    int activeFarmLevel;
    bool applyFarmLevel = true;

    int activeSpecialIndex;
    bool applySpecialIndex = true;

    int activeTerrainTypeIndex;

   

    private void Awake()
    {
        terrainMaterial.DisableKeyword("GRID_ON");
        //Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
        SetEditMode(false);
    }

    // Update is called once per frame
    void Update () {


        if(!EventSystem.current.IsPointerOverGameObject())
        {
            if(Input.GetMouseButton(0))
            {
                HandleInput();
                return;
            }
            if(Input.GetKeyDown(KeyCode.U))
            {
                if(Input.GetKey(KeyCode.LeftShift))
                {
                    DestroyUnit();
                }
                else
                CreateUnit();
                return;
            }            
        }
        previousCell = null;
	}

    void HandleInput()
    {
        HexCell_Script currentCell = GetCellUnderCursor();
        if(currentCell)
        { 
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
                isDrag = false;
           
            EditCells(currentCell);
            
            previousCell = currentCell;
            isDrag = true;
        }
        else
        {
            previousCell = null;
        }
    }

    void ValidateDrag(HexCell_Script currentCell)
    {
        for(dragDirection=HexDirection.NE;dragDirection<=HexDirection.NW;dragDirection++)
        {
            if(previousCell.GetNeighbor(dragDirection)==currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    void EditCells(HexCell_Script center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for(int r = 0, z = centerZ -brushSize;z<=centerZ;z++,r++)
        {
            for(int x = centerX-r;x<=centerX+brushSize;x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }
    void EditCell(HexCell_Script cell)
    {
        if (cell)
        {
            //if (applyColor)
            //    cell.Color = activeColor;
            if (activeTerrainTypeIndex >= 0)
                cell.TerrainTypeIndex = activeTerrainTypeIndex;
            if (applyElevation)
                cell.Elevation = activeElevation;
            if (applyWaterLevel)
                cell.WaterLevel = activeWaterLevel;
            if (applySpecialIndex)
                cell.SpecialIndex = activeSpecialIndex;
            if (applyUrbanLevel)
                cell.UrbanLevel = activeUrbanLevel;
            if (applyFarmLevel)
                cell.FarmLevel = activeFarmLevel;
            if (applyPlantLevel)
                cell.PlantLevel = activePlantLevel;
            if (riverMode == OptionalToggle.No)
                cell.RemoverRiver();
            if (roadMode == OptionalToggle.No)
                cell.RemoveRoads();
            if(isDrag)
            {
                HexCell_Script otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if(otherCell)
                {
                    if (riverMode == OptionalToggle.Yes)
                        otherCell.SetOutgoingRiver(dragDirection);
                    if (walledMode != OptionalToggle.Ignore)
                        cell.Walled = walledMode == OptionalToggle.Yes;
                    if (roadMode == OptionalToggle.Yes)
                        otherCell.AddRoad(dragDirection);
                }
            }
            //else if (isDrag && riverMode == OptionalToggle.Yes)
            //{
            //    HexCell_Script otherCell = cell.GetNeighbor(dragDirection.Opposite());
            //    if (otherCell)
            //        otherCell.SetOutgoingRiver(dragDirection);
            //}
        }
    }

    //public void SelectColor(int index)
    //{
    //    applyColor = index >= 0;
    //    if(applyColor)
    //    activeColor = colors[index];
    //}

    public  void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }


    //public void ShowUI (bool visible)
    //{
    //    hexGrid.ShowUI(visible);
    //}

    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    public void SetApplyWaterLevel(bool toggle)
    {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel(float level)
    {
        activeWaterLevel =(int) level;
    }
        
    public void SetApplyUrbanLevel(bool toggle)
    {
        applyUrbanLevel = toggle;
    }

    public void SetUrbanLevel(float level)
    {
        activeUrbanLevel = (int)level;
    }

    public void SetApplyPlantLevel(bool toggle)
    {
        applyPlantLevel = toggle;
    }

    public void SetPlantLevel(float level)
    {
        activePlantLevel = (int)level;
    }
    public void SetApplyFarmLevel(bool toggle)
    {
        applyFarmLevel = toggle;
    }

    public void SetFarmLevel(float level)
    {
        activeFarmLevel = (int)level;
    }

    public void SetWalledMode(int mode)
    {
        walledMode = (OptionalToggle)mode;
    }

    public void SetApplySpecialIndex(bool toggle)
    {
        applySpecialIndex = toggle;
    }

    public void SetSpecialIndex(float index)
    {
        activeSpecialIndex = (int)index;
    }

    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
    }

    //public void SetEditMode(bool toggle)
    //{
    //    enabled = toggle;
    //}

    public void ShowGrid(bool visible)
    {
        if (visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
        }
        else
            terrainMaterial.DisableKeyword("GRID_ON");
    }

    public void SetEditMode(bool toggle)
    {
        enabled = toggle;
    }

    HexCell_Script GetCellUnderCursor()
    {
        return hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }

    void CreateUnit()
    {
        HexCell_Script cell = GetCellUnderCursor();
        if (cell&& !cell.Unit)
        {
            hexGrid.AddUnit(Instantiate(HexUnit_Script.unitPrefab), cell, Random.Range(0f, 360f));
        }
    }

    void DestroyUnit()
    {
        HexCell_Script cell = GetCellUnderCursor();
        if (cell && cell.Unit)
            hexGrid.RemoveUnit(cell.Unit);
    }

    
}
