using System;

namespace LogFileManagement
{
    using System.Configuration;
    using System.IO;
    using System.Xml.Linq;
    using Newtonsoft.Json.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            PurgeLogFiles();

        }
        enum ConfigFileType
        {
            xml,
            json,
            notdefined,
            notexists,
            unknown
        }
        private static ConfigFileType GetConfig(string configItem, ref string fileName)
        {
            fileName = ConfigurationManager.AppSettings[configItem];

            if (fileName is null)
            {
                Console.WriteLine("{0} is not configured in AppSettings", configItem);
                fileName = "not configured";
                return ConfigFileType.notdefined;
            }

            if (File.Exists(fileName) == false)
            {
                Console.WriteLine("AppSettings.{0} {1} does not exist", configItem, fileName);
                return ConfigFileType.notexists;
            }

            string fileExtension = Path.GetExtension(fileName);
            if (String.Equals(fileExtension, ".json", StringComparison.OrdinalIgnoreCase))
            {
                return ConfigFileType.json;
            }
            else if (String.Equals(fileExtension, ".xml", StringComparison.OrdinalIgnoreCase))
            {
                return ConfigFileType.xml;
            }
            else
            {
                Console.WriteLine("AppSettings.LogFiles {0} unknown file type", fileName);
            }
            return ConfigFileType.unknown;
        }
        private static void PurgeLogFiles()
        {
            string fileName = "";
            ConfigFileType fileType;

            fileType = GetConfig("LogFiles", ref fileName);
            switch(fileType) {
                case ConfigFileType.xml:
                    PurgeLogFilesXMLConfig(fileName);
                    break;
                case ConfigFileType.json:
                    PurgeLogFilesJSONConfig(fileName);
                    break;
                default:
                    Console.WriteLine("AppSettings.{0} fileName={1} fileType={2}", "LogFile", fileName, fileType);
                    break;
            }

        }
        private static void PurgeLogFilesJSONConfig(string fileName)
        {
            JArray logFileArray = JArray.Parse(File.ReadAllText(fileName));
            string folder;
            string filePattern;
            int retentionPeriodDays;
            foreach (var logFile in logFileArray)
            {
                try
                {
                    folder = logFile["Folder"].Value<string>();
                    filePattern = logFile["FilePattern"].Value<string>();
                    retentionPeriodDays = logFile["RetentionPeriodDays"].Value<int>();
                } catch (Exception ex)
                {
                    Console.WriteLine("Failed to parse App.Config line {0}: {1}", logFile.ToString(), ex.Message);
                    continue;
                }
                ManageFolderRetention(folder, filePattern, retentionPeriodDays);
            }
        }
        private static void PurgeLogFilesXMLConfig(string fileName)
        {
            var xml = XDocument.Load(fileName);

            foreach (XElement xe in xml.Descendants("LogFile"))
            {
                string folder;
                string filePattern;
                int retentionPeriodDays;
                try
                {
                    folder = xe.Attribute("Folder").Value;
                    filePattern = xe.Attribute("FilePattern").Value;
                    retentionPeriodDays = Int32.Parse(xe.Attribute("RetentionPeriodDays").Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to parse App.config line {0}:{1}",
                        xe.ToString(), ex.Message);
                    continue;
                }

                ManageFolderRetention(folder, filePattern, retentionPeriodDays);
            }
        }
        private static void ManageFolderRetention(string folder, string filePattern, int retentionPeriodDays)
        {
            if (Directory.Exists(folder) == false)
            {
                Console.WriteLine("Log folder {0} does not exist", folder);
            }
            else
            {
                Console.WriteLine("Folder {0}, pattern {1}\n", folder, filePattern);
                foreach (string file in Directory.EnumerateFiles(folder, filePattern))
                {
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
