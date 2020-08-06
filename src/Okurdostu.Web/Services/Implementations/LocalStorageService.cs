using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace Okurdostu.Web.Services
{
    public class LocalStorageService : ILocalStorageService
    {
        private const string profilePhotoPath = "/image/profile/";
        private const string educationDocumentPath = "/documents/";

        public bool DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }

        public string UploadProfilePhoto(Stream streamFile, string webRootPath, string fileExtension)
        {
            string newPhotoFileName = Guid.NewGuid().ToString() + fileExtension;
            string photoPathAfterRoot = profilePhotoPath + newPhotoFileName;

            var profilePhoto = Image.Load(streamFile);
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
    }
}
