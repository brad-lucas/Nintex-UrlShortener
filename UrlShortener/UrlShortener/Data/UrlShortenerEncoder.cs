using System.Linq;
using System.Text;

namespace NintexUrlShortener.Data
{
    public static class UrlShortenerEncoder
    {
        public const string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static int Base
        {
            get { return Alphabet.Length; }
        }

        public static long Decode(string s)
        {
            var i = 0;

            foreach (var c in s)
            {
                i = (i * Base) + Alphabet.IndexOf(c);
            }

            return i;
        }

        public static string Encode(int i)
        {
            if (i == 0)
            {
                return Alphabet[0].ToString();
            }

            var sb = new StringBuilder();

            while (i > 0)
            {
                sb.Append(Alphabet[i % Base]);
                i = i / Base;
            }

            return string.Join(string.Empty, sb.ToString().Reverse());
        }
    }
}