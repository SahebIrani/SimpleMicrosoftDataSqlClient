
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
                        string commandText =
                            "SELECT TOP 8 Id, FullName, Age "
                            + "FROM db.People WHERE Id < @Age";

                        connection.Open();

                        using SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandTimeout = 15;
                        command.CommandType = CommandType.Text;
                        command.CommandText = commandText;


                    }

                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
