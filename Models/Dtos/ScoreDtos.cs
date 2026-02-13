namespace Api.Comercial.Models.Dtos;

public sealed record ScoreCategoryWeightCreateDto(string CategoryName, decimal CategoryWeight);
public sealed record ScoreCategoryWeightUpdateDto(decimal? CategoryWeight, bool? Active);
public sealed record ScoreCategoryWeightDto(string CategoryName, decimal CategoryWeight, bool Active);
public sealed record ScoreCategoryWeightQueryDto(string? CategoryName, bool? Active, int? Page, int? PageSize);

public sealed record ScoreValueWeightCreateDto(string CategoryName, string ValueName, decimal ValueWeight);
public sealed record ScoreValueWeightUpdateDto(decimal? ValueWeight, bool? Active);
public sealed record ScoreValueWeightDto(string CategoryName, string ValueName, decimal ValueWeight, bool Active);
public sealed record ScoreValueWeightQueryDto(string? CategoryName, string? ValueName, bool? Active, int? Page, int? PageSize);
