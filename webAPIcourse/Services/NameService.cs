using System.Text.Json;

public class NameService
{
    private readonly IConfiguration _configuration;
    private List<string> _names = new();
    private readonly string _namesFilePath;

    public NameService(IConfiguration configuration)
    {
        _configuration = configuration;
        _namesFilePath = Path.Combine(AppContext.BaseDirectory, _configuration["CustomSettings:Names"]);
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

    public async Task<object> GetRandomNameDataAsync(int count, string ipAddress)
    {
        using var client = new HttpClient();
        var token = _configuration["Ip:IpInfoToken"];
        var ipInfoUrl = $"https://ipinfo.io/{ipAddress}/json?token={token}";
        
        HttpResponseMessage response = await client.GetAsync(ipInfoUrl);
        if (!response.IsSuccessStatusCode)
        {
            return new { error = $"Unable to retrieve IP info. Status Code: {response.StatusCode}" };
        }
    
        string ipResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine("IP Response: " + ipResponse);
        
        Dictionary<string, string> ipData;
        try
        {
            ipData = JsonSerializer.Deserialize<Dictionary<string, string>>(ipResponse);
            if (ipData == null || !ipData.ContainsKey("country") || !ipData.ContainsKey("timezone"))
            {
                return new { error = "Missing required fields in IP info response." };
            }
        }
        catch (JsonException ex)
        {
            return new { error = $"Unable to parse IP info response. {ex.Message}" };
        }
    
        var country = ipData?.GetValueOrDefault("country", "Unknown");
        var timezone = ipData?.GetValueOrDefault("timezone", "Europe/Rome");
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
    
        // Ritorna i dati come oggetto JSON
        return new {
            country,
            timezone,
            localTime = localTime.ToString("HH:mm:ss"),
            localDate = localTime.ToShortDateString(),
            names = selectedNames
        };
    }
    }

    

    

