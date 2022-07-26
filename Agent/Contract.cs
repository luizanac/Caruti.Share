namespace Agent;

public class Contract
{
    public string Event { get; set; }
    public Position Data { get; set; }
}

public class Position
{
    public uint X { get; set; }
    public uint Y { get; set; }
}