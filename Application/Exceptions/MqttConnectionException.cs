using System;

public class MqttConnectionException : Exception
{
    public MqttConnectionException(string? message) : base(message)
    {
    }
}
