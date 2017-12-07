using System;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ShippingService
{
  class Program
  {

    SqlConnection sqlDatabaseConnection = null;
    Functions functions = new Functions();

    static void Main(string[] args)
    {
      if (args.Length >= 1)
        (new Program()).ExecuteAmsAci(args[0]);
      else
        Console.WriteLine("Exit");
    }

    public void ExecuteAmsAci(String sqlConnectionString)
    {
      // Open Database Connection
      try
      {
        // Create Database connection
        sqlDatabaseConnection = new SqlConnection(sqlConnectionString);
        sqlDatabaseConnection.Open();
      }
      catch (Exception e)
      {
        Console.WriteLine("{0:dd/MM/yyyy HH:mm:ss.fff} : {1}", DateTime.Now, e.Message);
        return;
      }

      // Gets all the records that are not closed and are not in error table
      String sqlString = String.Format("SELECT * FROM SHS_HBL AS S WHERE S.[FL_CHIUSO] = 0" +
                                       "AND S.[HBL_CHIAVE] NOT IN(SELECT [ERR_HBLISF_CHIAVE] FROM ERRORI)");
      DataRow[] dataRow = RetrieveDataRowEntries(sqlString);
      if (dataRow != null)
      {
        for (int i = 0; i < dataRow.Length; i++)
        {
          int count = CheckRow(dataRow[i]);
          if (count == 0)
          {
            // If check passed, insert in db and change FL_CHIUSO
          }
          else
          {
            Console.WriteLine("{0} : found {1} errors.", dataRow[i]["HBL_CHIAVE"], count);
          }
        }
      }

      // Close Database Connection
      sqlDatabaseConnection.Close();
    }

    // Gets an array of all DataRow objects of the table
    public DataRow[] RetrieveDataRowEntries(String sSqlString)
    {
      //
      try
      {
        SqlDataAdapter da = new SqlDataAdapter(sSqlString, sqlDatabaseConnection);
        DataSet ds = new DataSet(); da.Fill(ds); DataTable dt = ds.Tables[0];
        return dt.Select();
      }
      catch (Exception e)
      {
        Console.WriteLine("{0:dd/MM/yyyy HH:mm:ss.fff} : {1}", DateTime.Now, e.Message);
        return null;
      }
    }

    public int CheckRow(DataRow dataRow)
    {
      int errorsCount = 0;
      // Check
      for (int i = 0; i < dataRow.Table.Columns.Count; i++)
      {
        Error e = null;
        switch (dataRow.Table.Columns[i].ColumnName)
        {
          case "SHI_State":
          case "CON_State":
            e = functions.CheckSHI_State((String)dataRow[dataRow.Table.Columns[i].ColumnName]); 
            break;
          case "SHI_Country":
          case "CON_Country":
            break;
          default:
            break;
        }
          
        if (e != null)
        {
          InsertNewError((String)dataRow["HBL_CHIAVE"], e.getCode(), e.getMessage());
          errorsCount++;
        }
      }
      return errorsCount;
    }

    public void InsertNewError(String hblId, int errorCode, String errorMessage)
    {
      DateTime now = DateTime.Now;
      String date = String.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now);
      DataRow newErrorRow = CreateDataRowEntry("[ERRORI]");
      //
      newErrorRow.BeginEdit();

      newErrorRow["ERR_HBLISF_CHIAVE"] = hblId;
      newErrorRow["ERR_TIPO"] = "AMSAC";
      newErrorRow["ERR_DATA"] = date;
      newErrorRow["ERR_CODE"] = errorCode;
      newErrorRow["ERR_MESSAGE"] = errorMessage;
      newErrorRow["ERR_DATA_INVIO"] = date;

      //
      newErrorRow.EndEdit();
      //
      InsertDataRowEntry("[ERRORI]", newErrorRow);
    }
    
    public DataRow CreateDataRowEntry(String tableName)
    {
      try
      {
        SqlDataAdapter da = new SqlDataAdapter(String.Format("SELECT * FROM {0}", tableName), sqlDatabaseConnection);
        DataSet ds = new DataSet(); da.FillSchema(ds, SchemaType.Source, tableName);
        return ds.Tables[tableName].NewRow();
      }
      catch (Exception e)
      {
        Console.WriteLine("{0:dd/MM/yyyy HH:mm:ss.fff} : {1}", DateTime.Now, e.Message);
        return null;
      }
    }

    public Boolean InsertDataRowEntry(String tableName, DataRow dataRow)
    {
      try
      {
        SqlDataAdapter da = new SqlDataAdapter(String.Format("SELECT * FROM {0}", tableName), sqlDatabaseConnection);
        da.SelectCommand.CommandTimeout = 120;
        SqlCommandBuilder cb = new SqlCommandBuilder(da);
        da.InsertCommand = cb.GetInsertCommand();
        dataRow.Table.Rows.Add(dataRow);
        da.Update(dataRow.Table.DataSet, tableName);
        dataRow.Table.DataSet.AcceptChanges();
        return true;
      }
      catch (Exception e)
      {
        Console.WriteLine("{0:dd/MM/yyyy HH:mm:ss.fff} : {1}", DateTime.Now, e.Message);
        return false;
      }
    }

    //// Gets the first DataRow of the table
    //public DataRow RetrieveDataRowEntry(String sSqlString)
    //{
    //  //
    //  try
    //  {
    //    SqlDataAdapter da = new SqlDataAdapter(sSqlString, sqlDatabaseConnection);
    //    DataSet ds = new DataSet(); da.Fill(ds); DataTable dt = ds.Tables[0];
    //    if (dt.Rows.Count == 0)
    //      return null;

    //    return dt.Rows[0];
    //  }
    //  catch (Exception e)
    //  {
    //    Console.WriteLine("{0:dd/MM/yyyy HH:mm:ss.fff} : {1}", DateTime.Now, e.Message);
    //    return null;
    //  }
    //}
  }
}
