using System;

namespace LogFileManagement
{
    using System.Configuration;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            string fileName = ConfigurationManager.AppSettings["LogFiles"];
            Console.WriteLine("Hello World! How are you?"+fileName);

            if (File.Exists(fileName))
            {
                Console.WriteLine(File.ReadAllText(fileName));
            } else
            {
                Console.WriteLine("file doesn't exist");
            }
        }
    }
}
