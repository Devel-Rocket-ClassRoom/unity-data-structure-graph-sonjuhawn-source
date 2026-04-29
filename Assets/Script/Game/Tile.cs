using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public enum Sides
{
    None=-1,
    Bottom,
    Right,
    Left,
    Top
}

public class Tile
{
    public int id;
    public Tile[] adjacents = new Tile[4];
    public int autoTileId;
    public static readonly int[] tableWeight =
    {
        int.MaxValue,
        1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
        2,4,int.MaxValue,1,1,1
    };
    public int weight => tableWeight[autoTileId +1];
    public int fowTileId = -1;
    public Tile previous = null;

    public bool isVisited = false;

    public bool CanMove => weight != int.MaxValue;

    public void UpdateAutoTileId()
    {
        if (autoTileId == -1) return;

        autoTileId = 0;
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] != null && adjacents[i].autoTileId != -1)
                autoTileId |= 1 << (adjacents.Length - 1 - i);
        }
    }

    public void UpdateFowTileId()
    {
        fowTileId = 0;
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null || !adjacents[i].isVisited)
                fowTileId |= 1 << (adjacents.Length - 1 - i);
        }
    }

    public void Visit()
    {
        if (isVisited) return;

        isVisited = true;
        foreach (Tile tile in adjacents)
        {
            if (tile != null)
            {
                tile.UpdateFowTileId();
            }
        }

        
    }


    public void RemoveAdjacents(Tile tile)
    {
        for(int i=0;i <adjacents.Length; i++)
        {
            if (adjacents[i] == null || adjacents[i].autoTileId == -1)
                continue;

            if (adjacents[i].id == tile.id)
            {
                adjacents[i].autoTileId = -1;
                UpdateAutoTileId();
                break;
            }
        }
    }

    public void ClearAdjacents()
    {
        for(int i=0; i< adjacents.Length; i++)
        {
            if (adjacents[i] == null || adjacents[i].autoTileId == -1)
                continue;

            adjacents[i].RemoveAdjacents(this);
        }

        UpdateAutoTileId();
    }
}
