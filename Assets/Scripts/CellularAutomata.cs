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
    [Range(0, 1000)] [SerializeField] private int sizeX = 100;
    [Range(0, 1000)] [SerializeField] private int sizeY = 50;
    [Range(0,1)] [SerializeField] private float fillPercent = 0.5f;
    [Range(0, 100)] [SerializeField] private int iteration = 10;
    [SerializeField] private int minimumCellInRoom = 30;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase groundTile;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap groundTilemap;
    struct Cell
    {
        public bool IsAlive;
        public bool FutureState;
        public Vector2Int Position;
        public int Region;
    }

    class Room
    {
        public int RoomID;
        public List<Cell> Cells;
        public List<Room> ClosestRoom;
        public List<Tunnel> Tunnels;
        public Vector2 RoomCenter;
    }

    struct Tunnel
    {
        public Cell StartCell, EndCell;
    }

    Cell[,] _cells;
    List<Room> rooms = new List<Room>();

    private bool _isRunning = false;
    private int _currentRegion = 0;
    
    private List<Color> _colors;

    // Start is called before the first frame update
    void Start()
    {
        _cells = new Cell[sizeX, sizeX];

        _colors = new List<Color>
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

    void Generate()
    {
        Init();

        for (int i = 0; i < iteration; i++)
        {
            Cellular();
        }

        GetRoom();
        Path();
        
    }

    void Init()
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

    void Cellular()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (x == 1 || x == sizeX-1 || y == 1 || y == sizeY-1)
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

                room.RoomID = _currentRegion;
                rooms.Add(room);
                _currentRegion++;
            }
        }
        
        List<Room> smallRooms = new List<Room>();
        for(int i = 0; i < rooms.Count; i++) {
            Room currentRoom = rooms[i];
            
            if (currentRoom.Cells.Count < minimumCellInRoom) {
                CleanRoom(currentRoom);
                smallRooms.Add(currentRoom);
            }
        }

        foreach (Room r in smallRooms)
        {
            smallRooms.Remove(r);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];

            Vector2 center = Tunneling(currentRoom);
            Debug.Log(center);
            rooms[i].RoomCenter = center;
            
        }
    }


    void CleanRoom(Room room)
    {
        foreach (Cell c in room.Cells)
        {
            _cells[c.Position.x, c.Position.y].IsAlive = false;
        }
    }
    
    void Path() {
        Cell startCell = new Cell();
        startCell.IsAlive = true;
        while (startCell.IsAlive) {
            //Debug.Log("start Cell finding");
            startCell = _cells[Random.Range(5, sizeX-5), Random.Range(5, sizeY-5)];
        }
        //Debug.Log("start Cell Found");

        Cell endCell = new Cell();
        endCell.IsAlive = true;
        while (endCell.IsAlive) {
            //Debug.Log("end Cell finding");
            endCell = _cells[Random.Range(5, sizeX-5), Random.Range(5, sizeY-5)];
        }
        // Debug.Log("end Cell Found");

        //GetComponent<BFS>().PathFinder(cells, startCell, endCell);
    }

    private Vector2 Tunneling(Room room)
    {
        Vector2Int roomCenter = new Vector2Int();

        foreach (Cell cell in room.Cells)
        {
            roomCenter.x += cell.Position.x;
            roomCenter.y += cell.Position.y;
        }

        roomCenter.x /= room.Cells.Count;
        roomCenter.y /= room.Cells.Count;
        
        return roomCenter;
    }
    
    
    
    void OnDrawGizmos()
    {
        if (!_isRunning) return;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (!_cells[x, y].IsAlive)
                {
                    wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
            }
        }

        foreach (Room r in rooms)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(r.RoomCenter,1);
        }
    }
}