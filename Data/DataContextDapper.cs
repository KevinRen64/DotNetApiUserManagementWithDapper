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

public T LoadDataSingleWithParameters<T>(string sql, object parameters)
{
    using (SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
    {
        return dbConnection.QuerySingle<T>(sql, parameters);
    }
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
    
    // Executes a SQL command with parameters
    public bool ExecuteSqlWithParameters(string sql, List<SqlParameter> parameters)
    {
      // Create a new SQL command using the raw SQL query passed in
      SqlCommand commandWithParams = new SqlCommand(sql);

      // Add each parameter from the provided list to the command to safely pass values
      foreach (SqlParameter parameter in parameters)
      {
        commandWithParams.Parameters.Add(parameter);  // Prevents SQL injection
      }

      // Create a new SQL connection using the connection string from configuration
      SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      dbConnection.Open();  // Open the database connection

      // Assign the open connection to the SQL command
      commandWithParams.Connection = dbConnection;

      // Execute the SQL command (e.g., INSERT, UPDATE, DELETE), and get the number of rows affected
      int rowsAffected = commandWithParams.ExecuteNonQuery();

      // Close the connection after execution
      dbConnection.Close();

      // Return true if at least one row was affected, otherwise false
      return rowsAffected > 0;
    }

  }
}

