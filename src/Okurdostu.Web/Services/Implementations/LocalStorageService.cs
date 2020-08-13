using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;

namespace Okurdostu.Web.Services
{
    public class LocalStorageService : ILocalStorageService
    {
        private static readonly string profilePhotoPath = "/image/profile/";
        private static readonly string educationDocumentPath = "/documents/";
        private static readonly long acceptableFileSize = 1048576 / 2;
        private static readonly string[] acceptableFileTypesForDocument = { "application/pdf", "image/png", "image/jpg", "image/jpeg" };
        private static readonly string[] acceptableFileTypesForPhoto = { "image/png", "image/jpg", "image/jpeg" };

        public bool DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }
        public string UploadEducationDocument(IFormFile formFile, string webRootPath)
        {
            string documentFileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
            string filePathAfterRoot = educationDocumentPath + documentFileName;

            using (var Stream = File.Create(webRootPath + filePathAfterRoot))
            {
                formFile.CopyTo(Stream);
            };
            if (File.Exists(webRootPath + filePathAfterRoot))
            {
                return filePathAfterRoot;
            }

            return null;
        }
        public string UploadProfilePhoto(IFormFile formFile, string webRootPath)
        {
            string photoFileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
            string photoPathAfterRoot = profilePhotoPath + photoFileName;
            var profilePhoto = Image.Load(formFile.OpenReadStream());

            if (profilePhoto.Width > 200)
            {
                profilePhoto.Mutate(x => x.Resize(200, 200));
            }
            profilePhoto.Save(webRootPath + photoPathAfterRoot);
            if (File.Exists(webRootPath + photoPathAfterRoot))
            {
                return photoPathAfterRoot;
            }

            return null;
        }
        public string WarnAcceptability(IFormFile formFile, FileType fileType)
        {
            if (formFile != null && formFile.Length <= acceptableFileSize)
            {
                if (fileType == FileType.Document)
                {
                    var IsFileTypeAcceptable = !acceptableFileTypesForDocument.Any(x => x == formFile.ContentType);

                    if (IsFileTypeAcceptable)
                    {
                        return "Sadece fotoğraf veya PDF dökümanı seçebilirsiniz";
                    }
                }
                else if (fileType == FileType.Photo)
                {
                    var IsFileTypeAcceptable = !acceptableFileTypesForPhoto.Any(x => x == formFile.ContentType);

                    if (IsFileTypeAcceptable)
                    {
                        return "Sadece fotoğraf seçebilirsiniz";
                    }
                }
            }
            else if (formFile != null && formFile.Length > acceptableFileSize)
            {
                return "Yüklemeye çalıştığınız dosya kabul edilebilir boyutları aşıyor";
            }
            else
            {
                return "Dosya seçmediniz";
            }

            return null;
        }
    }
}
