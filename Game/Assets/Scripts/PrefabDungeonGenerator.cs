using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PrefabDungeonGenerator : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.005f);
    public int maxRooms;
    private int roomCounter;
    public float overlapPercentage = 0.9f;
    public GameObject playerPrefab;
    GameObject currentPlayerObject;
    public GameObject deadEndPrefab;
    public GameObject startRoomPrefab;
    public List<GameObject> rooms = new List<GameObject>();

    [SerializeField]
    public List<RoomWeight> roomChanceWeights = new List<RoomWeight>();
    public List<RoomWeight> endRoomChanceWeights = new List<RoomWeight>();

    [System.Serializable]
    public class RoomWeight
    {
        public GameObject room;
        public float weight;
    }

    void Start()
    {
        roomCounter = maxRooms;
        StartCoroutine(GenerateDungeonCoroutine());
        GameManager.Instance.OnGameRestart += OnGameRestart;
    }

    private void OnGameRestart()
    {
        StopAllCoroutines();
        Destroy(currentPlayerObject);
        foreach (var room in rooms)
        {
            Destroy(room);
        }
        rooms.Clear();
        roomCounter = maxRooms;
        StartCoroutine(GenerateDungeonCoroutine());
    }

    public IEnumerator GenerateDungeonCoroutine()
    {
        if (startRoomPrefab == null)
        {
            Debug.LogError("Start room prefab is null or destroyed!");
            yield break;
        }
        var initialRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        initialRoom.transform.parent = this.transform;
        rooms.Add(initialRoom);
        var player = Instantiate(playerPrefab, new Vector3(0, 3, 0), Quaternion.identity);
        currentPlayerObject = player;
        yield return StartCoroutine(BranchRoomOutCoroutine(initialRoom));
        FillEndRooms();
        LockFinalEdges();
    }

    private void LockFinalEdges()
    {
        foreach (var room in rooms)
        {
            var unusedConnectors = room.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("Door") && x.tag == "Untagged").ToArray();
            foreach (var connector in unusedConnectors)
            {
                if (deadEndPrefab != null)
                {
                    var deadEnd = Instantiate(deadEndPrefab, connector.position, connector.rotation);
                    deadEnd.transform.parent = this.transform;
                }
            }
        }
    }

    private void FillEndRooms()
    {
        foreach (var room in rooms)
        {
            var unusedConnectors = room.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("Door") && x.tag == "Untagged").ToArray();
            foreach (var connector in unusedConnectors)
            {
                var endRoomPrefab = GetRandomEndRoom();
                if (endRoomPrefab != null)
                {
                    var newRoom = PlaceRoomAtConnector(connector, endRoomPrefab);
                    if (newRoom != null)
                    {
                        rooms.Add(newRoom);
                    }
                }
            }
        }
    }

    GameObject GetRandomRoom()
    {
        var validWeights = roomChanceWeights.Where(rw => rw.room != null).ToList();
        if (validWeights.Count == 0) return null;
        var totalWeight = validWeights.Sum(rw => rw.weight);
        var randomValue = Random.Range(0f, totalWeight);
        
        float currentWeight = 0f;
        foreach (var roomWeight in validWeights)
        {
            currentWeight += roomWeight.weight;
            if (randomValue <= currentWeight)
            {
                return roomWeight.room;
            }
        }
        return validWeights.First().room;
    }

    GameObject GetRandomEndRoom()
    {
        var validWeights = endRoomChanceWeights.Where(rw => rw.room != null).ToList();
        if (validWeights.Count == 0) return null;
        var totalWeight = validWeights.Sum(rw => rw.weight);
        var randomValue = Random.Range(0f, totalWeight);
        
        float currentWeight = 0f;
        foreach (var roomWeight in validWeights)
        {
            currentWeight += roomWeight.weight;
            if (randomValue <= currentWeight)
            {
                return roomWeight.room;
            }
        }
        return validWeights.First().room;
    }

    IEnumerator BranchRoomOutCoroutine(GameObject room)
    {
        roomCounter--;
        if (roomCounter <= 0)
        {
            yield break;
        }
        
        var connectors = room.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("Door") && x.tag == "Untagged").ToArray();
        foreach (var item in connectors)
        {
            var roomPrefab = GetRandomRoom();
            if (roomPrefab == null) continue;
            var newRoom = PlaceRoomAtConnector(item, roomPrefab);
            if (newRoom == null) continue;
            rooms.Add(newRoom);
            newRoom.transform.parent = this.transform;
            
            item.tag = "Processed";
            // newRoomRandomConnector.tag = "Processed";
            
            yield return _waitForSeconds0_1;
            
            yield return StartCoroutine(BranchRoomOutCoroutine(newRoom));
        }
    }

    Bounds GetRoomBounds(GameObject room)
    {
        Renderer[] renderers = room.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(room.transform.position, Vector3.zero);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        
        // Shrink bounds by 20% to allow for door connections
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents * overlapPercentage;
        bounds = new Bounds(center, extents * 2);
        
        return bounds;
    }

    bool HasRoomCollision(GameObject newRoom)
    {
        Bounds newBounds = GetRoomBounds(newRoom);
        
        foreach (var existingRoom in rooms)
        {
            Bounds existingBounds = GetRoomBounds(existingRoom);
            
            if (newBounds.Intersects(existingBounds))
            {
                return true;
            }
        }
        
        return false;
    }

    private GameObject PlaceRoomAtConnector(Transform connector, GameObject roomPrefab)
    {
        var newRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
        var newRoomConnectors = newRoom.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("Door") && x.tag == "Untagged").ToArray();
        if (newRoomConnectors.Length > 0)
        {
            var newRoomRandomConnector = newRoomConnectors[Random.Range(0, newRoomConnectors.Length)];
            
            Quaternion targetRotation = connector.rotation * Quaternion.Euler(0, 180f, 0);
            newRoom.transform.rotation = targetRotation * Quaternion.Inverse(newRoomRandomConnector.localRotation);
            Vector3 offset = connector.position - newRoomRandomConnector.position;
            newRoom.transform.position += offset;
            
            // Add tiny random offset to prevent Z-fighting
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.001f, 0.001f), 
                Random.Range(-0.001f, 0.001f), 
                Random.Range(-0.001f, 0.001f)
            );
            newRoom.transform.position += randomOffset;
            
            if (HasRoomCollision(newRoom))
            {
                Destroy(newRoom);
                return null;
            }
            
            newRoom.transform.parent = this.transform;
            connector.tag = "Processed";
            newRoomRandomConnector.tag = "Processed";
        }
        else
        {
            // No connectors, place directly
            newRoom.transform.position = connector.position;
            newRoom.transform.rotation = connector.rotation;
            newRoom.transform.parent = this.transform;
            connector.tag = "Processed";
            if (HasRoomCollision(newRoom))
            {
                Destroy(newRoom);
                return null;
            }
        }
        
        return newRoom;
    }
}
