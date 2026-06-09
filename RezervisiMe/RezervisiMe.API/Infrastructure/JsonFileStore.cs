using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Hosting;
using Newtonsoft.Json;
using RezervisiMe.RezervisiMe.API.Models;

namespace RezervisiMe.RezervisiMe.API.Infrastructure
{
    public class JsonFileStore<T> where T : EntityBase
    {
        private readonly string _filePath;
        private readonly object _sync = new object();

        public JsonFileStore(string fileName)
        {

            var dir = HostingEnvironment.MapPath("~/App_Data");
            if (dir == null)
                throw new InvalidOperationException(
                    "HostingEnvironment.MapPath vratio null - aplikacija nije hostovana?");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            _filePath = Path.Combine(dir, fileName);
            if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "[]");

            
        }

        public List<T> LoadAll()
        {
            lock (_sync) { return ReadFromDisk(); }
        }

        public List<T> GetAll()
        {
            lock (_sync) { return ReadFromDisk().Where(x => !x.IsDeleted).ToList(); }
        }

        public T GetById(Guid id)
        {
            lock (_sync)
            {
                return ReadFromDisk()
                    .FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            }
        }

        public T Add(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            lock (_sync)
            {
                var items = ReadFromDisk();
                if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
                if (entity.CreatedAt == default) entity.CreatedAt = DateTime.UtcNow;
                items.Add(entity);
                WriteToDisk(items);
                return entity;
            }
        }

        public bool Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            lock (_sync)
            {
                var items = ReadFromDisk();
                var idx = items.FindIndex(x => x.Id == entity.Id);
                if (idx < 0) return false;
                items[idx] = entity;
                WriteToDisk(items);
                return true;
            }
        }

        public bool SoftDelete(Guid id)
        {
            lock(_sync)
            {
                var items = ReadFromDisk();
                var idx = items.FindIndex(x => x.Id == id);
                if (idx < 0) return false;
                if (items[idx].IsDeleted) return false;
                items[idx].IsDeleted = true;
                WriteToDisk(items);
                return true;
            }
        }

        public bool HardDelete(Guid id)
        {
            lock (_sync)
            {
                var items = ReadFromDisk();
                var removed = items.RemoveAll(x => x.Id == id);
                if (removed == 0) return false;
                WriteToDisk(items);
                return true;
            }
        }

        public void SaveAll(List<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            lock (_sync) { WriteToDisk(items); }
        }


        public T GetByIdIncludingDeleted(Guid id)
        {
            lock (_sync)
            {
                return ReadFromDisk().FirstOrDefault(x => x.Id == id);
            }
        }


        private List<T> ReadFromDisk()
        {
            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrEmpty(json)) return new List<T>();
            return JsonConvert.DeserializeObject<List<T>>(json, JsonSettings.ForFileStore) ?? new List<T>();

        }

        private void WriteToDisk(List<T> items)
        {
            var json = JsonConvert.SerializeObject(items, JsonSettings.ForFileStore);
            var tempPath = _filePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, _filePath, overwrite: true);
            File.Delete(tempPath);
        }

    }
}