using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySaveData
{
    public string Type; 
    public Vector3 Position;
    public float CurrentHp;
    public bool IsPeaceful;
}

[System.Serializable]
public class PlayerData
{
    
    public float Hp;
    public float MaxHp;
    public Vector3 Position;
    public int Kills;


    public List<EnemySaveData> Enemies = new List<EnemySaveData>();
}