using System.Threading.Tasks;

namespace Okurdostu.Web.Services
{
    public interface ILocalStorageService
    {
        /// <summary>
        /// Deletes a file from server if exists. Returns deleting status: Existed and Deleted: true, not existed: false
        /// </summary>
        /// <param name="path"></param>
        bool DeleteIfExists(string path);
    }
}
