using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabDungeonGenerator : MonoBehaviour
{
    public int maxRooms;
    public GameObject playerPrefab;
    public GameObject deadEndPrefab;

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
        //var maxWeight = roomChanceWeights.Sum(rw => rw.weight);
        var initialRoom = Instantiate(roomChanceWeights.First().room, Vector3.zero, Quaternion.identity);
        Instantiate(playerPrefab, new Vector3(0, 10, 0), Quaternion.identity);
        BranchRoomOut(initialRoom);
    }

    void BranchRoomOut(GameObject room)
    {
        var connectors = room.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("Door")).ToArray();
        foreach (var item in connectors)
        {
            var newRoom = Instantiate(roomChanceWeights.First().room, Vector3.zero, Quaternion.identity);
            var secondConnectors = newRoom.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("Door")).ToArray();
            var newRoomRandomSecondConnector = secondConnectors[Random.Range(0, secondConnectors.Length)];
            print($"Picked a random connector! {newRoomRandomSecondConnector.transform.position}");

            // Align the new room's connector with the original room's connector
            Quaternion targetRotation = item.rotation * Quaternion.Euler(0, 180f, 0);
            newRoom.transform.rotation = targetRotation * Quaternion.Inverse(newRoomRandomSecondConnector.localRotation);

            Vector3 offset = item.position - newRoomRandomSecondConnector.position;
            newRoom.transform.position += offset;
        }
    }
}
