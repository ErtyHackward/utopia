using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace UtopiaApi.Helpers
{
    public static class RandomHelper
    {
        public static string NextToken(this Random r, int len = 30)
        {
            var sb = new StringBuilder();

            char ch;
            for (int i = 0; i < len; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * r.NextDouble() + 65)));
                sb.Append(ch);
            }


            return sb.ToString();
        }
    }
}