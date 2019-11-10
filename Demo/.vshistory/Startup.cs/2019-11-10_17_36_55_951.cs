
using System;
using System.Collections.Generic;
using System.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;

using Newtonsoft.Json;

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
                            "SELECT TOP 17 Id, FullName, Age FROM People WHERE Age < @Age";

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
                            DestinationTableName = "dbo.People",
                            BatchSize = 50
                        };

                        IList<object> people = new List<object>();

                        try
                        {
                            while (reader.Read())
                            {
                                object id = reader[0];
                                object fullName = reader[1];
                                object age = reader[2];
                                people.Add(new { Id = id, FullName = fullName, Age = age });
                            }

                            //command.ExecuteScalar();
                            //bulkCopy.WriteToServer(reader);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        finally
                        {
                            //transaction.Commit();
                            //reader.Close();
                        }

                        SqlCommand command1 = connection.CreateCommand();
                        command1.CommandText = "IF EXISTS (SELECT * FROM People WHERE FullName=N'SinjulMSBH') DROP TABLE People";
                        try
                        {
                            //command1.ExecuteNonQuery();
                            //command1.BeginExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        string result = JsonConvert.SerializeObject(people);
                        IEnumerable<People> peopleResult = JsonConvert.DeserializeObject<List<People>>(result);

                        await context.Response.WriteAsync(result);
                    }
                });
            });
        }
    }
}
