
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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



                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
