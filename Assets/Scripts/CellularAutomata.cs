using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CellularAutomata : MonoBehaviour
{
    [Range(0, 1000)] [SerializeField] private int sizeX = 100;
    [Range(0, 1000)] [SerializeField] private int sizeY = 50;
    [Range(0, 100)] [SerializeField] private int iteration = 10;
    [Range(0,1)] [SerializeField] private float fillPercent = 0.5f;
    struct Cell
    {
        public bool IsAlive;
        public bool FutureState;
        public Vector2Int Position;
        public int Region;
    }

    struct Room
    {
        public List<Cell> Cells;
        public int NbCells;
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
            Generate();
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
                if (x == 0 || x == sizeX-1 || y == 0 || y == sizeY-1)
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
                    room.NbCells++;
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
                        room.NbCells++;
                        room.Cells.Add(_cells[pos.x, pos.y]);

                    }

                    openList.RemoveAt(0);
                }
                
                rooms.Add(room);
                _currentRegion++;
            }
        }

        Debug.Log("There is : " + (rooms.Count - 1) + " Rooms");
        int i = 1;
        foreach (Room r in rooms)
        {
            Debug.Log("In room " + i + " There is " + r.Cells.Count);
            if (r.Cells.Count < 30)
            {
                CleanRoom(r);
            }

            i++;
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
    
    void OnDrawGizmos()
    {
        if (!_isRunning) return;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (_cells[x, y].IsAlive)
                {
                    DrawAliveCell(new Vector2Int(x, y));
                }
                else
                {
                    DrawDeadCell(new Vector2(x, y));
                }
            }
        }
    }

    void DrawAliveCell(Vector2Int pos)
    {
        //Gizmos.color = _cells[pos.x, pos.y].Region < 0 ? Color.clear : _colors[_cells[pos.x, pos.y].Region % _colors.Count];
        Gizmos.color = Color.white;
        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }

    void DrawDeadCell(Vector2 pos)
    {
        Gizmos.color = Color.black;
        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }
}