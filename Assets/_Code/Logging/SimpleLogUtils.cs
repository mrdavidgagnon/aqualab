using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Logging
{
    public class SimpleLogUtils
    {
        // https://stackoverflow.com/questions/28833373/javascript-splice-in-c-sharp
        public static List<T> Splice<T>(List<T> source, int index, int count)
        {
            var items = source.GetRange(index, count);
            source.RemoveRange(index, count);
            return items;
        }

        // https://stackoverflow.com/questions/46093210/c-sharp-version-of-the-javascript-function-btoa
        public static string btoa(string str)
        {
            byte[] bytes = Encoding.GetEncoding(28591).GetBytes(str); // Give constant name - Western European ISO
            string str64 = System.Convert.ToBase64String(bytes);
            return str64;
        }

        public static long UUIDint()
        {
            DateTime dt = DateTime.Now;
            string id = ("" + dt.Year).Substring(2);

            if (dt.Month < 10) id += "0";
            id += dt.Month;

            if (dt.Day < 10) id += "0";
            id += dt.Day;

            if (dt.Hour < 10) id += "0";
            id += dt.Hour;

            if (dt.Minute < 10) id += "0";
            id += dt.Minute;

            if (dt.Second < 10) id += "0";
            id += dt.Second;

            System.Random rand = new System.Random();

            for (int i = 0; i < 5; ++i)
            {
                id += Math.Floor(rand.NextDouble() * 10);
            }

            return Int64.Parse(id);
        }

        // Build for WebGL
        public static string GetCookie(Cookie cookie, string name)
        {
            string full_cookie = Uri.EscapeDataString(cookie.Value);

            if (full_cookie == "")
            {
                return "";
            }

            string[] cookies = full_cookie.Split(';');
            string full_name = name + "=";

            for (int i = 0; i < cookies.Length; ++i)
            {
                string c = cookies[i];

                while (c[0] == ' ')
                {
                    c = c.Substring(1);
                }

                if (c.IndexOf(full_name) == 0)
                {
                    return c.Substring(full_name.Length, c.Length);
                }
            }

            return "";
        }

        // Use days to increment
        public static void SetCookie(Cookie cookie, string name, long val, int days)
        {
            cookie.Value = name + "=" + val + "; expires=" + DateTime.Now.ToString() + "; path=/";
        }
    }
}

