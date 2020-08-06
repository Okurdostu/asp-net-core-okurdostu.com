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
        /// <param name="path"></param>
        /// <returns>Deleting status: Existed and Deleted: true, not existed: false</returns>
        bool DeleteIfExists(string path);

        /// <summary>
        /// Checks file acceptable to upload into server. Also Returns a 'string' warning if needs
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="fileType"></param>
        /// <returns>Turkish warning for user</returns>
        string WarnAcceptability(IFormFile formFile, FileType fileType);

        /// <summary>
        /// Uploads a image to const profile photo path with width size configurations.
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="webRootPath">Environment.WebRootPath, Example: C:/Okurdostu/wwwroot </param>
        /// <returns>Uploaded location after the wwwroot path.</returns>
        string UploadProfilePhoto(IFormFile formFile, string webRootPath);

        /// <summary>
        /// Uploads a image or document to const education document path.
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="webRootPath">Environment.WebRootPath, Example: C:/Okurdostu/wwwroot </param>
        /// <returns>Uploaded location after the wwwroot path.</returns>
        string UploadEducationDocument(IFormFile formFile, string webRootPath);


    }
}
