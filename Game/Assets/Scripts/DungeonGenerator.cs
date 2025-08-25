using System;
using System.Collections.Generic;
using UnityEngine;
public class Room
{
    public int startX;
    public int startY;
    public int height;
    public int width;

}

public class DungeonGenerator : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public int numberOfPartitions = 10;
    public int corridorWidth = 2;
    public int[,] map;
    public List<Room> rooms = new List<Room>();
    public GameObject wallPrefab;
    public GameObject playerPrefab;
    void Awake()
    {
        map = new int[width, height];
        GenerateMap();
    }

    private void GenerateMap()
    {
        InitializeMap();
        // GenerateRooms();
        SampleOpenRoom();
        Generate3DMap();
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        List<Vector2Int> openTiles = new List<Vector2Int>();
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] == 0)
                {
                    openTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        if (openTiles.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, openTiles.Count);
            Vector2Int pos = openTiles[index];
            Vector3 spawnPosition = new Vector3(pos.x, 1, pos.y);
            Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void SampleOpenRoom()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                map[x, y] = 0;
            }
        }
        return;
    }

    private void Generate3DMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    Vector3 position = new Vector3(x, 0, y);
                    var gameObject = Instantiate(wallPrefab, position, Quaternion.identity);
                    gameObject.transform.parent = this.transform;
                }
            }
        }
    }

    private void GenerateRooms()
    {
        for (int i = 0; i < numberOfPartitions; i++)
        {
            List<Room> roomsToSplit = new List<Room>(rooms);
            foreach (Room room in roomsToSplit)
            {
                if (room.width > 8 && room.height > 8)
                {
                    int splitChance = UnityEngine.Random.Range(0, 100);
                    
                    if (splitChance < 75)
                    {
                        SplitRoom(room);
                    }
                }
            }
        }
        
        CarveRooms();
        GenerateCorridors();
    }

    private void SplitRoom(Room room)
    {
        if (room.width <= 8 || room.height <= 8) return;
        
        bool splitHorizontally = UnityEngine.Random.Range(0, 2) == 0;

        if (room.width > room.height * 1.25f) splitHorizontally = false;
        if (room.height > room.width * 1.25f) splitHorizontally = true;

        if (splitHorizontally)
        {
            int splitY = UnityEngine.Random.Range(room.startY + 4, room.startY + room.height - 4);
            Room topRoom = new Room
            {
                startX = room.startX,
                startY = room.startY,
                width = room.width,
                height = splitY - room.startY
            };
            Room bottomRoom = new Room
            {
                startX = room.startX,
                startY = splitY,
                width = room.width,
                height = room.startY + room.height - splitY
            };
            rooms.Remove(room);
            rooms.Add(topRoom);
            rooms.Add(bottomRoom);
        }
        else
        {
            int splitX = UnityEngine.Random.Range(room.startX + 4, room.startX + room.width - 4);
            Room leftRoom = new Room
            {
                startX = room.startX,
                startY = room.startY,
                width = splitX - room.startX,
                height = room.height
            };
            Room rightRoom = new Room
            {
                startX = splitX,
                startY = room.startY,
                width = room.startX + room.width - splitX,
                height = room.height
            };
            rooms.Remove(room);
            rooms.Add(leftRoom);
            rooms.Add(rightRoom);
        }
    }

    private void CarveRooms()
    {
        foreach (Room room in rooms)
        {
            // Limit room size to be more reasonable
            int maxRoomWidth = Mathf.Min(room.width - 4, 12); // Cap at 12 units wide
            int maxRoomHeight = Mathf.Min(room.height - 4, 12); // Cap at 12 units tall
            
            int roomWidth = UnityEngine.Random.Range(4, maxRoomWidth + 1);
            int roomHeight = UnityEngine.Random.Range(4, maxRoomHeight + 1);
            
            int roomStartX = room.startX + UnityEngine.Random.Range(2, room.width - roomWidth - 1);
            int roomStartY = room.startY + UnityEngine.Random.Range(2, room.height - roomHeight - 1);
            
            for (int x = roomStartX; x < roomStartX + roomWidth; x++)
            {
                for (int y = roomStartY; y < roomStartY + roomHeight; y++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        map[x, y] = 0;
                    }
                }
            }
            
            room.startX = roomStartX;
            room.startY = roomStartY;
            room.width = roomWidth;
            room.height = roomHeight;
        }
    }

    private void GenerateCorridors()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Room roomA = rooms[i];
            Room roomB = rooms[i + 1];
            
            int centerAX = roomA.startX + roomA.width / 2;
            int centerAY = roomA.startY + roomA.height / 2;
            int centerBX = roomB.startX + roomB.width / 2;
            int centerBY = roomB.startY + roomB.height / 2;
            
            CreateCorridor(centerAX, centerAY, centerBX, centerAY);
            CreateCorridor(centerBX, centerAY, centerBX, centerBY);
        }
    }

    private void CreateCorridor(int x1, int y1, int x2, int y2)
    {
        int startX = Mathf.Min(x1, x2) - corridorWidth / 2;
        int endX = Mathf.Max(x1, x2) + corridorWidth / 2;
        int startY = Mathf.Min(y1, y2) - corridorWidth / 2;
        int endY = Mathf.Max(y1, y2) + corridorWidth / 2;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    void InitializeMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = 1;
            }
        }
        rooms.Clear();
        rooms.Add(new Room { startX = 0, startY = 0, width = width, height = height });
    }
}