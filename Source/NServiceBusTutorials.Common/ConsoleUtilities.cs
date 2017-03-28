using System;

namespace NServiceBusTutorials.Common
{
    public static class ConsoleUtilities
    {
        public static void WriteWithColor(string message, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            Console.Write(message);

            Console.ForegroundColor = originalColor;
        }

        public static void WriteLineWithColor(string message, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            Console.WriteLine(message);

            Console.ForegroundColor = originalColor;
        }
    }
}