using System.Collections.Generic;

using UnityEngine;
using System;

[Serializable]
public class PlayerSaveData
{
    public string FileName;
    public int CurrentXP = 40;
    public List<ItemEntry> CollectedItems = new List<ItemEntry>();

    [Serializable]
    public struct ItemEntry
    {
        public int itemId;
        public int count;
    }

    public PlayerSaveData(string fileName)
    {
        FileName = fileName;
        CollectedItems = new List<ItemEntry>();
        CurrentXP = 40;
    }
}