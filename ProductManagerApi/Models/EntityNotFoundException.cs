namespace ProductManagerApi.Models;

public class EntityNotFoundException(string entityType, object key)
    : Exception($"Entity {entityType} with key {key} was not found.")
{
    public string EntityType { get; } = entityType;
    public object Key { get; } = key;
}