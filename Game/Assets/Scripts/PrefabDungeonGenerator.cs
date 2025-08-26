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
        Instantiate(playerPrefab, new Vector3(0, 3, 0), Quaternion.identity);
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

            newRoomRandomSecondConnector.transform.localScale = new Vector3(2, 2, 2);
            Vector3 targetForward = -item.forward;
            Vector3 currentForward = newRoomRandomSecondConnector.forward;
            Quaternion alignRotation = Quaternion.FromToRotation(currentForward, targetForward);
            newRoom.transform.rotation = alignRotation * room.transform.rotation;
            Vector3 offset = item.position - newRoomRandomSecondConnector.position;
            newRoom.transform.position += offset;
        }
    }
}
