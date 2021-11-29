using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//app.UseBlazorFrameworkFiles();
//app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();

app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/firstapp"), first =>
{
    first.Use((ctx, nxt) =>
    {
        ctx.Request.Path = "/firstapp" + ctx.Request.Path;
        return nxt();
    });
    first.UseBlazorFrameworkFiles("/firstapp");
    first.UseStaticFiles();
    first.UseStaticFiles("/firstapp");

    first.UseRouting();
    first.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapFallbackToFile("firstapp/{*path:nonfile}", "firstapp/index.html");
    });
});

app.MapGet("/firstapp/app.bundle", (HttpContext context) =>
{
    string? contentEncoding = null;
    var contentType = "multipart/form-data; boundary=\"--0a7e8441d64b4bf89086b85e59523b7d\"";
    var fileName = "app.bundle";

    var acceptEncodings = context.Request.Headers.AcceptEncoding;
    if (StringWithQualityHeaderValue.TryParseList(acceptEncodings, out var encodings))
    {
        if (encodings.Any(e => e.Value == "br"))
        {
            contentEncoding = "br";
            fileName += ".br";
        }
        else if (encodings.Any(e => e.Value == "gzip"))
        {
            contentEncoding = "gzip";
            fileName += ".gz";
        }
    }

    if (contentEncoding != null)
    {
        context.Response.Headers.ContentEncoding = contentEncoding;
    }
    return Results.File(
        app.Environment.WebRootFileProvider.GetFileInfo(Path.Combine("firstapp", fileName)).CreateReadStream(),
        contentType);
});
//app.MapFallbackToFile("index.html");

app.Run();
