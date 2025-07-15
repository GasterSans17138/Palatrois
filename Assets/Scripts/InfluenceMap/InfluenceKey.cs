using System;

public struct InfluenceKey : IEquatable<InfluenceKey>
{
    public ETeam Observer;
    public ETeam Source;
    public InfluenceType Type;

    public InfluenceKey(ETeam observer, ETeam source, InfluenceType type)
    {
        Observer = observer;
        Source = source;
        Type = type;
    }

    public override int GetHashCode() => HashCode.Combine(Observer, Source, Type);

    public bool Equals(InfluenceKey other) =>
        Observer == other.Observer && Source == other.Source && Type == other.Type;

    public override bool Equals(object obj) => obj is InfluenceKey other && Equals(other);
}