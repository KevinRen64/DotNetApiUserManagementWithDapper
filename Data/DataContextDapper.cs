using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotNetApi.Data
{
  
  // Dapper-based data context for executing SQL queries and commands
  class DataContextDapper
  {

    // Configuration object used to access the connection string
    private readonly IConfiguration _config;

    // Constructor initializes the configuration
    public DataContextDapper(IConfiguration config)
    {
      _config = config;
    }


    // Generic method to load multiple rows of data from the database
    public IEnumerable<T> LoadData<T>(string sql)
    {
      // Create a new SQL connection using the connection string
      IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

      // Execute the SQL query and return the result as IEnumerable<T>
      return dbConnection.Query<T>(sql);
    }


    // Generic method to load a single row of data from the database
    public T LoadDataSingle<T>(string sql)
    {
      IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

      // Execute the SQL query and return a single result
      return dbConnection.QuerySingle<T>(sql);
    }


    // Executes a SQL command that does not return data (e.g., INSERT, UPDATE, DELETE)
    // Returns true if at least one row was affected
    public bool ExecuteSql(string sql)
    {
      IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

      // Execute the command and check if rows were affected
      return dbConnection.Execute(sql) > 0;
    }


    // Executes a SQL command and returns the number of rows affected
    public int ExecuteSqlWithRowCount(string sql)
    {
      IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      
      // Execute the command and return the number of affected rows
      return dbConnection.Execute(sql);
    }

  }
}

