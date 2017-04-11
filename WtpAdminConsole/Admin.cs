using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Odbc;
using System.Data.SQLite;
using System.Diagnostics;
using System.Collections;

namespace WtpAdminConsole
{

    /// <summary>
    ///     Today, I'm going to add the functionality that imports user file in the collab databse back to the wtp_data
    ///     
    ///     Here are couple things I need to worry: 
    ///         How do I know which table should be imported
    ///         How to write them back
    ///         How to synchronize between the user 
    ///     
    /// </summary>
    class Admin
    {
        private const String MYSQL_DB_DSN = "wtp_data";
        private const String MYSQL_DB_UID = "wtpadmin";
        private const String MYSQL_DB_PWD = "Mh&g@1U";
        private const String SQLITE_DB_LOCATION = "O:/wtp_collab.db";

        private SQLiteConnection _sqliteConn;
        private OdbcConnection _odbcConn;
        private OdbcTransaction _transaction;
        //TODO: Read in DataTable from sql and generate the create statement
        public DataTableQueryMaker queryMaker { get; }
        public SQLiteDataAdapter _dataAdapter;
        public DataSet _workingSet;
        private UserTableTracker _userTableTracker;


        public Admin()
        {
            queryMaker = new DataTableQueryMaker();
            _sqliteConn = new SQLiteConnection(String.Format("Data source = {0}", SQLITE_DB_LOCATION));
            _userTableTracker = new UserTableTracker(_sqliteConn);
        }
        

        /* First we should get the tables that we need to import back to main db
         * Second run inserting
         * Third update user table tracker
         * Fourth update user table tracker in main_db (by importing the user table tracker.
         * 
         */
        public void dumpTablesFromSQLite()
        {
            String[] userTableMissing = userTablesMissingInMainDB();

            // Load those tables into the dataAdapter
            _dataAdapter = new SQLiteDataAdapter();
            _workingSet = new DataSet();

            foreach (var item in userTableMissing)
                Console.WriteLine(item);

            // Process each table
            String[] successDumpDataTable = userTableMissing.Where(dumpOneTable).ToArray();
            String[] failDumpDataTable = userTableMissing.Except(successDumpDataTable).ToArray();

            // Ask Tracker to update the tracker
            _userTableTracker.updateIsInWTPDataForTablerows(true, successDumpDataTable);

            // Also update the tracker in the wtp_data // Just drop the whole table and dump the tracker to it
            updateTrackerInWTPDATA();
        }

   
        public bool dumpOneTable(String tableName)
        {
            try
            {
                _odbcConn = new OdbcConnection(String.Format("DSN={0};UID={1};PWD={2}", MYSQL_DB_DSN, MYSQL_DB_UID, MYSQL_DB_PWD));
                _dataAdapter = new SQLiteDataAdapter();
                _workingSet = new DataSet();
                DataTable dt = GetDataTableFromSQLite();

                var createStmt = createTableStatementFromTable(dt);
                Debug.WriteLine(createStmt);

                OdbcCommand cmd = new OdbcCommand(createStmt, _odbcConn);
                _odbcConn.Open();
                cmd.ExecuteNonQuery();

                queryMaker.setEnclosingString("\'");
                insertDataTableRow(dt, queryMaker);
                _odbcConn.Close();
                return true;
            } catch (Exception e)
            {
                Debug.WriteLine(e.Message, e.StackTrace);
                return false;
            }

            DataTable GetDataTableFromSQLite(){
                _dataAdapter.SelectCommand = new SQLiteCommand($"SELECT * from {tableName};", _sqliteConn);
                _dataAdapter.Fill(_workingSet, tableName);
                return _workingSet.Tables[tableName];
            }
            
        }

        public String createTableStatementFromTable(DataTable dt)
        {
            queryMaker.Table = dt;
            // I set no primary key for user table
            queryMaker.Keys = new string[] { };
            return queryMaker.makeCreateStatementFor(dt.TableName);
        }

        private void insertDataTableRow(DataTable dt, DataTableQueryMaker maker)
        {
            _transaction = _odbcConn.BeginTransaction();
            // Improve performance
            int rowNum = 0;
            foreach (DataRow row in dt.Rows)
            {
                Debug.WriteLine(rowNum++);

                try
                {
                    executeSql(maker.getInsertStatmentForEachRow(row));
                }
                // TODO: ADD MORE ERROR HANDLING.
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    continue;
                }
            }
            _transaction.Commit();

        }

        protected void executeSql(String sqlQuery)
        {
            OdbcCommand odbcCmd = new OdbcCommand(sqlQuery, _odbcConn, (OdbcTransaction)_transaction);
            odbcCmd.ExecuteNonQuery();
        }

        public const string TRACKER_TABLE_NAME = "USER_TABLE_TRACKER";

        // Try to read the user tables that are not in the main database, that is : isInWTPData is 0.
        public String[] userTablesMissingInMainDB()
        {
            DataTable trackerTable = _userTableTracker.TrackerTable;

            IEnumerable<DataRow> missingTableRow = trackerTable.Rows.Cast<DataRow>()
                                                        .Where(record => record["isInWTPData"].Equals(0));

            return missingTableRow.Select(row => row["TableName"].ToString()).ToArray<String>();
        }

        public bool updateTrackerInWTPDATA()
        {
            // Actually I need to check whether tracker is in the wtp_data

            // Drop tracker table
            var command = new OdbcCommand($"DROP TABLE {TRACKER_TABLE_NAME}", _odbcConn);
            try
            {
                command.ExecuteNonQuery();
            } catch (Exception e)
            {
                Debug.WriteLine(command);
            }
            // Just have a copy of tracker table in database 
            return dumpOneTable(TRACKER_TABLE_NAME);
        }
    }
}
