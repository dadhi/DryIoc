namespace BlazorServerSide.Data;

using System.Diagnostics.CodeAnalysis;

public interface IRandomProvider 
{
    public Random Random { get; }
}

/// <summary>
/// Just and example service for the use of `SetsRequiredMembers` and `required properties` together with DryIoc.
/// </summary>
public class SharedRandomProvider : IRandomProvider
{
    public required Random Random { get; init; }

    [SetsRequiredMembers]
    public SharedRandomProvider()
    {
        Random = Random.Shared;
    }
}
