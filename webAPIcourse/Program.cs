using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var namesFilePath = Path.Combine(AppContext.BaseDirectory, builder.Configuration["CustomSettings:NamesFilePath1"]);

Console.WriteLine($"Trying to access names file at: {namesFilePath}");

List<string>? names = null;

if (!string.IsNullOrEmpty(namesFilePath) && File.Exists(namesFilePath))
{
    var json = File.ReadAllText(namesFilePath);
    var nameData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
    names = nameData?["Names"] ?? new List<string>();
}
else
{
    //names = new List<string> {"fede", "ange","vale","cricco"};
    Console.WriteLine("Names file not found.");
    names = new List<string> { "non funziona" };
}

app.MapGet("/RndName", async (HttpContext context) =>
{
    var ipAddress = builder.Configuration["Ip:IpAddress"];

    using var client = new HttpClient();
    var token = builder.Configuration["Ip:IpInfoToken"];
    var response1 = await client.GetStringAsync($"https://ipinfo.io/{ipAddress}/json?token={token}");

    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(response1);
    var country = data?["country"] ?? "Unknown";
    var timezone = data?["timezone"] ?? "Europe/Rome";

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

    Random rnd = new Random();
    var winner = names[rnd.Next(names.Count)];

    string responseHtml = $@"
<html>
<head>
    <title>Nome Casuale</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 20px;
            padding: 20px;
            background-color: black; /* Imposta lo sfondo su nero */
            color: white; /* Imposta il colore del testo su bianco per contrasto */
        }}
        h1 {{
            animation: rainbow 5s linear infinite;
        }}
        @keyframes rainbow {{
            0% {{ color: red; }}
            14% {{ color: orange; }}
            28% {{ color: yellow; }}
            42% {{ color: green; }}
            57% {{ color: blue; }}
            71% {{ color: indigo; }}
            85% {{ color: violet; }}
            100% {{ color: red; }}
        }}
        .content {{
            display: flex;
            flex-direction: column;
            align-items: center;
        }}
        .iframe-container {{
            margin: 20px 0;
            max-width: 800px; /* Limita la larghezza massima */
            width: 100%;
            display: flex;
            flex-direction: column;
            gap: 20px; /* Spazio tra gli iframe */
        }}
        .youtube-container {{
            position: relative;
            width: 100%;
            padding-top: 56.25%; /* Rapporto 16:9 */
            overflow: hidden;
            border-radius: 12px;
        }}
        .youtube-container iframe {{
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            border: none; /* Rimuovi il bordo */
        }}
        iframe {{
            border-radius: 12px;
        }}
    </style>
</head>
<body>
    <h1>Nome Casuale: {winner}</h1>
    <p>Ora locale: {localTime.ToShortTimeString()} ({localTime.ToShortDateString()})</p>
    <p>Il paese dell'utente è: {country}</p>
    <p>Fuso orario: {timezone}</p>
    <p>Indirizzo IP: {ipAddress}</p>
    <div class='content'>
        <div class='iframe-container'>
            <iframe src='https://open.spotify.com/embed/track/0e4abLRHABOFfZmD9rnjhf?utm_source=generator' width='100%' height='352' allowfullscreen='' allow='autoplay; clipboard-write; encrypted-media; fullscreen; picture-in-picture' loading='lazy'></iframe>
            <div class='youtube-container'>
                <iframe width='560' height='315' src='https://www.youtube.com/embed/2TjbF0FmEMI' title='Playboi Carti - On That Time (Official Audio)' allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share' allowfullscreen></iframe>
            </div>
            <iframe src='https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d8132.8394909058115!2d8.032559136656271!3d43.889127081022394!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x12d26ea82de9f293%3A0xf57b6d8c643d382c!2sAl%C3%AC%20Baba%20Kebab!5e0!3m2!1sit!2sit!4v1730573150850!5m2!1sit!2sit' width='100%' height='450' allowfullscreen='' loading='lazy'></iframe>
        </div>
    </div>
</body>
</html>";

    return Results.Content(responseHtml, "text/html");

    /*
    var result = Results.Content(iframe, "text/html");
    string responseText = $"Il paese dell'utente è: {country}, Fuso orario: {timezone}";
    string response = $"{winner}! Ora locale: {localTime.ToShortTimeString()} ({localTime.ToShortDateString()})\n"
                      + $"Il paese dell'utente è: {country}, Fuso orario: {timezone}\n"
                      + $"Indirizzo IP: {ipAddress}";
    return Results.Text(response, "text/plain");
    */
}).WithName("GetName");

app.Run();