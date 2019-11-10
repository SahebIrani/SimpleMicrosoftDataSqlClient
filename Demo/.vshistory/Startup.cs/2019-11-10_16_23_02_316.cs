
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
                            AsyncCallback callback = new AsyncCallback(HandleCallback);
                            command.BeginExecuteNonQuery(callback, command);
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
