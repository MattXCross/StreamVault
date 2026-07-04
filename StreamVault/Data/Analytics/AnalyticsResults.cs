namespace StreamVault.Data.Analytics;

public record DailyPlayCount(DateOnly Date, int Count);

public record CountryPlayCount(string Country, int Count);
