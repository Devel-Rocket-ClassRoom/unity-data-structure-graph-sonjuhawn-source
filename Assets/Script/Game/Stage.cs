using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public GameObject tilePrefab;
    private GameObject[] tileObjects;

    public int mapWidth = 20;
    public int mapHeight = 20;

    public int erodeIterations = 2;
    [Range(0f, 0.9f)]
    public float erodePercent = 0.5f;
    [Range(0f, 0.9f)]
    public float lakePercent = 0.01f;
    [Range(0f, 0.9f)]
    public float treePercent = 0.3f;
    [Range(0f, 0.9f)]
    public float hillPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float mountainPercent = 0.05f;
    [Range(0f, 0.9f)]
    public float townPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float monsterPercent = 0.1f;

    public Vector2 tileSize = new Vector2(16, 16);
    public Sprite[] islandSprites;
    public Sprite[] fogSprites;

    private Map map;
    public Map Map => map;

    private Vector2 zeroPos = Vector2.zero;

    private int lastMouseIndex = -1;
    private SpriteRenderer lastMouseSprite = null;
    private Camera mainCamera;

    public PlayerMovement playerPrefab;
    private PlayerMovement playerObject;


    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetStage();
        }

        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log(ScreenPosToField(Input.mousePosition));
            //Debug.Log(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }

        if (map != null)
        {
            int currentMouseIndex = ScreenPosToField(Input.mousePosition);
            if (currentMouseIndex != lastMouseIndex)
            {
                if (lastMouseSprite != null)
                {
                    lastMouseSprite.color = Color.white;
                }

                if (currentMouseIndex != -1 && currentMouseIndex < tileObjects.Length)
                {
                    lastMouseSprite = tileObjects[currentMouseIndex].GetComponent<SpriteRenderer>();
                    lastMouseSprite.color = Color.green;
                }
                else
                {
                    lastMouseSprite = null;
                }

                lastMouseIndex = currentMouseIndex;
            }
        }
    }

    private void CreatePlayer()
    {
        if (playerObject != null)
        {
            Destroy(playerObject.gameObject);
        }
        playerObject = Instantiate(playerPrefab);
        playerObject.Warp(map.startTile.id);
    }

    private void ResetStage()
    {
        map = new Map();
        map.Init(mapHeight, mapWidth);
        bool success = false;
        do
        {
            success = map.CreateIsland(erodePercent, erodeIterations, 
                lakePercent, treePercent, hillPercent, mountainPercent, townPercent, monsterPercent);

        }
        while (!success);
        CreateGrid();
        //DrawPath(tile);
        CreatePlayer();
    }

    private void DrawPath(List<Tile> path)
    {
        for(int i = 0;  i < path.Count; i++)
        {
            float t = i / (path.Count - 1);

            tileObjects[path[i].id].GetComponent<SpriteRenderer>().color =
                Color.Lerp(Color.green, Color.red, t);
        }
    }

    private void CreateGrid()
    {
        if (tileObjects != null)
        {
            foreach (var tileObject in tileObjects)
            {
                Destroy(tileObject);
            }
        }

        tileObjects = new GameObject[mapWidth * mapHeight];
        var position = new Vector2(-mapWidth * tileSize.x / 2, mapHeight * tileSize.y / 2);
        zeroPos = new Vector2(-(mapWidth) * tileSize.x / 2f, (mapHeight) * tileSize.y / 2f);
        Debug.Log(zeroPos);
        for (int h = 0; h < mapHeight; h++)
        {
            for (int w = 0; w < mapWidth; w++)
            {
                int tileId = h * mapWidth + w;
                var newGameobject = Instantiate(tilePrefab, transform);
                newGameobject.transform.position = position;
                position.x += tileSize.x;

                tileObjects[tileId] = newGameobject;
                DecorateTile(tileId);
            }
            position.x = -mapWidth * tileSize.x / 2;
            position.y -= tileSize.y;
        }
    }

    public void DecorateTile(int tileId)
    {
        var tile = map.tiles[tileId];
        var tileObject = tileObjects[tileId];
        var renderer = tileObject.GetComponent<SpriteRenderer>();

        if (tile == null)
            return;

        if (tile.isVisited)
        {
            if (tile.autoTileId != (int)TileTypes.Empty)
            {
                renderer.sprite = islandSprites[tile.autoTileId];
            }
            else
            {
                renderer.sprite = null;
            }
        }
        else
        {
            if (tile.autoTileId != (int)TileTypes.Empty && tile.fowTileId != (int)TileTypes.Empty && tile.fowTileId != -1)
            {
                renderer.sprite = fogSprites[tile.fowTileId];
            }
            else
            {
                renderer.sprite = null;
            }
        }
    }

    public void DecorateAll()
    {
        for (int i = 0; i < tileObjects.Length; i++)
        {
            DecorateTile(i);
        }
    }

    public int visitRadius = 1;

    public void OnTileVisited(int tileId)
    {
        OnTileVisited(map.tiles[tileId]);
    }

    public void OnTileVisited(Tile tile)
    {
        int centerX = tile.id % mapWidth;
        int centerY = tile.id / mapWidth;

        for (int i = -visitRadius; i <= visitRadius; i++)
        {
            for (int j = -visitRadius; j <= visitRadius; j++)
            {
                int x = centerX + j;
                int y = centerY + i;
                if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
                {
                    continue;
                }
                int id = y * mapWidth + x;
                map.tiles[id].isVisited = true;
                DecorateTile(id);
            }
        }

        var radius = visitRadius + 1;
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (i == radius || i == -radius || j == radius || j == -radius)
                {
                    int x = centerX + j;
                    int y = centerY + i;
                    if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
                    {
                        continue;
                    }
                    int id = y * mapWidth + x;
                    map.tiles[id].isVisited = true;
                    DecorateTile(id);
                }
            }
        }
    }

    public int ScreenPosToField(Vector3 screen)
    {
        screen.z = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
        return WorldPosToTileId(mainCamera.ScreenToWorldPoint(screen));
    }

    public int WorldPosToTileId(Vector3 world)
    {
        Vector2 tilePoint = (Vector2)world - zeroPos + tileSize / 2f;
        int w = Mathf.FloorToInt(tilePoint.x / tileSize.x);
        int h = -Mathf.FloorToInt(tilePoint.y / tileSize.y);

        //Debug.Log($"w:{w} h:{h}");
        if (w < 0 || h < 0 || w >= mapWidth || h >= mapHeight)
            return -1;

        return h * mapWidth + w;
    }


    public Vector3 GetTilePos(int y, int x)
    {
        Debug.Log($"{x} {y}");
        return new Vector3(zeroPos.x + x * tileSize.x, zeroPos.y - y * tileSize.y);
    }

    public Vector3 GetTilePos(int tileId)
    {
        return GetTilePos(tileId / mapWidth, tileId % mapWidth);
    }
}
