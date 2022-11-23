using Microsoft.EntityFrameworkCore;
using PSXLink.MVVM.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace PSXLink.MVVM.Data
{
    public class GameRepository
    {
        public async Task Create(Game entity)
        {
            using PSXLinkDataContext dbContexxt = new();
            await dbContexxt.Set<Game>().AddAsync(entity);
            await dbContexxt.SaveChangesAsync();
        }

        public async Task Update(Game entity)
        {
            using PSXLinkDataContext dbContexxt = new();
            dbContexxt.Set<Game>().Update(entity);
            await dbContexxt.SaveChangesAsync();
        }

        public async Task Delete(Game entity)
        {
            using PSXLinkDataContext dbContexxt = new();
            dbContexxt.Set<Game>().Remove(entity);
            await dbContexxt.SaveChangesAsync();
        }

        public async Task<List<Game>> ReadAll()
        {
            using PSXLinkDataContext dbContexxt = new();
            List<Game> entities = await dbContexxt.Set<Game>().Where(t=>t.Title!="Empty").ToListAsync();
            return await Task.FromResult(entities);
        }

        private async Task<bool> IsExist(Game entity)
        {

            try
            {
                using PSXLinkDataContext dbContexxt = new();
                Game? find = await dbContexxt.Set<Game>().FirstOrDefaultAsync(f => f.TitleID == entity.TitleID);
                return await Task.FromResult(find is not null);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task Backup()
        {
            FolderRepository.CreateFolder();
            using PSXLinkDataContext dbContexxt = new();
            using StreamWriter sw = new($@"Backup\{DateTime.Now:MM-dd-yyyy HH-mm-ss}.json");
            List<Game> entities = await dbContexxt.Set<Game>().ToListAsync();
            JsonSerializerOptions options = new() { WriteIndented = true };

            string jsonBackup = JsonSerializer.Serialize(entities, options);
            sw.WriteLine(jsonBackup);
            MessageBox.Show("Done", "Result", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async Task Restore(string path)
        {
            using PSXLinkDataContext dbContexxt = new();
            try
            {
                string jsonBackup = await File.ReadAllTextAsync(path);
                List<Game>? entities = JsonSerializer.Deserialize<List<Game>>(jsonBackup);
                foreach (Game entity in entities!)
                {
                    if (!await IsExist(entity))
                    {
                        await Create(entity);
                    }
                }
                MessageBox.Show("Done", "Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Something Wrong With Backup File", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
