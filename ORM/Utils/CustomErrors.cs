using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.Utils
{
    public static class CustomErrors
    {
        private static readonly string notification = "Error:";

        public static void RecordNotFound(int id)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{notification} Record with id = [{id}] not found");
            Console.ResetColor();
        }

        public static void RecordExists(int id)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{notification} Record with id = [{id}] already exists");
            Console.ResetColor();
        }
    }
}
