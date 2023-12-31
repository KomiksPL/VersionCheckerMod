﻿using System.Net;
using System.Net.Http.Json;
using MelonLoader;
using Newtonsoft.Json;
using Semver;
using VersionCheckerMod;

[assembly: MelonInfo(typeof(EntryPoint), "VersionChecker", "1.0.0", "KomiksPL", "https://www.nexusmods.com/slimerancher2/mods/51")]

namespace VersionCheckerMod;

public class EntryPoint : MelonMod
{
    public override void OnInitializeMelon()
    {
 

        WebClient webClient = new WebClient();
        var versionChecker = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(webClient.DownloadString("https://raw.githubusercontent.com/KomiksPL/VersionCheckerMod/main/modConnections.txt"));
        foreach (var registeredMelon in MelonMod.RegisteredMelons)
        {
            var infoDownloadLink = registeredMelon.Info.DownloadLink;
            if (Uri.TryCreate(infoDownloadLink, UriKind.Absolute, out Uri result) && result.Host.Equals("www.nexusmods.com"))
            {
                string modId = Convert.ToInt32(new string(result.Segments.Last().Where(char.IsDigit).ToArray())).ToString();
                Check(registeredMelon.Info, "slimerancher2", modId, webClient);
            }            
            else if (versionChecker.TryGetValue(registeredMelon.Info.Name, out string modId))
            {
                Check(registeredMelon.Info, "slimerancher2", modId, webClient);
            }
        }
        webClient.Dispose();
    }

    public void Check(MelonInfoAttribute info, string gameName, string modId, WebClient client)
    {
        // Build the URL with custom values
        string url = $"https://srapi.fly.dev/nexusapi?gameName={gameName}&modId={modId}";
        try
        {
            string response = client.DownloadString(url);

            if (!string.IsNullOrEmpty(response))
            {
                TableContext? content = Newtonsoft.Json.JsonConvert.DeserializeObject<TableContext>(response);
                SemVersion semVersion = SemVersion.Parse(content.version);
                if (semVersion >  info.SemanticVersion )
                {
                    LoggerInstance.Warning($"[{info.Name}] A new version has been released: {content.version}. Currently running {info.Version}. Please update the mod.");
                }
                else
                {
                    LoggerInstance.Warning($"[{info.Name}] Mod is updated.");
                }
            }
            //MelonLogger.Msg("Request failed with status code: " + response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            MelonLogger.Msg("Request failed with exception: " + ex.Message);
        }
    }
}
public class TableContext
{
    public int id { get; set; }
    public long uid { get; set; }
    public int modid { get; set; }
    public string gamename { get; set; }
    public long updatedtimestamp { get; set; }
    public DateTime updatedtime { get; set; }
    
    public string version { get; set; }
}