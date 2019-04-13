using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;
using Color = UnityEngine.Color;

public class Node
{
    public List<Node> neighbors = new List<Node>();
    public Vector2 pos;

    public bool isFree;

    public bool hasBeenVisited = false;
    public bool isPath = false;

    public Node cameFrom = null;
    
    public float currentCost = -1;
}

public class Pathfinding : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private bool gizmo;
    private int minX, minY, maxX, maxY;
    private Vector2 playerPos;
    private Vector2 enemiesPos;
    [HideInInspector] public Node tileStart;
    [HideInInspector] public Node tileGoal;
    Node[,] graph = new Node[CellularAutomata.instance.sizeX, CellularAutomata.instance.sizeY];
    public static Pathfinding instance;
    
    private void Awake()
    {
        instance = this;
        
    }

    private void Start()
    {

        //nInvoke("LateStart", 2);
        playerPos = FindObjectOfType<PlayerMouvement>().transform.position;

        GenerateGraph();
    }

    void GenerateGraph()
    {
        minX = tilemap.cellBounds.xMin;
        maxX = tilemap.cellBounds.xMax;

        minY = tilemap.cellBounds.yMin;
        maxY = tilemap.cellBounds.yMax;

        graph = new Node[maxX - minX, maxY - minY];

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                TileBase currentTile = tilemap.GetTile(new Vector3Int(x, y, 0));

                if (currentTile == null) continue;

                Node newNode = new Node
                {
                    pos = new Vector2(x * tilemap.cellSize.x + tilemap.cellSize.x / 2,
                        y * tilemap.cellSize.y + tilemap.cellSize.y / 2),
                    neighbors = new List<Node>()
                };

                switch (currentTile.name)
                {
                    case "CaveWall":
                        newNode.isFree = false;
                        break;
                    case "ColliderTilemap":
                        newNode.isFree = false;
                        break;
                    case "GroundTile":
                        newNode.isFree = true;
                        break;
                }

                graph[x - minX, y - minY] = newNode;
            }
        }

        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int i = 0; i < graph.GetLength(0); i++)
        {
            for (int j = 0; j < graph.GetLength(1); j++)
            {
                Node node = graph[i, j];

                if (node == null) continue;
                if (!node.isFree) continue;

                foreach (Vector2Int b in bounds.allPositionsWithin)
                {
                    if (i + b.x < 0 || i + b.x > maxX - minX || j + b.y <= 0 || j + b.y > maxY - minY) continue;
                    if (b.x == 0 && b.y == 0) continue;

                    if (graph[i + b.x, j + b.y] == null) continue;
                    if (!graph[i + b.x, j + b.y].isFree) continue;

                    node.neighbors.Add(graph[i + b.x, j + b.y]);
                }
            }
        }
    }

   public List<Vector2> Astar(Vector2 startPos)
    {
        tileStart = GetEnemiesPos(startPos);
        tileGoal = GetPlayerPos();
        
        List<Vector2> path = new List<Vector2>();
        
        
        List<Node> openList = new List<Node> {tileStart};
        List<Node> closedList = new List<Node>();

        int crashValue = 1000;

        while (openList.Count > 0 && --crashValue > 0)
        {
            openList = openList.OrderBy(x => x.currentCost).ToList();
            
            Node currentNode = openList[0];
            closedList.Add(currentNode);
            openList.RemoveAt(0);

            currentNode.hasBeenVisited = true;

            if (currentNode == tileGoal)
            {
                break;
            }
            else
            {
                foreach (Node currentNodeNeighbor in currentNode.neighbors)
                {
                    float modifier;
                    if (currentNode.pos.x == currentNodeNeighbor.pos.x ||
                        currentNode.pos.y == currentNodeNeighbor.pos.y)
                    {
                        modifier = 2;
                    }
                    else
                    {
                        modifier = 4;
                    }

                    //float newCost = currentNode.currentCost + currentNodeNeighbor.cost + modifier;
                    float newCost = Vector2.Distance(currentNode.pos, tileStart.pos) + Vector2.Distance(currentNodeNeighbor.pos , tileGoal.pos) + modifier;


                    if (currentNodeNeighbor.currentCost == -1 || currentNodeNeighbor.currentCost > newCost)
                    {
                        currentNodeNeighbor.cameFrom = currentNode;
                        currentNodeNeighbor.currentCost = newCost;
                        
                        openList.Add(currentNodeNeighbor);
                    }
                }
            }
        }

        if (crashValue <= 0)
        {
            Debug.Log("Mauvais");
        }

        
            Node _currentNode = tileGoal;
            while (_currentNode.cameFrom != null)
            {
                _currentNode.isPath = true;
                _currentNode = _currentNode.cameFrom;
                path.Add(_currentNode.pos);
            }
            
            _currentNode.isPath = true;
        

        ResetNode();
        return path;
    }
   
    public Node GetPlayerPos()
    {
        Node playerNode = new Node();

        float distance = 500;
        
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                TileBase currentTile = tilemap.GetTile(new Vector3Int(x, y, 0));

                if (currentTile == null) continue;
                if (Vector2.Distance(graph[x - minX, y - minY].pos, playerPos) < distance && graph[x - minX, y - minY].isFree)
                {
                    playerNode = graph[x - minX, y - minY];
                    distance = Vector2.Distance(graph[x - minX, y - minY].pos, playerPos);
                }
            }
        }
        
        Debug.Log("Player position" + playerPos);
        return playerNode;
    }

    public Node GetEnemiesPos(Vector2 enemiesPos)
    {
        Node enemiesNode = new Node();

        float distance = 500;

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                TileBase currentTile = tilemap.GetTile(new Vector3Int(x, y, 0));

                if (currentTile == null) continue;
                if (Vector2.Distance(graph[x - minX, y - minY].pos, enemiesPos) < distance && graph[x - minX, y - minY].isFree)
                {
                    enemiesNode = graph[x - minX, y - minY];
                    distance = Vector2.Distance(graph[x - minX, y - minY].pos, enemiesPos);
                }
            }
        }

        Debug.Log("Enemies position : " + enemiesNode.pos);
        
        
        return enemiesNode;
    }

    private void ResetNode()
    {
        foreach (Node node in graph)
        {
            if (node == null) continue;
            node.cameFrom = new Node();
            node.currentCost = 0;
            node.hasBeenVisited = false;
            node.isPath = false;
        }
    }
    
    void OnDrawGizmos()
    {
        if (gizmo)
        {
            if (minX == maxX || minY == maxY) return;

            Gizmos.DrawLine(new Vector3(minX, minY), new Vector3(maxX, minY));
            Gizmos.DrawLine(new Vector3(maxX, minY), new Vector3(maxX, maxY));
            Gizmos.DrawLine(new Vector3(maxX, maxY), new Vector3(minX, maxY));
            Gizmos.DrawLine(new Vector3(minX, maxY), new Vector3(minX, minY));

            foreach (Node node in graph)
            {
                if (node == null) continue;

                Gizmos.color = node.isFree ? Color.blue : Color.red;

                if (node.hasBeenVisited)
                {
                    Gizmos.color = Color.yellow;
                }

                if (node.isPath)
                {
                    Gizmos.color = Color.green;
                }

                if (node == tileStart)
                {
                    Gizmos.color = Color.magenta;
                }

                if (node == tileGoal)
                {
                    Gizmos.color = Color.red;
                }
                
                Gizmos.DrawCube(node.pos, Vector3.one * 0.75f);

                foreach (Node nodeNeighbor in node.neighbors)
                {
                    Gizmos.DrawLine(node.pos, nodeNeighbor.pos);
                }
            }
        }
    }
}
