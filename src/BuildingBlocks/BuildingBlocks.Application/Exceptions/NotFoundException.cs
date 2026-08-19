namespace BuildingBlocks.Application.Exceptions;

// <summary> A requested resource does not exist. Technical rather than
// business-specific, so it lives in BuildingBlocks and every module maps to
// the same 404. </summary>

public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} '{key}' was not found.")
{
    public string Resource { get; } = resource;

    public object Key { get; } = key;
}
