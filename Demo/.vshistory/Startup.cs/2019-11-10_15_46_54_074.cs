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

                        using SqlCommand command = new SqlCommand(
                            queryString,
                            connection);
                        SqlParameter parameter = new SqlParameter("@Age", SqlDbType.Int)
                        {
                            Value = 1.5
                        };
                        command.Parameters.Add(parameter);


                    }


                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
