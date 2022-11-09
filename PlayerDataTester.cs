using UnityEngine;
using System;
using System.IO;

public class PlayerDataTester : MonoBehaviour
{
    public PlayerData thePlayerData;
    private PlayerDataFile safePlayerData;
    public static string playerDataTesterFileName = "player_data.json";

    private void Start()
    {
        safePlayerData = new PlayerDataFile(thePlayerData);
        LoadPlayerData();
    }

    private void OnDisable()
    {
        ApplyPlayerData(safePlayerData);
    }

    public void LoadPlayerData()
    {
        var path = Path.Combine(Application.persistentDataPath, playerDataTesterFileName);
        if(File.Exists(path))
        {
            var contents = File.ReadAllTextAsync(path).Result;
            var data = JsonUtility.FromJson<PlayerDataFile>(contents);

            ApplyPlayerData(data);
        }
        else
        {
            SavePlayerData();
        }
    }

    public void SavePlayerData()
    {
        var path = Path.Combine(Application.persistentDataPath, playerDataTesterFileName);
        var data = new PlayerDataFile(thePlayerData);
        var contents = JsonUtility.ToJson(data, true);

        File.WriteAllTextAsync(path, contents);
    }

    public void ApplyPlayerData(PlayerDataFile theFile)
    {
        thePlayerData.baseSpeed = theFile.baseSpeed;
        thePlayerData.accelerationTime = theFile.accelerationTime;
        thePlayerData.deaccelerationTime = theFile.deaccelerationTime;
        thePlayerData.airAccelerationModifier = theFile.airAccelerationModifier;

        thePlayerData.maxJumpHeight = theFile.maxJumpHeight;
        thePlayerData.timeToJumpApex = theFile.timeToJumpApex;
        thePlayerData.fallGravityMultiplier = theFile.fallGravityMultiplier;
        thePlayerData.coyoteTime = theFile.coyoteTime;
    }
}

[Serializable]
public struct PlayerDataFile
{
    public float baseSpeed;
    public float accelerationTime;
    public float deaccelerationTime;
    public float airAccelerationModifier;

    public float maxJumpHeight;
    public float timeToJumpApex;
    public float fallGravityMultiplier;
    public float coyoteTime;

    public PlayerDataFile(PlayerData data)
    {
        baseSpeed = data.baseSpeed;
        accelerationTime = data.accelerationTime;
        deaccelerationTime = data.deaccelerationTime;
        airAccelerationModifier = data.airAccelerationModifier;

        maxJumpHeight = data.maxJumpHeight;
        timeToJumpApex = data.timeToJumpApex;
        fallGravityMultiplier = data.fallGravityMultiplier;
        coyoteTime = data.coyoteTime;
    }
}