using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using System.Diagnostics;

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
        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            logger.Info("");
            logger.Info("--------- Wtp Admin Console starts ---------");
            //TODO: Try to build good log file
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
            try
            {
                // Run the main function. The others are error message
                var admin = new Admin();
                admin.dumpTablesFromSQLite();

                Console.WriteLine("Dumping table done");
                logger.Info("Dumping table done");
                System.Environment.Exit(0);
            }
            catch (UserTableTracker.SqliteTrackerOperationError e)
            {
                logger.Fatal("Program terminates due to sqlite user table operation fail");
                System.Environment.Exit(1);
            } catch (Admin.WtpdataTrackerOperationError e)
            {
                logger.Fatal("Program terminates due to mysql user table operation fail");
                System.Environment.Exit(1);
            } 
        }

        private static void runWithBackup()
        {
            Console.WriteLine("Run admin with python migrator");
            logger.Info("Run admin with python migrator");
            // C:\Users\jxu259\PycharmProjects\MysqlToSQLite

            // Run the python script
            ProcessStartInfo mysqlToSqliteProcessInfo = new ProcessStartInfo("python");
            mysqlToSqliteProcessInfo.Arguments = "C:\\Users\\jxu259\\PycharmProjects\\MysqlToSQLite\\mysqlToSqlite.py";
            Process process = Process.Start(mysqlToSqliteProcessInfo);
            process.WaitForExit();
            
            //
            if (process.ExitCode != 0)
            {
                logger.Fatal("Python migrator fails due to error, please check the log file");
                Console.WriteLine("Python migrator fails due to error, please check the log file");
                Environment.Exit(1);
            }
            logger.Info("Sqlite Update finish");
            Console.WriteLine("Sqlite Update finish");
        }
    }
}
