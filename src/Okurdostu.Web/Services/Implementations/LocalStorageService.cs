using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;

namespace Okurdostu.Web.Services
{
    public class StorageStatus
    {
        public bool Succes { get; set; }
        public string Message { get; set; }
        public string UploadedPathAfterRoot { get; set; }
    }

    public class LocalStorageService : ILocalStorageService
    {
        private static readonly string profilePhotoPath = "/image/profile/";
        private static readonly string educationDocumentPath = "/documents/";
        private static readonly int acceptableFileSize = 1048576 * 6;
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

        public StorageStatus UploadEducationDocument(IFormFile formFile, string webRootPath)
        {
            var returnStorageStatus = CheckAcceptability(formFile, FileType.Document);
            if (!returnStorageStatus.Succes)
            {
                return returnStorageStatus;
            }

            string documentFileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
            string filePathAfterRoot = educationDocumentPath + documentFileName;

            using (var Stream = File.Create(webRootPath + filePathAfterRoot))
            {
                formFile.CopyTo(Stream);
            };

            if (File.Exists(webRootPath + filePathAfterRoot))
            {
                returnStorageStatus.UploadedPathAfterRoot = filePathAfterRoot;
                return returnStorageStatus;
            }

            returnStorageStatus.Succes = false;
            return returnStorageStatus;
        }

        public StorageStatus UploadProfilePhoto(IFormFile formFile, string webRootPath)
        {
            var returnStorageStatus = CheckAcceptability(formFile, FileType.Photo);
            if (!returnStorageStatus.Succes)
            {
                return returnStorageStatus;
            }

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
                returnStorageStatus.UploadedPathAfterRoot = photoPathAfterRoot;
                return returnStorageStatus;
            }

            returnStorageStatus.Succes = false;
            return returnStorageStatus;
        }

        public StorageStatus CheckAcceptability(IFormFile formFile, FileType fileType)
        {
            var returnStorageStatus = new StorageStatus();

            if (formFile != null && formFile.Length <= acceptableFileSize)
            {
                switch (fileType)
                {
                    case FileType.Document:
                        {
                            var IsFileTypeAcceptable = acceptableFileTypesForDocument.Any(x => x == formFile.ContentType);

                            if (IsFileTypeAcceptable == false)
                            {
                                returnStorageStatus.Message = "Sadece fotoğraf veya PDF dökümanı seçebilirsiniz";
                            }

                            break;
                        }
                    case FileType.Photo:
                        {
                            var IsFileTypeAcceptable = acceptableFileTypesForPhoto.Any(x => x == formFile.ContentType);

                            if (IsFileTypeAcceptable == false)
                            {
                                returnStorageStatus.Message = "Sadece fotoğraf seçebilirsiniz";
                            }

                            break;
                        }
                }
            }
            else
            {
                returnStorageStatus.Message = formFile != null && formFile.Length > acceptableFileSize
                    ? "Yüklemeye çalıştığınız dosya kabul edilebilir boyutları aşıyor"
                    : "Dosyaya ulaşılamadı";
            }

            if (string.IsNullOrEmpty(returnStorageStatus.Message))
            {
                returnStorageStatus.Succes = true;
            }

            return returnStorageStatus;
        }
    }
}
