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