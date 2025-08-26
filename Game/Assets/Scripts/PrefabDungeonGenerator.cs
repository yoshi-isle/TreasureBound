using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabDungeonGenerator : MonoBehaviour
{
    public int maxRooms;
    public GameObject playerPrefab;
    public GameObject deadEndPrefab;

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
        var initialRoom = Instantiate(GetRandomRoom(), Vector3.zero, Quaternion.identity);
        rooms.Add(initialRoom);
        Instantiate(playerPrefab, new Vector3(0, 10, 0), Quaternion.identity);
        
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
            
            print($"Placing new room at position: {newRoom.transform.position}");
            print($"Connector positions - existing: {item.position}, new: {newRoomRandomConnector.position}, offset: {offset}");

            if (HasRoomCollision(newRoom))
            {
                print("Collision detected! Skipping this connector...");
                Destroy(newRoom);
                continue;
            }
            
            rooms.Add(newRoom);
            
            item.tag = "Processed";
            newRoomRandomConnector.tag = "Processed";
            
            yield return new WaitForSeconds(0.1f);
            
            yield return StartCoroutine(BranchRoomOutCoroutine(newRoom));
        }
    }

    bool HasRoomCollision(GameObject newRoom)
    {
        var newRoomPosition = newRoom.transform.position;
        var minDistance = 15f;
        
        foreach (var existingRoom in rooms)
        {
            var distance = Vector3.Distance(newRoomPosition, existingRoom.transform.position);
            print($"Distance between rooms: {distance}");
            
            if (distance < minDistance)
            {
                print($"Rooms too close! Distance: {distance}, minimum required: {minDistance}");
                return true;
            }
        }
        
        return false;
    }
}
