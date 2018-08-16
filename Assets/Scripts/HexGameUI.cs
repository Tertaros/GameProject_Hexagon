
using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour {

    public HexGrid_Script grid;
    HexUnit_Script selectedUnit;
    HexCell_Script currentCell;

    public void SetEditMode(bool toggle)
    {
        grid.ClearPath();
        enabled = !toggle;
        grid.ShowUI(!toggle);
    }

    bool UpdateCurrentCell()
    {
        HexCell_Script cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        if(cell!=currentCell)
        {
            currentCell = cell;
            return true;
        }
        return false;
    }

    void DoSelection()
    {
        grid.ClearPath();
        UpdateCurrentCell();
        if (currentCell)
        {
            selectedUnit = currentCell.Unit;
        }
    }

    private void Update()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
        {
            if(Input.GetMouseButton(0))
            {
                DoSelection();
            }
            else if(selectedUnit)
            {
                if (Input.GetMouseButtonDown(1))
                    DoMove();
                else
                    DoPathfinding();
            }
        }
    }

    void DoPathfinding()
    {
        if(UpdateCurrentCell())
        {
            if(currentCell&& selectedUnit.IsValidDestination(currentCell))
            {
                grid.FindPath(selectedUnit.Location, currentCell, 24);
            }
            else
            {
                grid.ClearPath();
            }
        }

    }

    void DoMove()
    {
        if(grid.HasPath)
        {
            selectedUnit.Travel(grid.GetPath());
            grid.ClearPath();
        }
    }
}
