using Microsoft.EntityFrameworkCore;
using PSXLink.MVVM.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace PSXLink.MVVM.Data
{
    public class UpdateRepository
    {
        private readonly GameRepository _repository;

        public UpdateRepository()
        {
            _repository = new();
        }

        private async Task<string> GetWebContent(string? link)
        {
            try
            {
                HttpClient client = new();
                client.DefaultRequestHeaders.Add("user-agent", "Only a test!");
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                string xmlContent = await Task.Run(() => client.GetStringAsync(link));
                return await Task.Delay(50).ContinueWith(t => xmlContent);
            }
            catch
            {
                return await Task.Delay(50).ContinueWith(t => string.Empty);
            }
        }

        private string? Version(string xmlContent)
        {
            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(xmlContent);
            XmlNodeList pkgNodeList = xmlDocument.GetElementsByTagName("package");
            return pkgNodeList[0]?.Attributes?["version"]?.Value;
        }

        private int Region(string xmlContent)
        {
            XmlDocument? xmlDocument = new();
            xmlDocument.LoadXml(xmlContent);
            XmlNodeList? pkgNodeList = xmlDocument.GetElementsByTagName("package");
            string? titleID = pkgNodeList[0]?.Attributes?["content_id"]?.Value.Split('-')[0];
            int region = titleID?[0] switch
            {
                'I' => 0,
                'U' => 1,
                'E' => 2,
                _ => 3
            };
            return region;
        }

        public async Task<UpdateLog> CheckUpdate(Game game, bool checkVersion)
        {
            UpdateLog log = new();
            XmlDocument xmlDoc = new();

            string xmlContent = await Task.Run(() => GetWebContent(game.XmlLink));
            if (!string.IsNullOrEmpty(xmlContent) && xmlContent.TrimStart().StartsWith("<"))
            {
                xmlDoc.LoadXml(xmlContent);
                string? version = Version(xmlContent);
                string? folderName = await Task.Run(() => FolderRepository.CreateLogFolder(game.Title!, game.Region, game.TitleID!));
                if (checkVersion)
                {
                    if (version != game.Version)
                    {
                        log = await GetUpdate(xmlContent, game, version, checkVersion, folderName);
                        if (log.Link?.Length > 1)
                        {
                            game!.Version = version;
                            await _repository.Update(game);
                        }
                    }
                    else
                    {
                        log.Status = $"Latest Update: {game.Title} With TitleID {game.TitleID}";
                    }
                }

                else
                {
                    log = await GetUpdate(xmlContent, game, version, checkVersion, folderName);
                    if (log.Link?.Length > 1)
                    {
                        game!.Version = version;
                        await _repository.Update(game);
                    }
                }
            }
            return await Task.Delay(100).ContinueWith(t => log);
        }

        public async Task<UpdateLog> GetUpdate(string xmlContent, Game game, string? version, bool checkVersion, string folderName)
        {
            XmlDocument xmlDoc = new();
            StringBuilder sbLink = new();
            StringBuilder sbHash = new();
            UpdateLog log = new();
            sbLink.AppendLine($"{game?.Title}, {game?.Console}, {game?.TitleID}, {game?.Region}, {version}");
            sbHash.AppendLine($"{game?.Title}, {game?.Console}, {game?.TitleID}, {game?.Region}, {version}");
            try
            {
                xmlDoc.LoadXml(xmlContent);
                XmlNodeList? deltanodelist = xmlDoc.GetElementsByTagName("delta_info_set");
                string? deltaPKG = deltanodelist[0]?.Attributes!["url"]?.Value;
                sbLink.AppendLine(deltaPKG);
                XmlNodeList? pkgnodelist = xmlDoc.GetElementsByTagName("package");
                string json = await GetWebContent(pkgnodelist[0]!.Attributes!["manifest_url"]!.Value);
                UpdateJson? update = JsonSerializer.Deserialize<UpdateJson>(json);
                foreach (Piece link in update!.Pieces!)
                {
                    sbLink.AppendLine(link.Url);
                    Uri uri = new(link.Url!);
                    sbHash.AppendLine($"{Path.GetFileName(uri.LocalPath)} : {link.HashValue}");
                }
                sbLink.Replace($"{Environment.NewLine}{Environment.NewLine}", Environment.NewLine);
                sbHash.Replace($"{Environment.NewLine}{Environment.NewLine}", Environment.NewLine);
                log.Link = sbLink.ToString();
                log.Hash = sbHash.ToString();
                if (checkVersion)
                {
                    log.Status = $"New Update: {game?.Title} With TitleID {game?.TitleID} {game?.Version} => {version}";
                }
                else
                {
                    log.Status = $"Get Update: {game?.Title} With TitleID {game?.TitleID}  {version}";
                }
                using (StreamWriter writetext = new($"{folderName}\\1. {version}-Link.txt"))
                {
                    writetext.WriteLine(sbLink.ToString());
                }
                using (StreamWriter writetext = new($"{folderName}\\2. {version}-Hash.txt"))
                {
                    writetext.WriteLine(sbHash.ToString());
                }
            }
            catch
            {
                log.Status = $"Error: {game?.Title} With TitleID {game?.TitleID}";
            }
            return await Task.FromResult(log);
        }

        public async Task<UpdateLog> NewGame(Game game)
        {
            using PSXLinkDataContext dbContext = new();
            UpdateLog log = new();
            string xmlContent = await Task.Run(() => GetWebContent(game?.XmlLink).Result);
            if (!string.IsNullOrEmpty(xmlContent) && xmlContent.TrimStart().StartsWith("<"))
            {
                XmlDocument xmlDocument = new();
                xmlDocument.LoadXml(xmlContent);

                XmlNodeList titleNode = xmlDocument.GetElementsByTagName("title");

                string? title = titleNode?[0]?.InnerText;
                string? version = Version(xmlContent);
                int region = Region(xmlContent);

                game!.Title = title;
                game!.Version = version;
                game!.Region = region;

                dbContext.Set<Game>().Update(game);
                await dbContext.SaveChangesAsync();
                log.Status = $"TitleID {game?.TitleID} is New Game With Title {game?.Title}";
            }
            else
            {
                log.Status = $"TitleID ({game?.TitleID}) is not Game";
            }
            return await Task.FromResult(log);
        }

        public async Task<UpdateLog> FirmewarUpdate()
        {
            using PSXLinkDataContext dbContext = new();
            UpdateLog log = new();
            Firmware fw = await dbContext.Set<Firmware>().FirstAsync(i => i.ID == 1);

            string xml = await GetWebContent(fw.XmlLink);
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xml);
            XmlNodeList version = xmlDoc.GetElementsByTagName("system_pup");
            fw.Version = version[0]!.Attributes!["label"]!.Value;
            await dbContext.SaveChangesAsync();
            XmlNodeList linkNode = xmlDoc.GetElementsByTagName("image");
            string link = $"Firmware Link {Environment.NewLine}{linkNode[0]!.InnerText.Replace("?dest=us", "")}{Environment.NewLine}{linkNode[1]!.InnerText.Replace("?dest=us", "")}";
            log.Status = $"Firmware Link: Version => {version[0]!.Attributes!["label"]!.Value}";
            log.Link = link;
            return await Task.FromResult(log);
        }
    }
}
