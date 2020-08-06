

using System;
using System.IO;

namespace Okurdostu.Web.Services
{
    public class LocalStorageService : ILocalStorageService
    {
        public bool DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }
    }
}
