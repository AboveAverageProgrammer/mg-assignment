namespace ProductManagerApi.Models;

public class DuplicateIdException(string entityType, object key)
    : Exception($"Entity {entityType} with key {key} is already created.")
{
    public string EntityType { get; } = entityType;
    public object Key { get; } = key;
}