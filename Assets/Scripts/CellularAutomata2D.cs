using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata2D : MonoBehaviour
{
    
    [Range(0, 1000)] [SerializeField] int sizeX = 10; 
    [Range(0, 1000)] [SerializeField] int sizeY = 10;
    [Range(0, 100)] [SerializeField] int iteration = 10;
    
    struct Cell
    {
        public bool isAlive;
        public bool futureState;

        public int region;
    }

    struct Room
    {
        //public int 
    }

    Cell[,] cells;

    private bool _isRunning = false;
    private int _currentRegion = 0;
    
    void Start()
    {
        
        cells = new Cell[sizeX, sizeY];
        _isRunning = true;

        Generate();
    }
    
    void Generate()
    {
        Init();

        for (int i = 0; i < iteration; i++)
        {
            Cellular();
        }
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
        BoundsInt bounds = new BoundsInt(new Vector3Int(-1,-1,0),new Vector3Int(3,3,1));
        
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
                        Debug.Log(aliveNeighbours);
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
    
    void OnDrawGizmos()
    {
        if (!_isRunning) return;

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
        Gizmos.color = Color.white;

        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }

    void DrawDeadCell(Vector2 pos)
    {
        Gizmos.color = Color.black;
        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }
}
