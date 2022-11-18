using PSXLink.MVVM.Models;
using System;
using System.Collections.Generic;
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
        private List<UpdateLog> _logs;
        private readonly GameRepository _repository;

        public UpdateRepository()
        {
            _logs = new();
            _repository = new();
        }

        public void AddLog(UpdateLog log)
        {
            _logs.Add(log);
        }

        public List<UpdateLog> ReadLogs()
        {
            return _logs;
        }

        public void ClearLogs()
        {
            _logs = new();
        }

        private async Task<string> GetXmlContent(string? xmllink)
        {
            try
            {
                HttpClient client = new();
                client.DefaultRequestHeaders.Add("user-agent", "Only a test!");
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                string xmlContent = await Task.Run(() => client.GetStringAsync(xmllink));
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
            StringBuilder sbLink = new();
            StringBuilder sbHash = new();
            UpdateLog log = new();
            XmlDocument xmlDoc = new();

            string xmlContent = await Task.Run(() => GetXmlContent(game.XmlLink));
            if (!string.IsNullOrEmpty(xmlContent) && xmlContent.TrimStart().StartsWith("<"))
            {
                xmlDoc.LoadXml(xmlContent);
                string? version = Version(xmlContent);
                string? folderName = await Task.Run(() => FolderRepository.CreateLogFolder(game.Title!, game.Region, game.TitleID!));
                if (checkVersion)
                {
                    if (version != game.Version)
                    {
                        sbLink.AppendLine($"{game?.Title}, {game?.Console}, {game?.TitleID}, {game?.Region}, {version}");
                        sbHash.AppendLine($"{game?.Title}, {game?.Console}, {game?.TitleID}, {game?.Region}, {version}");
                        (string link, string hash) = await GetUpdate(xmlContent);
                        sbLink.AppendLine(link);
                        sbHash.AppendLine(hash);
                        sbLink.Replace($"{Environment.NewLine}{Environment.NewLine}", Environment.NewLine);
                        sbHash.Replace($"{Environment.NewLine}{Environment.NewLine}", Environment.NewLine);
                        using (StreamWriter writetext = new($"{folderName}\\1. {game?.Version}-Link.txt"))
                        {
                            writetext.WriteLine(sbLink.ToString());
                        }
                        using (StreamWriter writetext = new($"{folderName}\\2. {game!.Version}-Hash.txt"))
                        {
                            writetext.WriteLine(sbHash.ToString());
                        }

                        log.Status = $"{game.Title} with TitleID {game.TitleID} Has Update {game.Version} => {version}";
                        log.Link = sbLink.ToString();
                        log.Hash = sbHash.ToString();

                        game.Version = version;
                        await _repository.Update(game);
                    }
                    else
                    {
                        log.Status = $"{game.Title} with TitleID {game.TitleID} Has Latest Version";
                    }
                }
                else
                {
                    sbLink.AppendLine($"{game?.Title}, {game?.Console}, {game?.TitleID}, {game?.Region}, {version}");
                    sbHash.AppendLine($"{game?.Title}, {game?.Console}, {game?.TitleID}, {game?.Region}, {version}");
                    (string link, string hash) = await GetUpdate(xmlContent);
                    sbLink.AppendLine(link);
                    sbHash.AppendLine(hash);
                    sbLink.Replace($"{Environment.NewLine}{Environment.NewLine}", Environment.NewLine);
                    sbHash.Replace($"{Environment.NewLine}{Environment.NewLine}", Environment.NewLine);
                    using (StreamWriter writetext = new($"{folderName}\\1. {game!.Version}-Link.txt"))
                    {
                        writetext.WriteLine(sbLink.ToString());
                    }
                    using (StreamWriter writetext = new($"{folderName}\\2. {game.Version}-Hash.txt"))
                    {
                        writetext.WriteLine(sbHash.ToString());
                    }

                    log.Status = $"{game.Title} with TitleID {game.TitleID} Get Update {version}";
                    log.Link = sbLink.ToString();
                    log.Hash = sbHash.ToString();

                    game.Version = version;
                    await _repository.Update(game);
                }
            }
            return await Task.Delay(100).ContinueWith(t => log);
        }

        public async Task<(string link, string hash)> GetUpdate(string xmlContent)
        {
            XmlDocument xmlDoc = new();
            StringBuilder sbLink = new();
            StringBuilder sbHash = new();
            xmlDoc.LoadXml(xmlContent);
            XmlNodeList? deltanodelist = xmlDoc.GetElementsByTagName("delta_info_set");
            string? deltaPKG = deltanodelist[0]?.Attributes!["url"]?.Value;
            sbLink.AppendLine(deltaPKG);
            XmlNodeList? pkgnodelist = xmlDoc.GetElementsByTagName("package");
            using HttpClient client = new();
            using HttpResponseMessage response = await client.GetAsync(pkgnodelist[0]!.Attributes!["manifest_url"]!.Value);
            using HttpContent content = response.Content;
            string json = await content.ReadAsStringAsync();
            UpdateJson? update = JsonSerializer.Deserialize<UpdateJson>(json);
            foreach (Piece link in update!.Pieces!)
            {
                sbLink.AppendLine(link.Url);
                Uri uri = new(link.Url!);
                sbHash.AppendLine($"{Path.GetFileName(uri.LocalPath)} : {link.HashValue}");
            }
            return await Task.FromResult((sbLink.ToString(), sbHash.ToString()));
        }

        public async Task<UpdateLog> NewGame(Game game)
        {
            using PSXLinkDataContext dbContext = new();
            UpdateLog log = new();
            string xmlContent = await Task.Run(() => GetXmlContent(game?.XmlLink).Result);
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
                log.Status = $"TitleID ({game?.TitleID}) is New Game with title ({game?.Title})";
            }
            else
            {
                log.Status = $"TitleID ({game?.TitleID}) is not Game";
            }
            return await Task.FromResult(log);
        }
    }
}
