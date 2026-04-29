using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Stage stage;
    private Animator anim;

    private int currentTileId;
    private int targetTileId;

    public int viewArea = 1;

    private bool isMoving = false;
    private Coroutine coMove = null;

    private List<Tile> path = new List<Tile>();
    private Map map;

    public void Awake()
    {
        anim = GetComponent<Animator>();
        var findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();
        map = stage.Map;
    }

    private void Update()
    {

        var direction = Sides.None;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Sides.Top;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Sides.Bottom;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Sides.Right;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Sides.Left;
        }

        if (direction != Sides.None)
        {
            var targetTile = stage.Map.tiles[currentTileId].adjacents[(int)direction];
            if (targetTile != null && targetTile.CanMove)
            {
                MoveTo(targetTile.id);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            int currentMouseIndex = stage.ScreenPosToField(Input.mousePosition);
            if(AStar(stage.Map.tiles[currentTileId], stage.Map.tiles[currentMouseIndex]))
            {
                StartCoroutine(CoMovePath());
            }
        }
    }


    public void MoveTo(int tileId)
    {
        if (isMoving)
            return;

        targetTileId = tileId;
        if (coMove != null)
        {
            StopCoroutine(coMove);
            coMove = null;
        }
        coMove = StartCoroutine(CoMove());
    }

    public void Warp(int tileId)
    {
        if (coMove != null)
        {
            StopCoroutine(coMove);
            coMove = null;
        }
        isMoving = false;
        targetTileId = -1;

        anim.speed = 0f;
        currentTileId = tileId;
        transform.position = stage.GetTilePos(currentTileId);
        stage.OnTileVisited(currentTileId);
    }


    public float movespeed = 10f;
    private IEnumerator CoMove()
    {
        isMoving = true;

        var startPos = transform.position;
        var endPos = stage.GetTilePos(targetTileId);
        var duration = Vector3.Distance(startPos, endPos) / movespeed;
        
        var t = 0f;
        while(t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return 0f;
        }

        transform.position = endPos;
        anim.speed = 0f;

        currentTileId = targetTileId;
        targetTileId = -1;
        stage.OnTileVisited(currentTileId);
        isMoving = false;
        coMove = null;
    }

    public bool AStar(Tile startNode, Tile endNode)
    {
        path.Clear();
        map.ResetNodePrevious();

        var visited = new HashSet<Tile>();
        var priorityQueue = new PriorityQueue<Tile, int>();

        var distance = new int[map.tiles.Length];
        for (int i = 0; i < distance.Length; ++i)
        {
            distance[i] = int.MaxValue;
        }

        distance[startNode.id] = 0;
        priorityQueue.Enqueue(startNode, distance[startNode.id] + Heuristic(startNode, endNode));

        bool success = false;

        while (priorityQueue.Count > 0)
        {
            var currentNode = priorityQueue.Dequeue();
            if (visited.Contains(currentNode))
                continue;

            if (currentNode == endNode)
            {
                success = true;
                break;
            }

            visited.Add(currentNode);

            

            foreach (var adjacent in currentNode.adjacents)
            {
                if (visited.Contains(adjacent))
                    continue;

                var newDistance = distance[currentNode.id] + adjacent.weight;
                if (distance[adjacent.id] > newDistance)
                {
                    distance[adjacent.id] = newDistance;
                    adjacent.previous = currentNode;
                    priorityQueue.Enqueue(adjacent, distance[adjacent.id] + Heuristic(adjacent, endNode));
                }

            }
        }

        if (!success)
        {
            return false;
        }

        Tile step = endNode;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }

    private int Heuristic(Tile a, Tile b)
    {
        int ax = a.id % map.cols;
        int ay = a.id / map.cols;

        int bx = b.id % map.cols;
        int by = b.id / map.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    private IEnumerator CoMovePath()
    {
        foreach (var tile in path)
        {
            MoveTo(tile.id);

            while (isMoving)
                yield return null;
        }
    }
}
