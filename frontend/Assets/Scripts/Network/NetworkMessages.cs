using System;

/**
 * Data Transfer Objects (DTOs) for serializing and deserializing JSON.
 * We keep the fields public strictly for Unity's JsonUtility requirements.
 */

/// <summary>
/// Serialization: The process of converting an object from memory into a format 
/// that can be saved to a file or transmitted over a network 
/// (usually a text format like JSON, XML, or binary data).
/// </summary>

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