using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Node
{
    public List<Node> Neighbors = new List<Node>();
    public Vector2 Pos;

    public bool IsFree;

    public bool HasBeenVisited = false;
    public bool IsPath = false;

    public Node CameFrom = null;

    public float CurrentCost = -1;
}

public class Pathfinding : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    
    private Node[,] _graph;
    private int _minX, _minY, _maxX, _maxY;
    private Transform _playerPos;
    private Vector2 _enemiesPos;

    [HideInInspector] public Node tileStart;
    [HideInInspector] public Node tileGoal;
    
    [SerializeField] private bool gizmo;
    
    public static Pathfinding Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
         _graph = new Node[CellularAutomata.instance.sizeX, CellularAutomata.instance.sizeY];
         
        Invoke("LateStart", 2);
    }

    private void LateStart()
    {
        _playerPos = FindObjectOfType<PlayerMouvement>().transform;
        
        GenerateGraph();
    }

    void GenerateGraph()
    {
        _minX = tilemap.cellBounds.xMin;
        _maxX = tilemap.cellBounds.xMax;

        _minY = tilemap.cellBounds.yMin;
        _maxY = tilemap.cellBounds.yMax;

        _graph = new Node[_maxX - _minX, _maxY -_minY];

        for (int x = _minX; x < _maxX; x++)
        {
            for (int y = _minY; y < _maxY; y++)
            {
                TileBase currentTile = tilemap.GetTile(new Vector3Int(x, y, 0));

                if (currentTile == null) continue;

                Node newNode = new Node
                {
                    Pos = new Vector2(x * tilemap.cellSize.x + tilemap.cellSize.x / 2, y * tilemap.cellSize.y + tilemap.cellSize.y / 2),
                    Neighbors = new List<Node>()
                };

                switch (currentTile.name)
                {
                    case "GroundTile":
                        newNode.IsFree = true;
                        break;
                }

                _graph[x - _minX, y - _minY] = newNode;
            }
        }

        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        
        for (int i = 0; i < _graph.GetLength(0); i++)
        {
            for (int j = 0; j < _graph.GetLength(1); j++)
            {
                Node node = _graph[i, j];

                if (node == null) continue;
                if (!node.IsFree) continue;

                foreach (Vector2Int b in bounds.allPositionsWithin)
                {
                    if (i + b.x < 0 || i + b.x >= _maxX - _minX || j + b.y < 0 || j + b.y >= _maxY - _minY) continue;
                    if (b.x == 0 && b.y == 0) continue;
                    
                    if (_graph[i + b.x, j + b.y] == null) continue;
                    if (!_graph[i + b.x, j + b.y].IsFree) continue;
                    if (b.x != 0 || b.y != 0)
                    {
                        if (_graph[i,j + b.y] == null) continue;
                        if(!_graph[i,j + b.y].IsFree) continue;
                        
                        if (_graph[i + b.x,j] == null) continue;
                        if(!_graph[i + b.x,j].IsFree) continue;
                        
                    }
                    node.Neighbors.Add(_graph[i + b.x, j + b.y]);
                }
            }
        }
}

    public List<Vector2> Astar(Vector2 startPos)
    {
        tileGoal = GetEnemiesPos(startPos);
        tileStart = GetPlayerPos();

        List<Vector2> path = new List<Vector2>();

        List<Node> openList = new List<Node> {tileStart};
        List<Node> closedList = new List<Node>();

        int crashValue = 5000;

        while (openList.Count > 0 && --crashValue > 0)
        {
            openList.OrderBy(x => x.CurrentCost + Vector2.Distance(x.Pos,tileGoal.Pos)).ToList();

            Node currentNode = openList[0];
            openList.RemoveAt(0);

            currentNode.HasBeenVisited = true;
            closedList.Add(currentNode);
            
            //early exit
            if (currentNode == tileGoal)
            {
                break;
            }
            else
            {
                foreach (Node currentNodeNeighbor in currentNode.Neighbors)
                {
                    float modifier;
                    //take the straight line
                    if (currentNode.Pos.x == currentNodeNeighbor.Pos.x ||
                        currentNode.Pos.y == currentNodeNeighbor.Pos.y)
                    {
                        modifier = 2;
                    }
                    //else if cannot go straight because obstacles go for diagonal
                    else
                    {
                        modifier = 4;
                    } 
                    
                    float newCost = modifier;
                    if (currentNodeNeighbor.CurrentCost == -1 && currentNodeNeighbor != tileStart || currentNodeNeighbor.CurrentCost > newCost)
                    {
                        currentNodeNeighbor.CameFrom = currentNode;
                        currentNodeNeighbor.CurrentCost = newCost;

                        openList.Add(currentNodeNeighbor);
                    }
                }
            }

            
        }
        
        if (crashValue <= 0)
        {
            Debug.Log("Mauvais");
        }

        {
            Node currentNode = tileGoal;
            while (currentNode.CameFrom != null)
            {
                currentNode.IsPath = true;
                path.Add(currentNode.Pos);
                currentNode = currentNode.CameFrom;
                
            }

            currentNode.IsPath = true;
        }
        
        ResetNode();
        return path;
    }
    
    //Function who return the closest free node on the graph of the player position
    public Node GetPlayerPos()
    {
        Node playerNode = new Node();

        float distance = 500;

        for (int x = _minX; x < _maxX; x++)
        {
            for (int y = _minY; y < _maxY; y++)
            {
                TileBase currentTile = tilemap.GetTile(new Vector3Int(x, y, 0));
                
                
                if (currentTile == null) continue;
                // if the distance between a node in the graph and the player is less than a distance and it's free
                if (Vector2.Distance(_graph[x - _minX, y - _minY].Pos, _playerPos.position) < distance && _graph[x - _minX, y - _minY].IsFree)
                {
                    //set the node as node of the player
                    playerNode = _graph[x - _minX, y - _minY];
                    //Set distance of the closest node of the player
                    distance = Vector2.Distance(_graph[x - _minX, y - _minY].Pos, _playerPos.position);
                }
            }
        }

        Debug.Log("Player position" + _playerPos.position);
        
        return playerNode;
    }

    //Function who return the closest free node of the enemy position
    public Node GetEnemiesPos(Vector2 enemiesPos)
    {
        
        Node enemiesNode = new Node();

        float distance = 500;

        for (int x = _minX; x < _maxX; x++)
        {
            for (int y = _minY; y < _maxY; y++)
            {
                TileBase currentTile = tilemap.GetTile(new Vector3Int(x, y, 0));

                if (currentTile == null) continue;
                // if the distance between a node in the graph and the enemy is less than a distance and it's free
                if (Vector2.Distance(_graph[x - _minX, y - _minY].Pos, enemiesPos) < distance && _graph[x - _minX, y - _minY].IsFree)
                {
                    //set the node as node of the enemy
                    enemiesNode = _graph[x - _minX, y - _minY];
                    //Set distance of the closest node of the enemy
                    distance = Vector2.Distance(_graph[x - _minX, y - _minY].Pos, enemiesPos);
                }
            }
        }

        Debug.Log("Enemies position : " + enemiesNode.Pos);


        return enemiesNode;
    }

    private void ResetNode()
    {
        foreach (Node node in _graph)
        {
            if (node == null) continue;
            node.CameFrom = new Node();
            node.CurrentCost = -1;
            node.HasBeenVisited = false;
            node.IsPath = false;
        }
    }

    void OnDrawGizmos()
    {
        if (gizmo)
        {
            if (_minX == _maxX || _minY == _maxY) return;

            Gizmos.DrawLine(new Vector3(_minX, _minY), new Vector3(_maxX, _minY));
            Gizmos.DrawLine(new Vector3(_maxX, _minY), new Vector3(_maxX, _maxY));
            Gizmos.DrawLine(new Vector3(_maxX, _maxY), new Vector3(_minX, _maxY));
            Gizmos.DrawLine(new Vector3(_minX, _maxY), new Vector3(_minX, _minY));

            foreach (Node node in _graph)
            {
                if (node == null) continue;

                Gizmos.color = node.IsFree ? Color.blue : Color.red;

                if (node.HasBeenVisited)
                {
                    Gizmos.color = Color.yellow;
                }

                if (node.IsPath)
                {
                    Gizmos.color = Color.green;
                }

                if (node == tileStart)
                {
                    Gizmos.color = Color.magenta;
                }

                if (node == tileGoal)
                {
                    Gizmos.color = Color.white;
                }

                Gizmos.DrawCube(node.Pos, Vector3.one * 0.35f);

                foreach (Node nodeNeighbor in node.Neighbors)
                {
                    Gizmos.DrawLine(node.Pos, nodeNeighbor.Pos);
                }
            }
        }
    }
}
