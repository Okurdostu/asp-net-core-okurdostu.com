using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Okurdostu.Web.Extensions
{
    public static class StringExtensions
    {
        public static string SHA512(this String str)
        {
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var StringBytes = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
                var StringBuilder = new System.Text.StringBuilder(128);
                foreach (var charbyte in StringBytes)
                    StringBuilder.Append(charbyte.ToString("X2"));
                return (StringBuilder.ToString());
            }
        }

        public static string UppercaseFirstCharacters(this String str)
        {
            char[] charArray = str.ToCharArray();
            if (char.IsLower(charArray[0])) charArray[0] = char.ToUpper(charArray[0]);
            for (int i = 1; i < charArray.Length; i++)
                if (charArray[i - 1] == ' ' && char.IsLower(charArray[i])) charArray[i] = char.ToUpper(charArray[i]);
            return new string(charArray);
        }

        public static string ClearSpaces(this String str)
        {
            //example input: "            asd       asd              asd           "
            char[] charArray = str.ToCharArray();
            for (int i = 0; i < charArray.Length - 1; i++)
                if (charArray[i] == ' ' && charArray[i + 1] == ' ') // 
                    charArray[i] = '_'; // converts spaces to _

            //" asd asd asd "
            str = new string(charArray).Replace("_", ""); // delete '_' [space] characters

            charArray = str.ToCharArray();
            if (charArray[0] == ' ')
                charArray[0] = '_';

            if (charArray[charArray.Length - 1] == ' ')
                charArray[charArray.Length - 1] = '_';

            //last output: "asd asd asd"
            return new string(charArray).Replace("_", ""); // delete '_' [space] characters and return
        }

        //bora kaşmer
        //http://www.borakasmer.com/net-core-mvcde-bir-haber-basligini-urle-koyma/
        public static string FriendlyUrl(this String url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            url = url.ToLower();
            url = url.Trim();
            if (url.Length > 100)
            {
                url = url.Substring(0, 100);
            }

            url = url.Replace("İ", "I");
            url = url.Replace("ı", "i");
            url = url.Replace("ğ", "g");
            url = url.Replace("Ğ", "G");
            url = url.Replace("ç", "c");
            url = url.Replace("Ç", "C");
            url = url.Replace("ö", "o");
            url = url.Replace("Ö", "O");
            url = url.Replace("ş", "s");
            url = url.Replace("Ş", "S");
            url = url.Replace("ü", "u");
            url = url.Replace("Ü", "U");
            url = url.Replace("'", "");
            url = url.Replace("\"", "");

            char[] replacerList = @"$%#@!*?;:~`+=()[]{}|\'<>,/^&"".".ToCharArray();

            for (int i = 0; i < replacerList.Length; i++)
            {
                string strChr = replacerList[i].ToString();
                if (url.Contains(strChr))
                {
                    url = url.Replace(strChr, string.Empty);
                }
            }

            Regex r = new Regex("[^a-zA-Z0-9_-]");
            url = r.Replace(url, "-");
            while (url.IndexOf("--") > -1)
                url = url.Replace("--", "-");
            return url;
        }
    }
}