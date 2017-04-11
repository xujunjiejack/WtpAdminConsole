using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This program is the console function for wtp_admin.
/// It's used by IT people to automatically do data communication between wtp_collab
/// through system scheduler.
/// GUI program can't achieve this.
/// </summary>

namespace WtpAdminConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
                onlyRunDump();
            else if (args.Length == 1)
            {
                if (args[0].Equals("update"))
                    runWithBackup();
                else
                {
                    Console.WriteLine("Usage: no argument: only update mysql");
                    Console.WriteLine("       [update]: update sqlite db");
                }
            }
            else
            {
                Console.WriteLine("Usage: no argument: only update mysql");
                Console.WriteLine("       [update]: update sqlite db");
            }
            
            Console.Read();
        }

        private static void onlyRunDump() {
            var admin = new Admin();
            admin.dumpTablesFromSQLite();
            Console.WriteLine("Dumping table done");
        }

        private static void runWithBackup()
        {
            Console.WriteLine("Run admin with python migrator. Need to be implemented");
        }
    }
}
