using System.Linq;
using UnityEngine;

public enum TileTypes
{
    Empty = -1,
    // 0 ~ 14 : Shore
    Grass = 15,
    Tree,
    Hills,
    Mountains,
    Town,
    Castle,
    Monster,
}

public class Map
{
    public int rows = 0;
    public int cols = 0;

    public Tile[] tiles;

    public Tile[] CoastTiles => tiles.Where(t => t.autoTileId >= 0 && t.autoTileId < (int)TileTypes.Grass).ToArray();
    public Tile[] LandTiles => tiles.Where(t => t.autoTileId == (int)TileTypes.Grass).ToArray();

    public Tile startTile;
    public Tile castleTile;

    public void Init(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;

        tiles = new Tile[rows * cols];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Tile
            {
                id = i
            };
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (r + 1 < rows)
                {
                    tiles[r * cols + c].adjacents[(int)Sides.Bottom] = tiles[(r + 1) * cols + c];
                }

                if (c - 1 >= 0)
                {
                    tiles[r * cols + c].adjacents[(int)Sides.Left] = tiles[r * cols + (c - 1)];
                }

                if (c + 1 < cols)
                {
                    tiles[r * cols + c].adjacents[(int)Sides.Right] = tiles[r * cols + (c + 1)];
                }

                if (r - 1 >= 0)
                {
                    tiles[r * cols + c].adjacents[(int)Sides.Top] = tiles[(r - 1) * cols + c];
                }
            }
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].UpdateAutoTileId();
            tiles[i].UpdateFowTileId();
        }
    }

    public void ShuffleTiles(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i > 0; --i)
        {
            int rand = Random.Range(0, i + 1);
            (tiles[rand], tiles[i]) = (tiles[i], tiles[rand]);

        }
    }

    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);

        int total = Mathf.FloorToInt(tiles.Length * percent);
        for (int i = 0; i < total; i++)
        {
            tiles[i].autoTileId = (int)tileType;

        }        
    }

    public bool CreateIsland(float erodePercent, int erodeIterations)
    {
        for (int i = 0; i < erodeIterations; i++)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        return true;
    }

    public bool CreateIsland(float erodePercent, int erodeIterations,
    float lakePercent, float treePercent, float hillPercent,
    float mountainPercent, float townPercent, float monsterPercent)
    {
        for (int i = 0; i < erodeIterations; i++)
        {
            Debug.Log($"CoastTiles : {CoastTiles.Length}");
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        DecorateTiles(LandTiles,lakePercent, TileTypes.Empty);
        DecorateTiles(LandTiles,treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles,hillPercent, TileTypes.Hills);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountains);
        DecorateTiles(LandTiles, townPercent, TileTypes.Town);
        DecorateTiles(LandTiles, monsterPercent, TileTypes.Monster);

        var tilesLeft = LandTiles;
        if (tilesLeft.Length > 0)
        {
            ShuffleTiles(tilesLeft);
            tilesLeft[0].autoTileId = (int)TileTypes.Castle;
            castleTile = tilesLeft[0];
        }
        else
        {
            return false;
        }

        var towns = tiles.Where(t => t.autoTileId == (int)TileTypes.Town).ToArray();
        ShuffleTiles(towns);
        startTile = towns[0];

        return true;
    }

    public void ResetNodePrevious()
    {
        foreach (var node in tiles)
        {
            node.previous = null;
        }
    }
}