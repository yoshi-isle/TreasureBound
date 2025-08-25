using System.Collections.Generic;
using UnityEngine;

public class DungeonMaker : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int dungeonWidth = 50;
    public int dungeonHeight = 50;
    public int numberOfRooms = 8;
    public int minRoomSize = 4;
    public int maxRoomSize = 10;
    
    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;
    
    // Internal variables
    private int[,] dungeonMap;
    private List<Room> rooms = new List<Room>();
    
    // Tile types
    private const int WALL = 1;
    private const int FLOOR = 0;
    
    [System.Serializable]
    public class Room
    {
        public int x, y, width, height;
        
        public Room(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        
        public Vector2Int GetCenter()
        {
            return new Vector2Int(x + width / 2, y + height / 2);
        }
        
        public bool Overlaps(Room other)
        {
            return x < other.x + other.width && x + width > other.x &&
                   y < other.y + other.height && y + height > other.y;
        }
    }
    
    void Start()
    {
        GenerateDungeon();
    }
    
    void Update()
    {
        // Press R to regenerate dungeon
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearDungeon();
            GenerateDungeon();
        }
    }
    
    void GenerateDungeon()
    {
        InitializeMap();
        GenerateRooms();
        GenerateCorridors();
        BuildDungeonMesh();
        SpawnPlayer();
    }
    
    void InitializeMap()
    {
        dungeonMap = new int[dungeonWidth, dungeonHeight];
        rooms.Clear();
        
        // Fill with walls
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                dungeonMap[x, y] = WALL;
            }
        }
    }
    
    void GenerateRooms()
    {
        int attempts = 0;
        int maxAttempts = numberOfRooms * 10;
        
        while (rooms.Count < numberOfRooms && attempts < maxAttempts)
        {
            attempts++;
            
            // Random room size
            int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            
            // Random position (leave border)
            int roomX = Random.Range(1, dungeonWidth - roomWidth - 1);
            int roomY = Random.Range(1, dungeonHeight - roomHeight - 1);
            
            Room newRoom = new Room(roomX, roomY, roomWidth, roomHeight);
            
            // Check if room overlaps with existing rooms
            bool canPlace = true;
            foreach (Room existingRoom in rooms)
            {
                if (newRoom.Overlaps(existingRoom))
                {
                    canPlace = false;
                    break;
                }
            }
            
            if (canPlace)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }
        
        Debug.Log($"Generated {rooms.Count} rooms out of {numberOfRooms} requested");
    }
    
    void CarveRoom(Room room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                dungeonMap[x, y] = FLOOR;
            }
        }
    }
    
    void GenerateCorridors()
    {
        // Connect each room to the next one
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            ConnectRooms(rooms[i], rooms[i + 1]);
        }
        
        // Optionally connect first and last room for a loop
        if (rooms.Count > 2)
        {
            ConnectRooms(rooms[rooms.Count - 1], rooms[0]);
        }
    }
    
    void ConnectRooms(Room roomA, Room roomB)
    {
        Vector2Int centerA = roomA.GetCenter();
        Vector2Int centerB = roomB.GetCenter();
        
        // Create L-shaped corridor
        // First, move horizontally
        int currentX = centerA.x;
        int targetX = centerB.x;
        int y = centerA.y;
        
        while (currentX != targetX)
        {
            CarveFloor(currentX, y);
            currentX += (targetX > currentX) ? 1 : -1;
        }
        
        // Then move vertically
        int currentY = centerA.y;
        int targetY = centerB.y;
        int x = centerB.x;
        
        while (currentY != targetY)
        {
            CarveFloor(x, currentY);
            currentY += (targetY > currentY) ? 1 : -1;
        }
    }
    
    void CarveFloor(int x, int y)
    {
        if (x >= 0 && x < dungeonWidth && y >= 0 && y < dungeonHeight)
        {
            dungeonMap[x, y] = FLOOR;
        }
    }
    
    void BuildDungeonMesh()
    {
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                Vector3 position = new Vector3(x, 0, y);
                
                if (dungeonMap[x, y] == WALL)
                {
                    if (wallPrefab != null)
                    {
                        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
                        wall.transform.parent = transform;
                    }
                }
                else if (dungeonMap[x, y] == FLOOR)
                {
                    if (floorPrefab != null)
                    {
                        GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity);
                        floor.transform.parent = transform;
                    }
                }
            }
        }
    }
    
    void SpawnPlayer()
    {
        if (playerPrefab != null && rooms.Count > 0)
        {
            // Spawn player in the center of the first room
            Vector2Int roomCenter = rooms[0].GetCenter();
            Vector3 spawnPosition = new Vector3(roomCenter.x, 1, roomCenter.y);
            
            GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Player spawned at {spawnPosition}");
        }
    }
    
    void ClearDungeon()
    {
        // Destroy all child objects (walls and floors)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        // Find and destroy player if it exists
        GameObject existingPlayer = GameObject.FindWithTag("Player");
        if (existingPlayer != null)
        {
            DestroyImmediate(existingPlayer);
        }
    }
    
    // Utility method to check if a position is a floor
    public bool IsFloor(int x, int y)
    {
        if (x < 0 || x >= dungeonWidth || y < 0 || y >= dungeonHeight)
            return false;
        return dungeonMap[x, y] == FLOOR;
    }
    
    // Get a random floor position
    public Vector2Int GetRandomFloorPosition()
    {
        List<Vector2Int> floorPositions = new List<Vector2Int>();
        
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (dungeonMap[x, y] == FLOOR)
                {
                    floorPositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        if (floorPositions.Count > 0)
        {
            return floorPositions[Random.Range(0, floorPositions.Count)];
        }
        
        return Vector2Int.zero;
    }
}