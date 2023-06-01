using Yarhl.IO;

namespace ParBoil
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }

        public static DataStream CopyStreamFromFile(string fileName, FileOpenMode mode)
        {
            using DataStream fileStream = DataStreamFactory.FromFile(fileName, mode);
            DataStream memoryStream = DataStreamFactory.FromMemory();

            fileStream.WriteTo(memoryStream);

            return memoryStream;
        }
    }
}