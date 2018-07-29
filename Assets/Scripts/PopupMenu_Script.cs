using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupMenu_Script : MonoBehaviour {

    public HexGrid_Script hexGrid;
    public HexMapGenerator_Script mapGenerator;
    bool generateMaps = true;

    public void Open()
    {
        gameObject.SetActive(true);
        TacticalCamera_Script.Locked = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        TacticalCamera_Script.Locked = false;
    }

    void CreateMap(int x, int z)
    {
        if(generateMaps)
        {
            mapGenerator.GenerateMap(x, z);
        }
        else
        {
            hexGrid.CreateMap(x, z);
        }

        TacticalCamera_Script.ValidatePosition();
        Close();
    }

    public void CreateSmallMap()
    {
        CreateMap(20, 15);
    }
    public void CreateMediumMap()
    {
        CreateMap(40, 30);
    }
    public void CreateLargeMap()
    {
        CreateMap(80, 60);
    }

    public void ToggleMapGeneration(bool toggle)
    {
        generateMaps = toggle;
    }
}
