namespace Api.Comercial.Models.Dtos;

public sealed record LookupCreateDto(string Name);
public sealed record LookupUpdateDto(string? Name, bool? Active);
public sealed record LookupDto(int Id, string Name, bool Active);
public sealed record LookupQueryDto(int? Id, string? Name, bool? Active, int? Page, int? PageSize);

public sealed record CodeNameCreateDto(string Code, string Name);
public sealed record CodeNameUpdateDto(string? Name, bool? Active);
public sealed record CodeNameDto(string Code, string Name, bool Active);
public sealed record CodeNameQueryDto(string? Code, string? Name, bool? Active, int? Page, int? PageSize);

public sealed record StateCreateDto(string StateCode, string CountryCode, string StateName);
public sealed record StateUpdateDto(string? CountryCode, string? StateName, bool? Active);
public sealed record StateDto(string StateCode, string CountryCode, string StateName, bool Active);
public sealed record StateQueryDto(string? StateCode, string? CountryCode, string? StateName, bool? Active, int? Page, int? PageSize);
