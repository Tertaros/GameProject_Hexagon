
using System.Collections.Generic;


public class HexCellPriorityQueue{

    int count = 0;
    int minimum = int.MaxValue;
    public int Count
    {
        get { return count; }
    }
    List<HexCell_Script> list = new List<HexCell_Script>();

    public void Enqueue(HexCell_Script cell)
    {
        count++;
        int priority = cell.SearchPriority;
        if(priority<minimum)
        {
            minimum = priority;
        }
        while(priority>=list.Count)
        {
            list.Add(null);
        }
        cell.NextWithSamePriority = list[priority];
        list[priority] = cell;
    }

    public HexCell_Script Dequeue()
    {
        count--;
        for (; minimum < list.Count; minimum++)
        {
            HexCell_Script cell = list[minimum];
            if (cell != null)
            {
                list[minimum] = cell.NextWithSamePriority;
                return cell;
            }
                
        }
        return null; 
    }

    public void Change(HexCell_Script cell,int oldPriority)
    {
        HexCell_Script current = list[oldPriority];
        HexCell_Script next = current.NextWithSamePriority;

        if(current==cell)
        {
            list[oldPriority] = next;
        }
        else
        {
            while(next!=cell)
            {
                current = next;
                next = current.NextWithSamePriority;
            }
            current.NextWithSamePriority = cell.NextWithSamePriority;
        }

        Enqueue(cell);
        count--;
    }

    public void Clear()
    {
        list.Clear();
        count = 0;
    }
}
