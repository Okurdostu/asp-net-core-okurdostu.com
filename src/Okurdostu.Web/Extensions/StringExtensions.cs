using System;
using System.Text.RegularExpressions;

namespace Okurdostu.Web.Extensions
{
    public static class StringExtensions
    {
        public static string SHA512(this String str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return str;
            }

            using var hash = System.Security.Cryptography.SHA512.Create();
            var StringBytes = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
            var StringBuilder = new System.Text.StringBuilder(128);
            foreach (var charbyte in StringBytes)
            {
                StringBuilder.Append(charbyte.ToString("X2"));
            }
            return (StringBuilder.ToString());
        }

        public static string CapitalizeFirstCharOfWords(this String str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return str;
            }

            char[] strArray = str.ToLower().ToCharArray();
            if (char.IsLower(strArray[0]))
            {
                strArray[0] = char.ToUpper(strArray[0]);
            }
            for (int i = 1; i < strArray.Length; i++)
            {
                if (strArray[i - 1] == ' ' && char.IsLower(strArray[i]))
                {
                    switch (strArray[i])
                    {
                        case 'i':
                            strArray[i] = 'İ';
                            break;
                        case 'ı':
                            strArray[i] = 'I';
                            break;
                        case 'ş':
                            strArray[i] = 'Ş';
                            break;
                        case 'ç':
                            strArray[i] = 'Ç';
                            break;
                        case 'ğ':
                            strArray[i] = 'Ğ';
                            break;
                        case 'ü':
                            strArray[i] = 'Ü';
                            break;
                        case 'ö':
                            strArray[i] = 'Ö';
                            break;
                        default:
                            strArray[i] = char.ToUpper(strArray[i]);
                            break;
                    }
                }
            }
            return new string(strArray);
        }

        public static string ClearExtraBlanks(this String str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            //example input: "            asd       asd              asd           "
            char[] charArray = str.ToCharArray();
            for (int i = 0; i < charArray.Length - 1; i++)
            {
                if (charArray[i] == ' ' && charArray[i + 1] == ' ')
                {
                    charArray[i] = '_'; // converts blanks to _
                }
            }

            //last and first character checks
            if (charArray[0] == ' ')
            {
                charArray[0] = '_';
            }
            if (charArray[^1] == ' ')
            {
                charArray[^1] = '_';
            }

            return new string(charArray).Replace("_", ""); // delete '_' [space] characters and return
        }

        //FriendlyUrl author: Bora Kaşmer
        //http://www.borakasmer.com/net-core-mvcde-bir-haber-basligini-urle-koyma/
        public static string FriendlyUrl(this String url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

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
            {
                url = url.Replace("--", "-");
            }
            return url;
        }

        public static string NormalizePrice(this String price)
        {
            if (string.IsNullOrEmpty(price))
            {
                return price;
            }

            //7'ye bir tanım bulamadım.
            //7;    örnek olarak 1500,00 gibi bir price geldiği zaman
            //      1.500,00 olarak döndürmek için kullanılıyor
            price = price.Replace(".", ",");

            return price.Substring(0, price.Length - 2).Length >= 7 ? AddDot(price.Substring(0, price.Length - 2)) : price.Substring(0, price.Length - 2);
        }
        private static string AddDot(string price)
        {
            char[] _price = price.ToCharArray();
            string DottedPrice = "";
            for (int x = 0; x < _price.Length; x++)
            {
                if (_price.Length - 7 == x)
                {
                    DottedPrice += _price[x];
                    DottedPrice += ".";
                }
                else
                {
                    DottedPrice += _price[x];
                }
            }
            return DottedPrice;
        }

        public static string StarsToEmail(this String email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return email;
            }

            //input     halil.i.kocaoz@gmail.com
            //output    ha************@g*****com
            char[] emailArray = email.ToCharArray();
            for (int i = 2; i < emailArray.Length - 3; i++)
            {
                if (emailArray[i] != '@' && emailArray[i - 1] != '@')
                {
                    emailArray[i] = '*';
                }
            }
            return new string(emailArray);
        }

        public static string ReplaceRandNsToBR(this String text)  =>
        !string.IsNullOrEmpty(text) ? text.Replace("\r\n", "<br/>").Replace("\r", "<br/>").Replace("\n", "<br/>") : text;

        public static string RemoveLessGreaterSigns(this String text) =>
        !string.IsNullOrEmpty(text) ? text.Replace(">", "").Replace("<", "") : text;

    }
}