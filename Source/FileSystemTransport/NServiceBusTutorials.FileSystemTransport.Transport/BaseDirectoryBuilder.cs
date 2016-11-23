using System;
using System.IO;

namespace NServiceBusTutorials.FileSystemTransport.Transport
{
    public static class BaseDirectoryBuilder
    {
        public static string BuildBasePath(string address)
        {
            var temp = Environment.ExpandEnvironmentVariables("%temp%");
            var fullPath = Path.Combine(temp, "FileTransport", address);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }
    }
}