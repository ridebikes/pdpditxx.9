using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;

namespace pdpditxx
{
    internal class ZipProcess
    {
        public static AppSettings.Root ProcessInputFile(AppSettings.Root appSettings, string appConfigDir, string zipWorkDir, FileInfo inputFile)
        {
            // unzip the input file
            ZipFile.ExtractToDirectory(inputFile.FullName, zipWorkDir);

            Console.WriteLine($"Unzipped {Directory.GetFiles(zipWorkDir).Length} files.");

            // check if the JSON file exists
            if (Directory.GetFiles(zipWorkDir, "*.itext.config.json").Length == 0)
            {
                throw new FileNotFoundException($"There is no *.itext.config.json configuration file.");
            }

            // get the name of the JSON file in our zipworkdir
            FileInfo thisConfigJson = new FileInfo(Directory.GetFiles(zipWorkDir, "*.itext.config.json").FirstOrDefault());

            Console.WriteLine($"JSON Config in use : {thisConfigJson.Name}");

            //move the appconfig file to the app config directory so we don't lose it
            File.Move(thisConfigJson.FullName, $"{appConfigDir}{thisConfigJson.Name}");

            //Deserialize this to the appSettings class
            using (StreamReader zipJson = File.OpenText($"{appConfigDir}{thisConfigJson.Name}"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
                appSettings = (AppSettings.Root)serializer.Deserialize(zipJson, typeof(AppSettings.Root));
            }

            return appSettings;
        }

        public static List<string> SortToConcat(FileInfo inputFile, string zipWorkDir)
        {
            string indexFileName = $"{Path.GetFileNameWithoutExtension(inputFile.FullName)}.idx";
            string indexFile = $"{zipWorkDir}{indexFileName}";

            if (!File.Exists(indexFile))
            {
                throw new FileNotFoundException($"Index File : {indexFileName} not found");
            }

            List<string> mergeIndex = new List<string>();

            foreach (string line in File.ReadLines(indexFile))
            {
                string[] lineParts = line.Split("||");
                string inFileName = lineParts[1];
                mergeIndex.Add($"{zipWorkDir}{inFileName}");
            }
            return mergeIndex;
        }

        public static List<string> SortToProcess(string zipWorkDir, string zipOutDir)
        {
            //get listing of pdf files in temp zip directory
            List<string> otherFilesInZip = Directory.EnumerateFiles(zipWorkDir).Where(x => !x.ToLower().EndsWith(".pdf")).ToList();
            List<string> pdfFilesInZip = Directory.EnumerateFiles(zipWorkDir).Where(x => x.ToLower().EndsWith(".pdf")).ToList();

            if (otherFilesInZip.Count > 0)
            {
                foreach (string otherFile in otherFilesInZip)
                {
                    File.Move(otherFile, $"{zipOutDir}{Path.GetFileName(otherFile)}");
                }
            }

            return pdfFilesInZip;

        }

        public static List<SplitIndex> SortToSplit(FileInfo inputFile, string zipWorkDir, string zipOutDir)
        {
            string indexFileName = $"{Path.GetFileNameWithoutExtension(inputFile.FullName)}.idx";
            string indexFile = $"{zipWorkDir}{indexFileName}";

            if (!File.Exists(indexFile))
            {
                throw new FileNotFoundException($"Index File : {indexFileName} not found");
            }

            List<SplitIndex> splitIndex = new List<SplitIndex>();
            SplitIndex thisSplit = new SplitIndex();

            foreach (string line in File.ReadLines(indexFile))
            {
                string[] lineParts = line.Split("||");
                thisSplit.FileName = $"{zipOutDir}{lineParts[1]}";
                thisSplit.FirstPage = int.Parse(lineParts[2]);
                thisSplit.LastPage = int.Parse(lineParts[3]);
                splitIndex.Add(thisSplit);
                thisSplit = new SplitIndex();
            }
            return splitIndex;
        }

        public static void ZipProcessedOutput(string zipOutDir, string zipWorkDir, FileInfo inputFile, string outDir)
        {
            try
            {
                string zipFileName = $"{Path.GetFileNameWithoutExtension(inputFile.FullName)}.zip";
                ZipFile.CreateFromDirectory(zipOutDir, $"{zipWorkDir}{zipFileName}");
                File.Copy($"{zipWorkDir}{zipFileName}", $"{outDir}{zipFileName}", true);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error in ZipProcessedOutput : {e.Message}");
            }
        }

    }
}
