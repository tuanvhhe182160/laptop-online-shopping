using System;
class Program {
    static void Main() {
        string hash = BCrypt.Net.BCrypt.HashPassword("123");
        Console.WriteLine(hash);
    }
}
