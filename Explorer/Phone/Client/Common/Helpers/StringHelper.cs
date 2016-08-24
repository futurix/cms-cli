using System;
using System.Collections.Generic;
using System.Text;

namespace Wave.Common
{
    public static class StringHelper
    {
        public static int GetByteCount(string input)
        {
            if (input != null)
                return Encoding.UTF8.GetByteCount(input);
            else
                return 0;
        }
        
        public static byte[] GetBytes(string input)
        {
            if (input != null)
                return Encoding.UTF8.GetBytes(input);
            else
                return null;
        }

        public static byte[] GetBytes(char[] input)
        {
            if (input != null)
                return Encoding.UTF8.GetBytes(input);
            else
                return null;
        }

        public static string GetString(byte[] input)
        {
            if (input != null)
                return Encoding.UTF8.GetString(input, 0, input.Length);
            else
                return null;
        }
        
        public static List<string> FindEnclosures(string source, char startTag, char endTag)
        {
            List<string> res = new List<string>();

            if (startTag != endTag)
            {
                int pos = 0;

                while (pos != -1)
                {
                    pos = source.IndexOf(startTag, pos);

                    if (pos != -1)
                    {
                        int startPos = pos;

                        pos = source.IndexOf(endTag, pos);

                        if (pos != -1)
                        {
                            int endPos = pos;

                            if (startPos < (endPos - 1))
                            {
                                string temp = source.Substring(startPos + 1, endPos - startPos - 1);

                                if (!String.IsNullOrEmpty(temp))
                                    res.Add(temp);
                            }
                        }
                    }
                }
            }

            return res;
        }

        public static List<Pair<string, string>> FindPairs(string source, char mainDelim, char pairDelim)
        {
            List<Pair<string, string>> res = new List<Pair<string, string>>();

            string[] pairHolders = source.Split(new char[] { mainDelim }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pairHolder in pairHolders)
            {
                string[] pair = pairHolder.Split(new char[] { pairDelim }, StringSplitOptions.RemoveEmptyEntries);

                if (pair.Length == 2)
                    res.Add(new Pair<string, string>(pair[0], pair[1]));
            }

            return res;
        }

        public static string Until(string source, char end)
        {
            if (!String.IsNullOrEmpty(source))
            {
                int pos = source.IndexOf(end);

                if (pos > 0)
                    return source.Substring(0, pos);
            }

            return String.Empty;
        }
    }
}
