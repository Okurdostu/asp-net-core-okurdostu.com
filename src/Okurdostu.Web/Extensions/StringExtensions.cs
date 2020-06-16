using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Okurdostu.Web.Extensions
{
    public static class StringExtensions
    {
        public static string SHA512(this String str)
        {
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashGirdi = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
                var stringhashGirdi = new System.Text.StringBuilder(128);
                foreach (var a in hashGirdi)
                    stringhashGirdi.Append(a.ToString("X2"));
                return (stringhashGirdi.ToString());
            }
        }
    }
}
