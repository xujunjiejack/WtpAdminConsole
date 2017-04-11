using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Collections;

namespace WtpAdminConsole
{
    class DataTableQueryMaker
    {
        public DataTable Table { get; set; }
        public String[] Keys { get; set; } // To store the primary keys for the table
        private String _stringEnclosing;

        public DataTableQueryMaker(): this(null, null)
        {
        }

        public DataTableQueryMaker(DataTable dt): this(dt, null)
        {
        }

        public DataTableQueryMaker(DataTable dt, String[] keys)
        {
            Table = dt;
            Keys = keys;
        }
        

        public String makeCreateStatementFor(String tableName){

            if (Keys.Length == 0)
            {
                return String.Format("CREATE TABLE IF NOT EXISTS `{0}` ({1});",
                                                tableName,
                                                createFieldsStatements());
            }

            else
            {
                return String.Format("CREATE TABLE IF NOT EXISTS `{0}` ({1}, {2});",
                                                tableName,
                                                createFieldsStatements(),
                                                getPrimaryKeyStmt());  // "CREATE TABLE tablename (Column stmt, primary key stmt);";
            }
        }

        private String createFieldsStatements()
        {  
            var colStmts = Table.Columns.Cast<DataColumn>().Select(getOneColStmt).ToArray<String>();
            return String.Join(", ", colStmts);
        }

        private String getOneColStmt(DataColumn col)
        {
            String fieldType = getColFieldType(col.DataType);
            String constraint = "";

            // Key can't be null
            if (Keys.Contains<String>(col.ColumnName))
            {
                constraint = "NOT NULL";
            }

            return String.Format("`{0}` {1} {2}", col.ColumnName, fieldType, constraint);
        }

        private String getColFieldType(Type dataType)
        {
            if (dataType.Equals(typeof(System.Int32)))
            {
                return "int(10)";
            }

            if (dataType.Equals(typeof(String)))
            {
                return "varchar(50)";
            }

            if (dataType.Equals(typeof(Double))){
                return "float";
            }

            throw new ArgumentException("We currently don't support this data type: " + dataType.ToString());
        }

        private String getPrimaryKeyStmt()
        {
            return String.Format("PRIMARY KEY ({0})",string.Join(",", Keys));
        }

        public String getInsertStatmentForEachRow(DataRow row)
        {
         
            String[] fieldNames = Table.Columns.Cast<DataColumn>()
                                        .Select(col => "" + col.ColumnName + "")
                                        .ToArray<String>();

            String[] fieldData = Table.Columns.Cast<DataColumn>()
                                        .Select(col => getDataInStringBasedOnDifferentType(row, col))
                                        .ToArray<String>();

            return String.Format("INSERT INTO `{0}` ({1}) VALUES ({2});", Table.TableName, 
                                                                        string.Join(",", fieldNames),
                                                                        string.Join(",", fieldData));

        }

        public void setEnclosingString(String delimiter)
        {
            _stringEnclosing = delimiter;
        }

        public String getDataInStringBasedOnDifferentType(DataRow row, DataColumn col){
            
            if (row[col.ColumnName].GetType().Equals(typeof(DBNull)))
            {
                return "NULL";
            }

            if (col.DataType.Equals(typeof(String)))
            {
                // TODO: Need data validation for cleaning double quote ""
                // Need to change the closing symbol for string.
                return String.Format(_stringEnclosing+ "{0}" + _stringEnclosing, row[col.ColumnName]);
            }

            if (col.DataType.Equals(typeof(Int32)) || col.DataType.Equals(typeof(double)))
            {
                return row[col.ColumnName].ToString();
            }

            throw new Exception(String.Format("The data can't be casted to String: Col: {0}, Data: {1}", col.ColumnName, row[col.ColumnName]));
        }
    }
}
