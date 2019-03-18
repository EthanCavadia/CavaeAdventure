using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;

public class CellularAutomata : MonoBehaviour
{
    [Range(0, 1000)] [SerializeField] int sizeX = 10;
    [Range(0, 1000)] [SerializeField] int sizeY = 10;
    [Range(0, 100)] [SerializeField] int iteration = 10;

    struct Cell
    {
        public bool isAlive;
        public bool futureState;

        public int region;
        public int cellCount;
    }

    Cell[,] cells;

    bool isRunning = false;

    int currentRegion = 0;

    List<Color> colors;

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[sizeX, sizeY];

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

        isRunning = true;

        Generate();
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
                cells[x, y] = new Cell();

                cells[x, y].region = -1;

                float isAlive = Random.Range(0f, 1f);

                cells[x, y].isAlive = isAlive < 0.5f;
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
                int aliveNeighbours = 0;
                foreach (Vector2Int b in bounds.allPositionsWithin)
                {
                    if (b.x == 0 && b.y == 0) continue;
                    if (x + b.x < 0 || x + b.x >= sizeX || y + b.y < 0 || y + b.y >= sizeY) continue;

                    if (cells[x + b.x, y + b.y].isAlive)
                    {
                        aliveNeighbours++;
                    }
                }

                if (cells[x, y].isAlive && (aliveNeighbours == 1 || aliveNeighbours >= 4))
                {
                    cells[x, y].futureState = true;
                }
                else if (!cells[x, y].isAlive && aliveNeighbours >= 5)
                {
                    cells[x, y].futureState = true;
                }
                else
                {
                    cells[x, y].futureState = false;
                }
            }
        }

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                cells[x, y].isAlive = cells[x, y].futureState;
            }
        }
    }

    void GetRoom()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 4, 4, 2);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (!cells[x, y].isAlive) continue;
                if (cells[x, y].region != -1) continue;

                List<Vector2Int> openList = new List<Vector2Int>();
                List<Vector2Int> closedList = new List<Vector2Int>();

                openList.Add(new Vector2Int(x, y));

                while (openList.Count > 0)
                {
                    cells[openList[0].x, openList[0].y].region = currentRegion;
                    closedList.Add(openList[0]);

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
                        if (!cells[pos.x, pos.y].isAlive) continue;

                        //check region not yet associated
                        if (cells[pos.x, pos.y].region != -1) continue;

                        //Check if already visited
                        if (closedList.Contains(pos)) continue;

                        //Check if already set to be visited
                        if (openList.Contains(pos)) continue; //Error

                        openList.Add(new Vector2Int(pos.x, pos.y));
                        
                    }
                    
                    Debug.Log(openList.Count);
                    openList.RemoveAt(0);
                }
                
                currentRegion++;
                Debug.Log(currentRegion);
            }
        }
    }
    
    void Path() {
        Cell startCell = new Cell();
        startCell.isAlive = true;
        while (startCell.isAlive) {
            //Debug.Log("start Cell finding");
            startCell = cells[Random.Range(5, sizeX-5), Random.Range(5, sizeY-5)];
        }
        //Debug.Log("start Cell Found");

        Cell endCell = new Cell();
        endCell.isAlive = true;
        while (endCell.isAlive) {
            //Debug.Log("end Cell finding");
            endCell = cells[Random.Range(5, sizeX-5), Random.Range(5, sizeY-5)];
        }
        // Debug.Log("end Cell Found");

        //GetComponent<BFS>().PathFinder(cells, startCell, endCell);
    }
    
    void OnDrawGizmos()
    {
        if (!isRunning) return;
        
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (cells[x, y].isAlive)
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
        Gizmos.color = cells[pos.x, pos.y].region < 0 ? Color.clear : colors[cells[pos.x, pos.y].region % colors.Count];

        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }

    void DrawDeadCell(Vector2 pos)
    {
        Gizmos.color = Color.black;
        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }
}
