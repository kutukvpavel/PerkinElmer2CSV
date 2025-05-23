﻿using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace PerkinElmerSP2CSV
{
    static class Program
    {
        static readonly Dictionary<string, IFileProvider> SupportedProviders = (new IFileProvider[]
        {
            SpFileProvider.Instance,
            PrfFileProvider.Instance
        }).ToDictionary(x => x.Extension, x => x);

        static readonly CsvConfiguration CsvConf = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
        static bool RecursiveOption = false;
        static bool OverwriteOption = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Perkin Elmer CSV toolkit v2.1 started!");
            Console.CancelKeyPress += Console_CancelKeyPress;
            RecursiveOption = args.Contains("-r");
            OverwriteOption = args.Contains("-o");
            Console.WriteLine($"Recursive folder processing: {RecursiveOption}, Overwrite existing CSV: {OverwriteOption}");
            List<string> files = new List<string>();
            if (args.Length > 0)
            {
                foreach (var item in args)
                {
                    files.AddRange(GetFileOrDir(item));
                }
            }
            else
            {
                files.AddRange(Directory.GetFiles(Environment.CurrentDirectory));
            }
            var query = files.Where(x => SupportedProviders.Keys.Contains(Path.GetExtension(x).ToLower()));
            if (!OverwriteOption)
            {
                query = query.Where(x => !File.Exists(GetOutputFilePath(x)));
            }
            files = query.ToList();
            Console.WriteLine($"Info: total files to process = {files.Count}.");
            Parallel.ForEach(files, ProcessFile);
            Console.WriteLine("Finished.");
        }

        static void ProcessFile(string path)
        {
            try
            {
                using TextWriter tw = new StreamWriter(GetOutputFilePath(path));
                using CsvWriter w = new CsvWriter(tw, CsvConf);
                var d = SupportedProviders[Path.GetExtension(path)].GetData(path);
                d?.WriteCsv(w);
                Console.WriteLine(d == null ? $"Warning: no data found in '{path}'." : $"Info: processed file '{path}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: for file '{path}', {Environment.NewLine}\t{ex}");
            }
        }

        static string[] GetFileOrDir(string path)
        {
            if (File.Exists(path))
            {
                return new string[] { path };
            }
            else if (Directory.Exists(path))
            {
                return RecursiveOption ? Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    : Directory.GetFiles(path);
            }
            else
            {
                return new string[] { };
            }
        }

        static string GetOutputFilePath(string inputPath)
        {
            return inputPath + ".csv";
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
