using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class CellularAutomata2D : MonoBehaviour
{
    [Range(0, 1000)] public int size = 10;
    [Range(0, 100)] [SerializeField] int iteration = 10;
    [SerializeField] int maxRoomSearch = 20;
    [SerializeField] bool showRooms, showPaths;

    public struct Cell
    {
        public bool isAlive;
        public bool futureState;
        public Vector2Int position;
        public Vector2Int lastPosition;
        public int region;
    }

    class Room
    {
        public int ID;
        public List<Cell> cells;
        public Vector2 center;
        public List<Room> closestRooms;
        public List<Path> paths;
    }

    struct Path
    {
        public Cell start, end;
    }

    Cell[,] cells;
    List<Room> rooms = new List<Room>();

    bool isRunning = false;

    int currentRegion = 0;

    List<Color> colors;

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[size, size];

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
    }

    void Init()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                cells[x, y] = new Cell();

                cells[x, y].region = -1;

                float isAlive = Random.Range(0f, 1f);

                cells[x, y].isAlive = isAlive < 0.5f;
                cells[x, y].position = new Vector2Int(x, y);
            }
        }
    }

    void Cellular()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int aliveNeighbours = 0;
                foreach (Vector2Int b in bounds.allPositionsWithin)
                {
                    if (b.x == 0 && b.y == 0) continue;
                    if (x + b.x < 0 || x + b.x >= size || y + b.y < 0 || y + b.y >= size) continue;

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

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                cells[x, y].isAlive = cells[x, y].futureState;
            }
        }
    }

    void GetRoom()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (!cells[x, y].isAlive) continue;
                if (cells[x, y].region != -1) continue;

                List<Vector2Int> openList = new List<Vector2Int>();
                List<Vector2Int> closedList = new List<Vector2Int>();

                openList.Add(new Vector2Int(x, y));
                Room room = new Room();
                room.cells = new List<Cell>();

                while (openList.Count > 0)
                {
                    cells[openList[0].x, openList[0].y].region = currentRegion;
                    closedList.Add(openList[0]);
                    room.cells.Add(cells[openList[0].x, openList[0].y]);

                    foreach (Vector2Int b in bounds.allPositionsWithin)
                    {
                        //Check not self
                        if (b.x == 0 && b.y == 0) continue;

                        //Check if is on cross
                        if (b.x != 0 && b.y != 0) continue;

                        Vector2Int pos = new Vector2Int(openList[0].x + b.x, openList[0].y + b.y);

                        //Check inside bounds
                        if (pos.x < 0 || pos.x >= size || pos.y < 0 || pos.y >= size) continue;

                        //Check is alive
                        if (!cells[pos.x, pos.y].isAlive) continue;

                        //check region not yet associated
                        if (cells[pos.x, pos.y].region != -1) continue;

                        //Check if already visited
                        if (closedList.Contains(pos)) continue;

                        //Check if already set to be visited
                        if (openList.Contains(pos)) continue; //Error

                        openList.Add(new Vector2Int(pos.x, pos.y));
                        room.cells.Add(cells[pos.x, pos.y]);

                    }

                    openList.RemoveAt(0);
                }

                room.ID = currentRegion;
                rooms.Add(room);
                currentRegion++;
            }
        }

        //Remove smaller rooms
        List<Room> roomsToRemove = new List<Room>();
        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];
            if (currentRoom.cells.Count < 20)
            {
                CleanRoom(currentRoom);
                roomsToRemove.Add(currentRoom);
            }
        }

        //Remove them from the list of rooms
        foreach (Room r in roomsToRemove)
        {
            rooms.Remove(r);
        }


        //Find rooms center
        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];

            Vector2 center = FindCenterOfRoom(currentRoom);
            Debug.Log(center);
            rooms[i].center = center;
        }

        //Find room closest Room
        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];
            rooms[i].closestRooms = FindClosestRooms(currentRoom);
        }

        //Find all paths
        for (int i = 0; i < rooms.Count; i++)
        {
            Room currentRoom = rooms[i];
            rooms[i].paths = FindPaths(currentRoom.closestRooms, currentRoom);
        }

        foreach (Room r in rooms)
        {
            foreach (Path p in r.paths)
            {
                List<Cell> path = pathToEnd(p);
                ClearPath(path);
            }
        }
        /*foreach(Room r in rooms) {
            Cell start = r.closestCell;
            Cell end = r.closestRoom.closestCell;

            List<Vector2> path = GetComponent<BFS>().PathFinder(cells, start, end);
            ClearPath(path);
        }*/

    }

    //Remove Smaller rooms
    void CleanRoom(Room room)
    {
        foreach (Cell c in room.cells)
        {
            cells[c.position.x, c.position.y].isAlive = false;
        }
    }

    #region Draw

    void OnDrawGizmos()
    {
        if (!isRunning) return;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
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

        if (showPaths)
        {
            foreach (Room r in rooms)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(r.center, 1);

                foreach (Path path in r.paths)
                {
                    Vector3 start = new Vector3(path.start.position.x, path.start.position.y, 0);
                    Vector3 end = new Vector3(path.end.position.x, path.end.position.y, 0);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(start, 0.5f);
                    Gizmos.color = Color.blue;

                    Gizmos.DrawSphere(end, 0.5f);
                    Gizmos.DrawLine(start, end);

                }
            }
        }

        /*foreach(Room r in rooms) {
            foreach(Room room in r.closestRooms) {
                Gizmos.color = room.ID < 0 ? Color.clear : colors[room.ID % colors.Count];

                Gizmos.DrawLine(room.center, r.center);
            }
        }*/


    }

    void DrawAliveCell(Vector2Int pos)
    {
        if (showRooms)
        {
            Gizmos.color = cells[pos.x, pos.y].region < 0
                ? Color.clear
                : colors[cells[pos.x, pos.y].region % colors.Count];
        }
        else
        {
            Gizmos.color = Color.white;
        }

        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }

    void DrawDeadCell(Vector2 pos)
    {
        Gizmos.color = Color.black;
        Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), Vector2.one);
    }

    #endregion

    //Find the center of the room
    Vector2 FindCenterOfRoom(Room room)
    {
        Vector2 roomCenter = new Vector2();

        foreach (Cell cell in room.cells)
        {
            roomCenter.x += cell.position.x;
            roomCenter.y += cell.position.y;
        }

        roomCenter.x /= room.cells.Count;
        roomCenter.y /= room.cells.Count;

        return roomCenter;
    }

    //Find the closest room to the center of this room
    List<Room> FindClosestRooms(Room room)
    {
        float closest = 100;
        List<Room> closestRoom = new List<Room>();
        Room currentClosest = new Room();
        foreach (Room r in rooms)
        {
            if (r != room)
            {
                float dist = Distance(r.center, room.center);
                if (dist < closest)
                {
                    closest = dist;
                    currentClosest = r;
                }
            }
        }

        closestRoom.Add(currentClosest);

        currentClosest = new Room();
        closest = 100;
        foreach (Room r in rooms)
        {
            if (r != room && !closestRoom.Contains(r))
            {
                float dist = Distance(r.center, room.center);
                if (dist < closest)
                {
                    currentClosest = r;
                    closest = dist;
                }
            }
        }

        closestRoom.Add(currentClosest);

        return closestRoom;
    }

    List<Path> FindPaths(List<Room> closestRooms, Room myRoom)
    {
        List<Path> paths = new List<Path>();

        foreach (Room closestRoom in closestRooms)
        {
            Path path = new Path();

            //Find myClosestCell
            Cell myClosestCell = new Cell();
            float myClosestDistance = 100;
            foreach (Cell c in myRoom.cells)
            {
                float dist = Distance(c.position, closestRoom.center);
                if (dist < myClosestDistance)
                {
                    myClosestCell = c;
                    myClosestDistance = dist;
                }
            }

            //Find otherClosestCell
            Cell otherClosestCell = new Cell();
            float otherClosestDistance = 100;
            foreach (Cell c in closestRoom.cells)
            {
                float dist = Distance(c.position, myRoom.center);
                if (dist < otherClosestDistance)
                {
                    otherClosestCell = c;
                    otherClosestDistance = dist;
                }
            }

            path.start = myClosestCell;
            path.end = otherClosestCell;
            paths.Add(path);
        }

        return paths;


    }

    float Distance(Vector2 start, Vector2 end)
    {
        float distance = Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y);
        return distance;
    }


    void ClearPath(List<Path> path)
    {
        List<Cell> pathCell = new List<Cell>();
        foreach (Path p in path)
        {

        }
    }

    Vector3 Vector2IntToVector3(Vector2Int vect)
    {
        Vector3 v = new Vector3(vect.x, vect.y, 0);
        return v;
    }


    void ClearPath(List<Cell> path)
    {
        foreach (Cell c in path)
        {
            if (!c.isAlive)
            {
                cells[c.position.x, c.position.y].isAlive = true;
                List<Cell> neighbors = findN(c);
                foreach (Cell nc in neighbors)
                {
                    if (!nc.isAlive)
                    {
                        cells[nc.position.x, nc.position.y].isAlive = true;
                    }
                }
            }
        }
    }

    List<Cell> pathToEnd(Path path)
    {
        List<Cell> pathTo = new List<Cell>();
        pathTo.Add(path.start);
        int error = 0;
        int maxerror = 50;

        while (pathTo[pathTo.Count - 1].position != path.end.position && error < maxerror)
        {
            error++;
            List<Cell> neighbors = findN(pathTo[pathTo.Count - 1]);
            float lowestD = 500;
            Cell lowestCell = new Cell();
            foreach (Cell c in neighbors)
            {
                if (Distance(c.position, path.end.position) < lowestD)
                {
                    lowestD = Distance(c.position, path.end.position);
                    lowestCell = c;
                }
            }

            pathTo.Add(lowestCell);

        }

        return pathTo;
    }

    List<Cell> findN(Cell cell)
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        List<Cell> allCells = new List<Cell>();
        foreach (Vector2Int b in bounds.allPositionsWithin)
        {
            if (b.x == 0 && b.y == 0) continue;
            if (cell.position.x + b.x < 0 || cell.position.x + b.x >= size || cell.position.y + b.y < 0 ||
                cell.position.y + b.y >= size) continue;

            allCells.Add(cells[cell.position.x + b.x, cell.position.y + b.y]);

        }
        /*Cell upperCell, lowerCell, rightCell, leftCell, upRightCell, downRightCell, upLeftCell, downLeftCell;
        upperCell = cells[cell.position.x, cell.position.y + 1];
        lowerCell = cells[cell.position.x, cell.position.y - 1];
        rightCell = cells[cell.position.x + 1, cell.position.y];
        leftCell = cells[cell.position.x - 1, cell.position.y];

        upRightCell = cells[cell.position.x + 1, cell.position.y + 1];
        downRightCell = cells[cell.position.x + 1, cell.position.y - 1];
        upLeftCell = cells[cell.position.x - 1, cell.position.y + 1];
        downLeftCell = cells[cell.position.x - 1, cell.position.y - 1];

        List<Cell> allCells = new List<Cell>();

        allCells.Add(upperCell);
        allCells.Add(lowerCell);
        allCells.Add(rightCell);
        allCells.Add(leftCell);

        allCells.Add(upRightCell);
        allCells.Add(downRightCell);
        allCells.Add(upLeftCell);
        allCells.Add(downLeftCell);*/

        return allCells;
    }

}