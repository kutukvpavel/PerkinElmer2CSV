using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvExportFixer
{
    enum FileCondition
    {
        DoNotProcess,
        Original,
        SecondaryProcessing
    }

    /// <summary>
    /// Intended to fix messed up CSV formatting in export files from TimeBase (yet)
    /// Main export drawback: number format culture infot is ignored, resulting in mixed separators.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            List<string> files = new List<string>();
            SearchOption recursive = args.Contains("-r") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string searchPattern = args.FirstOrDefault(x => x.StartsWith("-e:")) ?? "*.csv";
            foreach (var item in args)
            {
                if (File.Exists(item))
                {
                    files.Add(item);
                }
                else if (Directory.Exists(item))
                {
                    files.AddRange(Directory.GetFiles(item, searchPattern, recursive));
                }
            }
            foreach (var item in files)
            {
                string contents = File.ReadAllText(item);
                FileCondition c = MatchFile(contents, item);
                if (c != FileCondition.DoNotProcess)
                {
                    File.WriteAllText(item, ProcessFile(contents, c));
                    Console.WriteLine("Processed file: " + item);
                }
            }
        }

        static string ProcessFile(string contents, FileCondition c)
        {
            if (c == FileCondition.Original) contents = contents.FixHeader();
            contents = contents.FixCultureToInvariant();
            return contents;
        }

        static FileCondition MatchFile(string contents, string path)
        {
            if (path.Contains(".sp")) return FileCondition.DoNotProcess;
            if (contents.StartsWith("\"Name:\",")) return FileCondition.Original;
            if (contents.StartsWith("\"Wavenumber\",")) return FileCondition.SecondaryProcessing;
            return FileCondition.DoNotProcess;
        }
    }
}
