using System;
using System.Collections.Generic;
using UnityEngine;

public class GuaranteedItemSpawn : MonoBehaviour
{
    public List<GameObject> itemsToSpawn;
    void Start()
    {
        SpawnOneItem();
    }

    private void SpawnOneItem()
    {
        // Get a random item from the list
        if (itemsToSpawn.Count == 0) return;

        GameObject itemToSpawn = itemsToSpawn[UnityEngine.Random.Range(0, itemsToSpawn.Count)];
        Instantiate(itemToSpawn, transform.position, Quaternion.identity);
    }

}
