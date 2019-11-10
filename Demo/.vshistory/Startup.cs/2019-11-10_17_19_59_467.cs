
using System;
using System.Collections.Generic;
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

                        SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
                        command.Transaction = transaction;

                        SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);

                        using SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString)
                        {
                            DestinationTableName = "dbo.AppDbContext",
                            BatchSize = 50
                        };
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
                            transaction.Commit();
                            reader.Close();
                        }

                        command.ExecuteScalar();

                        IList<dynamic> people = new List<dynamic>();

                        while (reader.Read())
                        {
                            object id = reader[0];
                            object fullName = reader[1];
                            object age = reader[2];

                            people.Add(new { Id = id, FullName = fullName, Age = age });
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
                    }
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
