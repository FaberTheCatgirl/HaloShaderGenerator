using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloShaderGenerator
{
    static class QueuedConsole
    {
        private static object Lock = new object();

        public static void QueueMessage(string message, ConsoleColor colour)
        {
            lock (Lock)
            {
                Console.ForegroundColor = colour;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
    }
}
