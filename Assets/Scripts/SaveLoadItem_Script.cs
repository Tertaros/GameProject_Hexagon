using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadItem_Script : MonoBehaviour {

    public SaveLoadMenu_Script menu;
    string mapName;
	public string MapName
    {
        get { return mapName; }
        set
        {
            mapName = value;
            transform.GetChild(0).GetComponent<Text>().text = value;
        }
    }

    public void Select()
    {
        menu.SelectItem(mapName);
    }
}
