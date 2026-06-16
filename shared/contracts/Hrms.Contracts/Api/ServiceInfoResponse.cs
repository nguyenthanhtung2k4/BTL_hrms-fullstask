namespace Hrms.Contracts.Api;

public sealed record ServiceInfoResponse(
    string ServiceName,
    string Version,
    string Database,
    string[] OwnedModules,
    string[] PublishedEvents,
    string[] ConsumedEvents);

