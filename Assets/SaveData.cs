using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string sceneName;

    public float playerX, playerY, playerZ;

    public List<string> defeatedEnemies = new List<string>();
    public List<string> collectedItems = new List<string>();
    public List<string> inventoryItemIds = new(); 
    public List<string> completedWorldEvents = new();
    public List<string> openedDoors = new();
}