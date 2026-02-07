#r "nuget: BCrypt.Net-Next, 4.0.3"

using BCrypt.Net;

var password = "Admin@123";
var storedHash = "$2a$11$8zK5N5rJc1GqvFjXh8yXPO9ZQXqM3gYxN3jB5lZ9vXhN1qW5qO5Mu";

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Stored Hash: {storedHash}");
Console.WriteLine($"Verification Result: {BCrypt.Verify(password, storedHash)}");
Console.WriteLine();

// 生成新的正確哈希
var correctHash = BCrypt.HashPassword(password);
Console.WriteLine($"Correct Hash: {correctHash}");
Console.WriteLine($"New Verification: {BCrypt.Verify(password, correctHash)}");
