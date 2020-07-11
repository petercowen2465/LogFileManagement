using System;

namespace LogFileManagement
{
    using System.Configuration;
    using System.IO;
    using Newtonsoft.Json.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            
            string fileName = ConfigurationManager.AppSettings["LogFiles"];

            if (fileName is null)
            {
                Console.WriteLine("LogFiles is not configured in AppSettings");
                System.Environment.Exit(-1);
            }
           
            if (File.Exists(fileName) == false)
            {
                Console.WriteLine("AppSettings.LogFiles {0} does not exit", fileName);
                System.Environment.Exit(-1);
            }


            JArray logFileArray = JArray.Parse(File.ReadAllText(fileName));

            foreach (var logFile in logFileArray)
            {
                string folder = logFile["Folder"].Value<string>();
                string filePattern = logFile["FilePattern"].Value<string>();
                int retentionPeriodDays = logFile["RetentionPeriodDays"].Value<int>();

                ManageFolderRetention(folder, filePattern, retentionPeriodDays);
            }
            
        }

        private static void ManageFolderRetention(string folder, string filePattern, int retentionPeriodDays)
        {
            if (Directory.Exists(folder) == false)
            {
                Console.WriteLine("Log folder {0} does not exist", folder);
            } else
            {
                Console.WriteLine("Folder {0}, pattern {1}\n", folder, filePattern);
                foreach (string file in Directory.EnumerateFiles(folder, filePattern)){
                    FileInfo fi = new FileInfo(file);
                    if (fi.CreationTime < DateTime.Now.AddDays(-retentionPeriodDays))
                    {
                        Console.WriteLine(file);
                    }
                }
                
            }
        }
    }
}
