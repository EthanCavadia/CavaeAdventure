using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class CellularAutomata : MonoBehaviour
{
    [Header("Map ")] 
    [Range(0, 1000)] public int sizeX = 100;
    [Range(0, 1000)] public int sizeY = 50;
    [Range(0, 1)] [SerializeField] private float fillPercent = 0.5f;
    [Range(0, 100)] [SerializeField] private int iteration = 10;
    [SerializeField] private int minimumCellInRoom = 30;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase groundTile;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private bool showPaths;
    [SerializeField] private bool drawDebugGizmo;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemiesPrefab;
    [SerializeField] private int seed;
    struct Cell
    {
        public bool IsAlive;
        public bool FutureState;
        public Vector2Int Position;
        public int Region;
    }

    class Room
    {
        public List<Cell> Cells;
        public List<Room> ClosestRoom;
        public List<Tunnel> Tunnels;
        public Vector2 RoomCenter;
        public Vector2 playerSpawn;
        public bool occupied;
    }

    private struct Tunnel
    {
        public Cell StartCell, EndCell;
    }

    Cell[,] _cells;
    List<Room> rooms = new List<Room>();
    private int _playerCount = 1;
    private int _enemiesCount = 10;
    private bool _isRunning = false;
    private int _currentRegion = 0;
    private float offset = 2;
    private List<Color> colors;


    public static CellularAutomata instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {

        if (seed == 0)
        {
            seed = Random.Range(0 , 1000000);
        }
        
        Random.InitState(seed);
        

        colors = new List<Color>
        {
            Color.white,
            Color.blue,
            Color.cyan,
            Color.gray,
            Color.green,
            Color.magenta,
            Color.red,
            Color.yellow
        };
        _cells = new Cell[sizeX, sizeY];
        _isRunning = true;
        
        Generate();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Generate()
    {
        Init();

        for (int i = 0; i < iteration; i++)
        {
            Cellular();
            
        }

        GetRoom();
        DrawMap();
    }

    private void Init()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                _cells[x, y] = new Cell();

                _cells[x, y].Region = -1;

                float isAlive = Random.Range(0f, 1f);

                _cells[x, y].IsAlive = isAlive < fillPercent;
                _cells[x, y].Position = new Vector2Int(x, y);
            }
        }
    }

    private void Cellular()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (x == 1 || x == sizeX - 1 || y == 1 || y == sizeY - 1)
                {
                    _cells[x, y].IsAlive = false;
                }

                int aliveNeighbours = 0;
                foreach (Vector2Int b in bounds.allPositionsWithin)
                {
                    if (b.x == 0 && b.y == 0) continue;
                    if (x + b.x < 0 || x + b.x >= sizeX || y + b.y < 0 || y + b.y >= sizeY) continue;

                    if (_cells[x + b.x, y + b.y].IsAlive)
                    {
                        aliveNeighbours++;
                    }
                }

                if (_cells[x, y].IsAlive && (aliveNeighbours == 1 || aliveNeighbours >= 4))
                {
                    _cells[x, y].FutureState = true;
                }
                else if (!_cells[x, y].IsAlive && aliveNeighbours >= 5)
                {
                    _cells[x, y].FutureState = true;
                }
                else
                {
                    _cells[x, y].FutureState = false;
                }
            }
        }

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                _cells[x, y].IsAlive = _cells[x, y].FutureState;
            }
        }
    }

    private void GetRoom()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (!_cells[x, y].IsAlive) continue;
                if (_cells[x, y].Region != -1) continue;

                List<Vector2Int> openList = new List<Vector2Int>();
                List<Vector2Int> closedList = new List<Vector2Int>();

                openList.Add(new Vector2Int(x, y));
                Room room = new Room();
                room.Cells = new List<Cell>();

                while (openList.Count > 0)
                {
                    _cells[openList[0].x, openList[0].y].Region = _currentRegion;
                    closedList.Add(openList[0]);
                    room.Cells.Add(_cells[openList[0].x, openList[0].y]);

                    foreach (Vector2Int b in bounds.allPositionsWithin)
                    {
                        //Check not self
                        if (b.x == 0 && b.y == 0) continue;

                        //Check if is on cross
                        if (b.x != 0 && b.y != 0) continue;

                        Vector2Int pos = new Vector2Int(openList[0].x + b.x, openList[0].y + b.y);

                        //Check inside bounds
                        if (pos.x < 0 || pos.x >= sizeX || pos.y < 0 || pos.y >= sizeY) continue;

                        //Check is alive
                        if (!_cells[pos.x, pos.y].IsAlive) continue;

                        //check region not yet associated
                        if (_cells[pos.x, pos.y].Region != -1) continue;

                        //Check if already visited
                        if (closedList.Contains(pos)) continue;

                        //Check if already set to be visited
                        if (openList.Contains(pos)) continue; //Error

                        openList.Add(new Vector2Int(pos.x, pos.y));
                        room.Cells.Add(_cells[pos.x, pos.y]);
                    }

                    openList.RemoveAt(0);
                }

                rooms.Add(room);
                _currentRegion++;
            }
        }

        List<Room> smallRooms = new List<Room>();
        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];

            if (currentRoom.Cells.Count < minimumCellInRoom)
            {
                //for each room with less than the minimum cell count 
                
                CleanRoom(currentRoom);
                smallRooms.Add(currentRoom);
            }
        }

        foreach (Room r in smallRooms)
        {
            rooms.Remove(r);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];

            Vector2 center = GetRoomCenter(currentRoom);
            rooms[i].RoomCenter = center;
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];

            rooms[i].ClosestRoom = GetClosestRoom(currentRoom);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];

            rooms[i].Tunnels = GetPath(currentRoom.ClosestRoom, currentRoom);
        }

        foreach (Room r in rooms)
        {
            foreach (Tunnel t in r.Tunnels)
            {
                List<Cell> tunnel = TunnelToEnd(t);
                CreateTunnel(tunnel);
            }
        }

        int randomNb = Random.Range(0, rooms[0].Cells.Count);
        Vector3 spawnPosition = new Vector3(rooms[0].Cells[randomNb].Position.x, rooms[0].Cells[randomNb].Position.y);
        
        
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        rooms[0].occupied = true;


        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];
            int random = Random.Range(0, currentRoom.Cells.Count);
            Vector3 cellsPos = new Vector3(rooms[i].Cells[random].Position.x, rooms[i].Cells[random].Position.y);
            
            if (_enemiesCount > 0 && !currentRoom.occupied)
            {
                _enemiesCount--;
                Instantiate(enemiesPrefab, cellsPos, Quaternion.identity);
                Debug.Log("enemies spawn position" + cellsPos);
            }
        }
    }


    private void CleanRoom(Room room)
    {
        foreach (Cell c in room.Cells)
        {
            _cells[c.Position.x, c.Position.y].IsAlive = false;
        }
    }

    private Vector2 GetRoomCenter(Room room)
    {
        Vector2 roomCenter = new Vector2();

        foreach (Cell cell in room.Cells)
        {
            roomCenter.x += cell.Position.x;
            roomCenter.y += cell.Position.y;
        }

        roomCenter.x /= room.Cells.Count;
        roomCenter.y /= room.Cells.Count;

        return roomCenter;
    }

    private List<Room> GetClosestRoom(Room room)
    {
        float bestDistance = 500;
        List<Room> closestRoom = new List<Room>();
        Room currentClosestRoom = new Room();

        foreach (Room r in rooms)
        {
            if (r != room)
            {
                float d = Distance(r.RoomCenter, room.RoomCenter);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    currentClosestRoom = r;
                }
            }
        }
        closestRoom.Add(currentClosestRoom);
        
        currentClosestRoom = new Room();
        bestDistance = 500;
        foreach (Room r in rooms)
        {
            if (r != room && !closestRoom.Contains(r))
            {
                float d = Distance(r.RoomCenter, room.RoomCenter);
                if (d < bestDistance)
                {
                    currentClosestRoom = r;
                    bestDistance = d;
                }
            }
        }

        closestRoom.Add(currentClosestRoom);
        return closestRoom;
    }

    private List<Tunnel> GetPath(List<Room> closestRooms, Room currentRoom)
    {
        List<Tunnel> tunnels = new List<Tunnel>();
        
        foreach (Room closestRoom in closestRooms)
        {
            Tunnel tunnel = new Tunnel();
            Cell currentClosestCell = new Cell();
            float bestDistance = 500;

            foreach (Cell c in currentRoom.Cells)
            {
                float dist = Distance(c.Position, closestRoom.RoomCenter);
                if (dist < bestDistance)
                {
                    currentClosestCell = c;
                    bestDistance = dist;
                }
            }


            Cell otherClosestCell = new Cell();
            float otherClosestDistance = 500;
            
            foreach (Cell c in closestRoom.Cells)
            {
                float dist = Distance(c.Position, currentRoom.RoomCenter);
                if (dist < otherClosestDistance)
                {
                    otherClosestCell = c;
                    otherClosestDistance = dist;
                }
            }

            tunnel.StartCell = currentClosestCell;
            tunnel.EndCell = otherClosestCell;
            tunnels.Add(tunnel);
        }

        return tunnels;
    }

    private void CreateTunnel(List<Cell> tunnel)
    {
        foreach (Cell cell in tunnel)
        {
            if (!cell.IsAlive)
            {
                _cells[cell.Position.x, cell.Position.y].IsAlive = true;
                List<Cell> neighbours = FindCells(cell);

                foreach (Cell c in neighbours)
                {
                    if (!c.IsAlive)
                    {
                        _cells[c.Position.x, c.Position.y].IsAlive = true;
                    }
                }
            }
        }
    }

    List<Cell> TunnelToEnd(Tunnel tunnel)
    {
        List<Cell> tunnelTo = new List<Cell>();
        tunnelTo.Add(tunnel.StartCell);

        //safe exit
        int iteration = 0;
        int maxIteration = 100;

        while (tunnelTo[tunnelTo.Count - 1].Position != tunnel.EndCell.Position && iteration < maxIteration)
        {
            iteration++;
            List<Cell> neighbours = FindCells(tunnelTo[tunnelTo.Count - 1]);
            float lowestDist = 500;
            Cell lowestCell = new Cell();

            foreach (Cell c in neighbours)
            {
                if (Distance(c.Position, tunnel.EndCell.Position) < lowestDist)
                {
                    lowestDist = Distance(c.Position, tunnel.EndCell.Position);
                    lowestCell = c;
                }
            }

            tunnelTo.Add(lowestCell);
        }

        return tunnelTo;
    }

    private List<Cell> FindCells(Cell cell)
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        List<Cell> allCells = new List<Cell>();
        foreach (Vector2Int b in bounds.allPositionsWithin)
        {
            //Not check itself
            if (b.x == 0 && b.y == 0) continue;
            //Not check if out of bounds
            if (cell.Position.x + b.x < 0 || cell.Position.x + b.x >= sizeX || cell.Position.y + b.y < 0 ||
                cell.Position.y + b.y >= sizeY) continue;

            //Add neighbours to list
            allCells.Add(_cells[cell.Position.x + b.x, cell.Position.y + b.y]);
        }

        return allCells;
    }

    float Distance(Vector2 start, Vector2 end)
    {
        float distance = Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y);
        return distance;
    }

    void DrawMap()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (!_cells[x, y].IsAlive)
                {
                    wallTilemap.SetTile(new Vector3Int(x,y,0),wallTile);
                }

                if (_cells[x,y].IsAlive)
                {
                    groundTilemap.SetTile(new Vector3Int(x,y,0), groundTile);
                }
                
                backgroundTilemap.SetTile(new Vector3Int(x,y,0),groundTile);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!_isRunning) return;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (_cells[x, y].IsAlive && drawDebugGizmo)
                {
                     DrawAliveCell(_cells[x,y].Position);
                }
               
            }

            if (showPaths)
            {
                foreach (Room r in rooms)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(r.RoomCenter, 1);

                    foreach (Tunnel path in r.Tunnels)
                    {
                        Vector3 start = new Vector3(path.StartCell.Position.x, path.StartCell.Position.y, 0);
                        Vector3 end = new Vector3(path.EndCell.Position.x, path.EndCell.Position.y, 0);
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawSphere(start, 0.5f);
                        Gizmos.color = Color.blue;

                        Gizmos.DrawSphere(end, 0.5f);
                        Gizmos.DrawLine(start, end);

                    }
                }
            }
        }
    }

    void DrawAliveCell(Vector2Int pos)
    {
        Gizmos.color = _cells[pos.x, pos.y].Region < 0 ? Color.clear : colors[_cells[pos.x, pos.y].Region % colors.Count];

        Gizmos.DrawCube(new Vector3Int(pos.x, pos.y, 0),Vector2.one);
        
    }
}