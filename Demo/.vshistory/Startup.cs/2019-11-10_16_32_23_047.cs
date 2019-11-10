
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
                        @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AppDbContext;Integrated Security=SSPI;";

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

                        command.ExecuteScalar();

                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                        }

                        SqlCommand command1 = connection.CreateCommand();
                        command1.CommandText = commandText;
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

                        SqlTransaction transaction1 = connection.BeginTransaction(IsolationLevel.Serializable);
                        command1.Transaction = transaction1;
                        SqlDataReader reader2 = command1.ExecuteReader(CommandBehavior.CloseConnection);
                        transaction1.Commit();

                        SqlDataReader reader = command.ExecuteReader();
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
                        {
                            bulkCopy.DestinationTableName = "dbo.AppDbContext";

                            bulkCopy.BatchSize = 50;

                            try
                            {
                                bulkCopy.WriteToServer(reader);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            finally
                            {
                                reader.Close();
                            }
                        }
                    }

                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
