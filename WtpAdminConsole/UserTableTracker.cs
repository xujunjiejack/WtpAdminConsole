using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using log4net.Appender;

namespace WtpAdminConsole
{
    class UserTableTracker
    {
        private FileAppender fileAppender = new FileAppender();
        public const string TRACKER_TABLE_NAME = "USER_TABLE_TRACKER";
        private SQLiteConnection _sqliteConn;
        //private ConnectionCoordinater _cc;
        private SQLiteDataAdapter _dataadapter;
        private DataSet _ds;
        private List<String> _userTableNames;

        public DataTable TrackerTable { get => _ds.Tables[TRACKER_TABLE_NAME]; }

        public List<String> UserTableNames
        {
            get
            {
                _userTableNames = _userTableNames ?? getUserTableNames();
                return _userTableNames;
            }
        }

        public bool tablenameIsDuplicate(String candidate) => UserTableNames.Contains(candidate);
      

        private List<String> getUserTableNames()
        {
            // get the memory representation of the tracker
            SQLiteDataAdapter dataadapter = new SQLiteDataAdapter(String.Format("SELECT * FROM {0}", TRACKER_TABLE_NAME), _sqliteConn);
            DataSet ds = new DataSet();
            dataadapter.Fill(ds, TRACKER_TABLE_NAME);
            DataTable trackerTable = new DataTable();
            trackerTable = ds.Tables[TRACKER_TABLE_NAME];
            List<String> userTableNames = trackerTable.Rows.Cast<DataRow>()
                                            .Select(row => (String)row["TableName"]).ToList<String>(); 
            return userTableNames;
        }

        public String[] getAllDataTables()
        {
            _sqliteConn.Open();
            DataTable dt = _sqliteConn.GetSchema("TABLES", new String[] { null, null });
            _sqliteConn.Close();
            String[] allTableNames = dt.Rows.Cast<DataRow>().Select(row => row["TABLE_NAME"].ToString()).ToArray();
            return allTableNames;
        }

        public String[] getMissingUserTableInDB()
        {
            List<String> userTableNames = UserTableNames;
            String[] allTables = getAllDataTables();
            String[] missingUserTablesInDB = userTableNames.Except(userTableNames.Intersect(allTables)).ToArray();
            return missingUserTablesInDB;
        }

        public void updateUserTableNames() => _userTableNames = getUserTableNames();

        //public UserTableTracker(ConnectionCoordinater cc) => _cc = cc;
        public UserTableTracker(SQLiteConnection sqliteConn)
        {
            _sqliteConn = sqliteConn;
            _dataadapter = new SQLiteDataAdapter(String.Format("SELECT * FROM {0}", TRACKER_TABLE_NAME), _sqliteConn);
            _ds = new DataSet();
            _dataadapter.Fill(_ds, TRACKER_TABLE_NAME);
        }

        public void updateIsInWTPDataForTablerows(Boolean status, String[] tableNames)
        {
            var rowsNeedUpdated = TrackerTable.Rows.Cast<DataRow>().TakeWhile(row => tableNames.Contains(row["TableName"]));
            rowsNeedUpdated.ToList().ForEach(row => row["isInWTPData"] = (status ? 1 : 0) );

            SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(_dataadapter);
            cmdBuilder.GetUpdateCommand();
            // use data adapter to update the sqlite
            _dataadapter.Update(_ds, TRACKER_TABLE_NAME);
        }

        /// <summary>
        /// This method creates a corresponding record in the USER_TABLE_TRACKER. The new record includes the creation date of the table, its
        /// tablename, its creator and the machine that his creator for generating this table. 
        /// 
        /// </summary>
        /// <param name="newDataTable">
        ///     The new table that has been built by DataFileBuilder based on the user's lists of tables.
        /// </param>
        public void addDataTable(DataTable newDataTable)
        {
            SQLiteDataAdapter dataadapter = new SQLiteDataAdapter(String.Format("SELECT * FROM {0}", TRACKER_TABLE_NAME), _sqliteConn);
            DataSet ds = new DataSet();
            dataadapter.Fill(ds, TRACKER_TABLE_NAME);
            DataTable trackerTable = new DataTable();
            trackerTable = ds.Tables[TRACKER_TABLE_NAME];

            DataRow newDataTableRow = trackerTable.NewRow();
            newDataTableRow["TableName"] = newDataTable.TableName;
            newDataTableRow["CreationTime"] = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
            newDataTableRow["User"] = Environment.UserName;
            newDataTableRow["ComputerName"] = Environment.MachineName;
            newDataTableRow["isInWTPData"] = 0;
            trackerTable.Rows.Add(newDataTableRow);

            SQLiteCommandBuilder cmdBuilder = new SQLiteCommandBuilder(dataadapter);
            SQLiteCommand cmd = cmdBuilder.GetUpdateCommand();
            dataadapter.Update(ds, TRACKER_TABLE_NAME);
        }
    }
}
