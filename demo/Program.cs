using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();



// In-memory list to simulate a database
var users = new List<User>();

// Create a user
app.MapPost("/users", (User user) =>
{
    users.Add(user);
    return Results.Created($"/users/{user.Username}", user);
});

// Read all users
app.MapGet("/users", () =>
{
    return Results.Ok(users);
});

// Read one user by username
app.MapGet("/users/{username}", (string username) =>
{
    var user = users.FirstOrDefault(u => u.Username == username);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

// Update user
app.MapPut("/users/{username}", (string username, User updatedUser) =>
{
    var user = users.FirstOrDefault(u => u.Username == username);
    if (user is null)
        return Results.NotFound();

    user.Userage = updatedUser.Userage;
    return Results.Ok(user);
});

// Delete user
app.MapDelete("/users/{username}", (string username) =>
{
    var user = users.FirstOrDefault(u => u.Username == username);
    if (user is null)
        return Results.NotFound();

    users.Remove(user);
    return Results.NoContent();
});

app.Run();

record User
{
    public string Username { get; set; }
    public int Userage { get; set; }
}
