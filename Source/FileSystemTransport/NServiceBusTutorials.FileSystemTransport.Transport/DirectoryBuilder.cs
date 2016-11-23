using System;
using System.IO;

namespace NServiceBusTutorials.FileSystemTransport.Transport
{
    internal static class DirectoryBuilder
    {
        public static string BuildBasePath(string address)
        {
            var tempDirectory = Environment.ExpandEnvironmentVariables("%temp%");
            var directory = Path.Combine(tempDirectory, "FileTransport", address);
            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}