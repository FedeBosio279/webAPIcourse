using System.Text.Json;

public class NameService
{
    private readonly IConfiguration _configuration;
    private List<string> _names = new();
    private readonly string _namesFilePath;

    public NameService(IConfiguration configuration)
    {
        _configuration = configuration;
        _namesFilePath = Path.Combine(AppContext.BaseDirectory, _configuration["CustomSettings:NamesFilePath1"]);
        LoadNames();
    }

    private void LoadNames()
    {
        if (File.Exists(_namesFilePath))
        {
            var json = File.ReadAllText(_namesFilePath);
            var nameData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
            _names = nameData?["Names"] ?? new List<string>();
        }
    }

    public async Task<string> GetRandomNameAsync(int count, string ipAddress)
    {
        using var client = new HttpClient();
        var token = _configuration["Ip:IpInfoToken"];
        var ipInfoUrl = $"https://ipinfo.io/{ipAddress}/json?token={token}";

        string ipResponse = "";
        try
        {
            ipResponse = await client.GetStringAsync(ipInfoUrl);
        }
        catch (Exception ex)
        {
            return $"<html><body><h1>Error: Unable to retrieve IP info. {ex.Message}</h1></body></html>";
        }

        Dictionary<string, string> ipData;
        try
        {
            ipData = JsonSerializer.Deserialize<Dictionary<string, string>>(ipResponse);
        }
        catch
        {
            return "<html><body><h1>Error: Unable to parse IP info response.</h1></body></html>";
        }

        var country = ipData?["country"] ?? "Unknown";
        var timezone = ipData?["timezone"] ?? "Europe/Rome";

        var utcNow = DateTime.UtcNow;
        TimeZoneInfo timeZoneInfo;
        try
        {
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("UTC");
        }

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);

        var rnd = new Random();
        var selectedNames = Enumerable.Range(0, count)
            .Select(_ => _names[rnd.Next(_names.Count)])
            .ToList();

        return $@"
<html>
<head>
    <title>Nomi Casuali</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: black;
            color: white;
        }}
        h1 {{ animation: rainbow 5s linear infinite; }}
    </style>
</head>
<body>
    <h1>Nomi Casuali</h1>
    <p>Ora locale: {localTime.ToShortTimeString()} ({localTime.ToShortDateString()})</p>
    <p>Paese: {country}</p>
    <p>Fuso orario: {timezone}</p>
    <div>{string.Join("<br>", selectedNames)}</div>
</body>
</html>";
    }
}
    

    

