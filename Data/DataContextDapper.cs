using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace UserManagement.Data
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


    // ===============================================
    // Executes a SQL SELECT that returns multiple rows
    // ===============================================
    public IEnumerable<T> LoadData<T>(string sql)
    {
      // Establish connection using the connection string
      IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

      // Use Dapper to execute the query and map results to type T
      return dbConnection.Query<T>(sql);
    }


    // ===============================================
    // Executes a SQL SELECT with parameters and returns a single row
    // ===============================================
    public T LoadDataSingleWithParameters<T>(string sql, object parameters)
    {
      using (SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
      {
        // Query for a single object or null if no match found
        return dbConnection.QuerySingleOrDefault<T>(sql, parameters);
      }
    }


    // ===============================================
    // Executes a SQL SELECT with parameters and returns multiple rows
    // ===============================================
    public IEnumerable<T> LoadDataWithParameters<T>(string sql, object parameters)
    {
      using (SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
      {
        // Use Dapper to execute the query and return a list of T
        return dbConnection.Query<T>(sql, parameters);
      }
    }


    // ======================================================
    // Executes a SQL command (like DELETE) with a single param object
    // Returns the number of rows affected
    // ======================================================
    public int ExecuteSqlWithSingleParameter(string sql, object parameters)
    {
      using (SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
      {
        return dbConnection.Execute(sql, parameters);
      }
    }


    // ======================================================
    // Executes SQL (INSERT, UPDATE, DELETE) using SqlParameter list
    // Safer than string interpolation—prevents SQL injection
    // ======================================================
    public bool ExecuteSqlWithParameters(string sql, List<SqlParameter> parameters)
    {
      using var dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      using var command = new SqlCommand(sql, dbConnection);
      // Add parameters safely
      command.Parameters.AddRange(parameters.ToArray());
      dbConnection.Open();
      int rowsAffected = command.ExecuteNonQuery();
      return rowsAffected > 0;
    }
  }
}

