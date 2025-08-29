using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PrefabDungeonGenerator : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.02f);
    public int maxRooms;
    public float overlapPercentage = 0.9f;
    public GameObject playerPrefab;
    public GameObject deadEndPrefab;
    public GameObject startRoomPrefab;
    public List<GameObject> rooms = new List<GameObject>();

    [SerializeField]
    public List<RoomWeight> roomChanceWeights = new List<RoomWeight>();

    [System.Serializable]
    public class RoomWeight
    {
        public GameObject room;
        public float weight;
    }

    void Start()
    {
        StartCoroutine(GenerateDungeonCoroutine());
    }

    public IEnumerator GenerateDungeonCoroutine()
    {
        var initialRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        initialRoom.transform.parent = this.transform;
        rooms.Add(initialRoom);
        Instantiate(playerPrefab, new Vector3(0, 2, 0), Quaternion.identity);
        
        yield return StartCoroutine(BranchRoomOutCoroutine(initialRoom));
    }

    GameObject GetRandomRoom()
    {
        var totalWeight = roomChanceWeights.Sum(rw => rw.weight);
        var randomValue = Random.Range(0f, totalWeight);
        
        float currentWeight = 0f;
        foreach (var roomWeight in roomChanceWeights)
        {
            currentWeight += roomWeight.weight;
            if (randomValue <= currentWeight)
            {
                return roomWeight.room;
            }
        }
        return roomChanceWeights.First().room;
    }

    IEnumerator BranchRoomOutCoroutine(GameObject room)
    {
        maxRooms--;
        if (maxRooms <= 0)
        {
            yield break;
        }
        
        var connectors = room.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("Door") && x.tag == "Untagged").ToArray();
        foreach (var item in connectors)
        {
            var newRoom = Instantiate(GetRandomRoom(), Vector3.zero, Quaternion.identity);
            var newRoomConnectors = newRoom.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("Door") && x.tag == "Untagged").ToArray();
            var newRoomRandomConnector = newRoomConnectors[Random.Range(0, newRoomConnectors.Length)];
            
            print($"Picked a random connector! {newRoomRandomConnector.transform.position}");

            Quaternion targetRotation = item.rotation * Quaternion.Euler(0, 180f, 0);
            newRoom.transform.rotation = targetRotation * Quaternion.Inverse(newRoomRandomConnector.localRotation);

            Vector3 offset = item.position - newRoomRandomConnector.position;
            newRoom.transform.position += offset;
            var entitiesContainer = newRoom.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == "Entities");
            if (entitiesContainer != null && entitiesContainer.gameObject != null)
            {
                entitiesContainer.gameObject.SetActive(true);
            }
        
            print($"Placing new room at position: {newRoom.transform.position}");
            print($"Connector positions - existing: {item.position}, new: {newRoomRandomConnector.position}, offset: {offset}");

            if (HasRoomCollision(newRoom))
            {
                print("Collision detected! Skipping this connector...");
                Destroy(newRoom);
                continue;
            }
            
            rooms.Add(newRoom);
            newRoom.transform.parent = this.transform;
            
            item.tag = "Processed";
            newRoomRandomConnector.tag = "Processed";
            
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
                print("Room bounds intersect! Collision detected.");
                return true;
            }
        }
        
        return false;
    }
}
