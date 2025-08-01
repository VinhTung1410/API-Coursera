using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// In-memory list to simulate a database
var users = new List<User>();

// ===== MIDDLEWARE =====

// Logging middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.Now}] Request: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"[{DateTime.Now}] Response: {context.Response.StatusCode}");
});

// Stub Authentication middleware (can expand later)
app.Use(async (context, next) =>
{
    // Just a stub, not real auth
    var isAuthenticated = true;

    if (!isAuthenticated)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
    }
    else
    {
        await next();
    }
});


// ===== ENDPOINTS =====

// Create a user
app.MapPost("/users", (User user) =>
{
    var validationResult = Validator.ValidateUser(user, isNew: true, users);
    if (validationResult is not null)
        return Results.BadRequest(validationResult);

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
    var userIndex = users.FindIndex(u => u.Username == username);
    if (userIndex == -1)
        return Results.NotFound();

    var validationResult = Validator.ValidateUser(updatedUser, isNew: false, users);
    if (validationResult is not null)
        return Results.BadRequest(validationResult);

    users[userIndex] = updatedUser with { Username = username };
    return Results.Ok(users[userIndex]);
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

// ===== MODEL & VALIDATION =====

record User(string Username, int Userage);

static class Validator
{
    public static string? ValidateUser(User user, bool isNew, List<User> users)
    {
        return string.IsNullOrWhiteSpace(user.Username)
            ? "Username is required."
            : user.Userage is < 0 or > 120
            ? "Userage must be between 0 and 120."
            : isNew && users.Any(u => u.Username == user.Username) ? "Username already exists." : null;
    }
}
