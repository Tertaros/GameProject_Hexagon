using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class SaveLoadMenu_Script : MonoBehaviour {

    public HexGrid_Script hexGrid;
    bool saveMode;

    public Text menuLabel;
    public Text actionButtonLabel;

    public InputField nameInput;
    public const int mapVersionNumber = 2;
    public RectTransform listContent;
    public SaveLoadItem_Script itemPrefab;

    public void Open(bool saveMode) {
        this.saveMode = saveMode;
        if(saveMode)
        {
            menuLabel.text = "Save Map";
            actionButtonLabel.text = "Save";
        }
        else
        {
            menuLabel.text = "Load Map";
            actionButtonLabel.text = "Load";
        }
        FillList();
        gameObject.SetActive(true);
        TacticalCamera_Script.Locked = true;
	}
	
	// Update is called once per frame
	public void Close () {
        gameObject.SetActive(false);
        TacticalCamera_Script.Locked = false;

	}

    string GetSelectedPath()
    {
        string mapName = nameInput.text;
        if (mapName.Length == 0)
            return null;

        return Path.Combine(Application.persistentDataPath, mapName + ".map");
    }

   void Save(string path)
    {
        Debug.Log(Application.persistentDataPath);

        using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            writer.Write(mapVersionNumber);
            hexGrid.Save(writer);
        }


    }

    void Load(string path)    {

        if(!File.Exists(path))
        {
            Debug.LogError("File does not exist: " + path);
                return;
        }
        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
        {
            int header = reader.ReadInt32();
            if (header == mapVersionNumber)
            {
                hexGrid.Load(reader);
                TacticalCamera_Script.ValidatePosition();
            }
            else
                Debug.LogWarning("Unknown map format " + header);
        }
    }

    public void Action()
    {
        string path = GetSelectedPath();

        if (path == null)
            return;
        if (saveMode)
            Save(path);
        else
            Load(path);

        Close();
    }

    public void SelectItem(string name)
    {
        nameInput.text = name;
    }

    void FillList()
    {
        for(int i=0; i<listContent.childCount;i++)
        {
            Destroy(listContent.GetChild(i).gameObject);
        }

        string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.map");
        Array.Sort(paths);
        for(int i=0; i<paths.Length;i++)
        {
            SaveLoadItem_Script item = Instantiate(itemPrefab);
            item.menu = this;
            item.MapName = Path.GetFileNameWithoutExtension(paths[i]);
            item.transform.SetParent(listContent, false);
        }
    }

    public void Delete()
    {
        string path = GetSelectedPath();
        if (path == null)
            return;

        if(File.Exists(path))
        File.Delete(path);

        nameInput.text = "";
        FillList();
    }
}
