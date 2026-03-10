using System;

/**
 * Data Transfer Objects (DTOs) for serializing and deserializing JSON.
 * We keep the fields public strictly for Unity's JsonUtility requirements.
 */

[Serializable]
public class ServerCommand
{
    public string type;
    public string action;
    public string payload;
}

[Serializable]
public class ClientResponse
{
    public string type;
    public string payload;
}