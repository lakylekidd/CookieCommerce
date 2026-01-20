using System.Runtime.InteropServices.ObjectiveC;

namespace CookieCommerce.DomainBuildingBlocks;

public abstract class AggregateBase
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
    public int Version { get; internal set; } = -1;

    private readonly List<object> _uncommittedEvents = new();
    public IReadOnlyCollection<object> DomainEvents => _uncommittedEvents.AsReadOnly();

    protected void Apply(object domainEvent)
    {
        _uncommittedEvents.Add(domainEvent);
    }

    protected abstract void When(object @event);

    public void ClearUncommitedEvents()
    {
        _uncommittedEvents.Clear();
    }
}
