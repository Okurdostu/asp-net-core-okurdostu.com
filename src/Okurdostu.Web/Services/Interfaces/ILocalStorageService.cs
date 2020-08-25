using Microsoft.AspNetCore.Http;

namespace Okurdostu.Web.Services
{
    public enum FileType
    {
        Document,
        Photo
    }

    public interface ILocalStorageService
    {
        /// <summary>
        /// Deletes a file from server if exists.
        /// </summary>
        /// <param name="path">File full path</param>
        /// <returns><see cref="System.Boolean"></see> Deleting status: Existed and Deleted: true, not existed: false</returns>
        bool DeleteIfExists(string path);

        /// <summary>
        /// Checks file acceptable to upload into server. Also Returns a 'string' warning if needs
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="fileType"></param>
        /// <returns><see cref="StorageStatus"/></returns>
        StorageStatus CheckAcceptability(IFormFile formFile, FileType fileType);

        /// <summary>
        /// Uploads a image to const profile photo path with width size configurations.
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="webRootPath">Environment.WebRootPath, Example: C:/Okurdostu/wwwroot </param>
        /// <returns><see cref="StorageStatus"/></returns>
        StorageStatus UploadProfilePhoto(IFormFile formFile, string webRootPath);

        /// <summary>
        /// Uploads a image or document to const education document path.
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="webRootPath">Environment.WebRootPath, Example: C:/Okurdostu/wwwroot </param>
        /// <returns><see cref="StorageStatus"/></returns>
        StorageStatus UploadEducationDocument(IFormFile formFile, string webRootPath);


    }
}
