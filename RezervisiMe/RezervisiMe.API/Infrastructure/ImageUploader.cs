using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace RezervisiMe.RezervisiMe.API.Infrastructure
{

    public static class ImageUploader
    {
        private static readonly string[] AllowedExt = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxBytes = 5 * 1024 * 1024;

        public static async Task<Result<string>> SaveFromRequest(HttpRequestMessage request)
        {
            if (request == null || !request.Content.IsMimeMultipartContent())
                return Error.Validation("Multipart sadržaj očekivan");

            var root = HostingEnvironment.MapPath("~/Content/uploads/");
            if (root == null) return Error.Internal("MapPath vratio null");
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);

            var provider = new MultipartMemoryStreamProvider();
            await request.Content.ReadAsMultipartAsync(provider);

            foreach (var part in provider.Contents)
            {
                var rawName = part.Headers.ContentDisposition?.FileName;
                if (string.IsNullOrEmpty(rawName)) continue;

                var name = rawName.Trim('"');
                var ext = Path.GetExtension(name).ToLowerInvariant();
                if (!AllowedExt.Contains(ext))
                    return Error.Validation(
                        $"Nepodržan format slike: {ext}. Dozvoljeno: {string.Join(", ", AllowedExt)}");

                var bytes = await part.ReadAsByteArrayAsync();
                if (bytes.Length > MaxBytes)
                    return Error.Validation($"Slika je veća od {MaxBytes / 1024 / 1024} MB");

                var fileName = $"{Guid.NewGuid():N}{ext}";
                File.WriteAllBytes(Path.Combine(root, fileName), bytes);
                return fileName;
            }

            return Error.Validation("Nijedan fajl nije primljen");
        }
    }
}