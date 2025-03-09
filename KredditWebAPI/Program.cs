using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;

using Data;
using Service;
using shared.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Saetter CORS saa API'en kan bruges fra andre domaener
var AllowSomeStuff = "_AllowSomeStuff";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSomeStuff, builder => {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Tilfoj DbContext factory som service.
builder.Services.AddDbContext<PostContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ContextSQLite")));

// Tilfoj DataService saa den kan bruges i endpoints
builder.Services.AddScoped<DataService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
    dataService.SeedData(); // Fylder data paa, hvis databasen er tom. Ellers ikke.
}

app.UseHttpsRedirection();
app.UseCors(AllowSomeStuff);

// Middlware der korer for hver request. Saetter ContentType for alle responses til "JSON".
app.Use(async (context, next) =>
{
    context.Response.ContentType = "application/json; charset=utf-8";
    await next(context);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// DataService faas via "Dependency Injection" (DI)
app.MapGet("/", (DataService service) =>
{
    return new { message = "Hello World!" };
});

app.MapGet("/posts", (DataService service) =>
{
    return service.GetPosts();
});

app.MapGet("/posts/{id}", (DataService service, int id) => {
    return service.GetPost(id);
});

app.MapPost("/posts/{postId}/comments", (DataService service, int postId, CommentData data) =>
{
    Comment newComment = service.CreateComment(data.Content, postId, data.UserId);
    return newComment;
});

app.MapPost("/posts", (DataService service, PostData data) =>
{
    Post newPost = service.CreatePost(data.Title, data.Content, data.UserId);
    return newPost;
});

app.Run();

public class CommentData
{
    public string? Content { get; set; }
    public int UserId { get; set; }
}

public class PostData
{
    public string Title { get; set; }
    public string Content { get; set; }
    public int UserId { get; set; }
}

//app.UseAuthorization();

//app.MapControllers();


