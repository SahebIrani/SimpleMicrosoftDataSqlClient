using System;
using System.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    const string connectionString =
                       @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AppDbContext;Integrated Security=SSPI;";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string queryString =
                            "SELECT TOP 8 Id, FullName, Age "
                            + "FROM AppDbContext.People WHERE Id < @Age";

                        connection.Open();

                        using SqlCommand command = new SqlCommand(queryString, connection);
                        command.CommandTimeout = 15;
                        command.CommandType = CommandType.Text;

                        SqlCommand command1 = connection.CreateCommand();
                        command1.CommandText = "IF EXISTS "
                            + "(SELECT * FROM sys.tables WHERE name=N'TestSnapshot') "
                            + "DROP TABLE TestSnapshot"
                        ;
                        try
                        {
                            command1.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        SqlTransaction transaction1 = connection.BeginTransaction(IsolationLevel.Serializable);
                        command1.Transaction = transaction1;
                        command1.CommandText = "UPDATE TestSnapshot SET valueCol=22 WHERE ID=1";
                        command1.ExecuteNonQuery();
                        SqlDataReader reader2 = command1.ExecuteReader();
                        transaction1.Commit();

                        SqlDataReader reader = command1.ExecuteReader();
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


                        SqlTransaction transaction2 = connection.BeginTransaction(IsolationLevel.Snapshot);

                        SqlParameter parameter = new SqlParameter("@Age", SqlDbType.Int)
                        {
                            Value = 1.5,
                        };
                        command.Parameters.Add(parameter);


                        SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                        table.Load(reader);

                        reader.Close();
                    }


                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
