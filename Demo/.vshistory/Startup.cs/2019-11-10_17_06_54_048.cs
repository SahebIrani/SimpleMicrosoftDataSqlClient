
using System;
using System.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;

namespace Demo
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    const string connectionString =
                        @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AppDbContext;Integrated Security=SSPI; MultipleActiveResultSets=true";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        const string commandText =
                            "SELECT TOP 8 Id, FullName, Age FROM People WHERE Id < @Age";

                        connection.Open();

                        using SqlCommand command = new SqlCommand
                        {
                            Connection = connection,
                            CommandText = commandText,
                            CommandTimeout = 15,
                            CommandType = CommandType.Text,
                        };

                        SqlParameter parameter = new SqlParameter("@Age", SqlDbType.Int)
                        {
                            Value = 28,
                        };
                        command.Parameters.Add(parameter);

                        command.ExecuteScalar();

                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            object r0 = reader[0];
                            object r1 = reader[1];
                            object r2 = reader[2];
                        }

                        reader.Close();

                        SqlCommand command1 = connection.CreateCommand();
                        command1.CommandText = "IF EXISTS (SELECT * FROM People WHERE FullName=N'SinjulMSBH') DROP TABLE People";
                        try
                        {
                            command1.ExecuteNonQuery();
                            command1.BeginExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
                        command1.Transaction = transaction;
                        SqlDataReader reader2 = command1.ExecuteReader(CommandBehavior.CloseConnection);
                        transaction.Commit();

                        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString);
                        bulkCopy.DestinationTableName = "dbo.AppDbContext";
                        bulkCopy.BatchSize = 50;
                        try
                        {
                            bulkCopy.WriteToServer(reader1);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        finally
                        {
                            reader1.Close();
                        }
                    }
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
