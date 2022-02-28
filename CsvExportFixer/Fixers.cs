using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CsvExportFixer
{
    public static class Fixers
    {
        /// <summary>
        /// Use after <see cref="FixHeader(string)"/>
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static string FixCultureToInvariant(this string contents)
        {
            using StringReader r = new StringReader(contents);
            using StringWriter w = new StringWriter();
            w.WriteLine(r.ReadLine()); //Header
            string line = null;
            while ((line = r.ReadLine()) != null)
            {
                w.WriteLine(LineCultureHelper(line));
            }
            return w.ToString();
        }
        public static string FixHeader(this string contents)
        {
            using StringReader r = new StringReader(contents);
            using StringWriter w = new StringWriter();
            for (int i = 0; i < 3; i++)
            {
                r.ReadLine();
            }
            w.WriteLine(r.ReadLine());
            r.ReadLine();
            w.Write(r.ReadToEnd());
            return w.ToString();
        }


        private static string LineCultureHelper(string l)
        {
            l = l.Replace(",00, ", ".00, ");
            int i = l.IndexOf(',') + 1;
            i = l.IndexOf(',', i);
            if (i > -1) l = l.Remove(i, 1).Insert(i, ".");
            return l;
        }
    }
}
