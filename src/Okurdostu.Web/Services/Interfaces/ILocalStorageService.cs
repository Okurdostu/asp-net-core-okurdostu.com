using System.IO;

namespace Okurdostu.Web.Services
{
    public interface ILocalStorageService
    {
        /// <summary>
        /// Deletes a file from server if exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Deleting status: Existed and Deleted: true, not existed: false</returns>
        bool DeleteIfExists(string path);

        /// <summary>
        /// Uploads a image to const profile photo path with width size configurations.
        /// </summary>
        /// <param name="streamFile"></param>
        /// <param name="webRootPath">Environment.WebRootPath, Example: C:/Okurdostu/wwwroot </param>
        /// <param name="fileExtension">.jpg, .png, .jpeg</param>
        /// <returns>Uploaded location after the wwwroot path.</returns>
        string UploadProfilePhoto(Stream streamFile, string webRootPath, string fileExtension);
    }
}
