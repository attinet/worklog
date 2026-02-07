using System;

// 使用第三方的在線工具或本地生成
var password = \"Admin@123\";
var hash = BCrypt.Net.BCrypt.HashPassword(password, 11);
Console.WriteLine(\Generated hash: {hash}\);
Console.WriteLine(\Verify test: {BCrypt.Net.BCrypt.Verify(password, hash)}\);
