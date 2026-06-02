using System;
using System.Linq;
using WebAPI.Entities;

namespace WebAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Seed Roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Staff" }
                );
                context.SaveChanges();
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { CategoryName = "ASUS", Description = "Laptop ASUS" },
                    new Category { CategoryName = "Dell", Description = "Laptop Dell" },
                    new Category { CategoryName = "HP", Description = "Laptop HP" },
                    new Category { CategoryName = "Lenovo", Description = "Laptop Lenovo" },
                    new Category { CategoryName = "MacBook", Description = "Apple MacBook" }
                );
                context.SaveChanges();
            }

            // Seed Users (Admin & Staff)
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                var staffRole = context.Roles.FirstOrDefault(r => r.RoleName == "Staff");

                if (adminRole != null && staffRole != null)
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword("123");

                    context.Users.AddRange(
                        new User
                        {
                            Username = "admin",
                            PasswordHash = hashedPassword,
                            FullName = "Admin Quản Trị",
                            Email = "admin@laptopshop.com",
                            IsActive = true,
                            RoleId = adminRole.RoleId
                        },
                        new User
                        {
                            Username = "staff",
                            PasswordHash = hashedPassword,
                            FullName = "Nhân viên bán hàng",
                            Email = "staff@laptopshop.com",
                            IsActive = true,
                            RoleId = staffRole.RoleId
                        }
                    );
                    context.SaveChanges();
                }
            }

            // Seed Laptops
            if (!context.Laptops.Any())
            {
                var asus = context.Categories.FirstOrDefault(c => c.CategoryName == "ASUS");
                var dell = context.Categories.FirstOrDefault(c => c.CategoryName == "Dell");
                var hp = context.Categories.FirstOrDefault(c => c.CategoryName == "HP");
                var lenovo = context.Categories.FirstOrDefault(c => c.CategoryName == "Lenovo");
                var macbook = context.Categories.FirstOrDefault(c => c.CategoryName == "MacBook");

                if (asus != null && dell != null && hp != null && lenovo != null && macbook != null)
                {
                    context.Laptops.AddRange(
                        new Laptop
                        {
                            LaptopCode = "ASUS-ROG",
                            LaptopName = "Asus ROG Strix G15",
                            Price = 28500000,
                            StockQuantity = 10,
                            CategoryId = asus.CategoryId,
                            Status = true,
                            CreatedDate = DateTime.Now
                        },
                        new Laptop
                        {
                            LaptopCode = "DELL-XPS15",
                            LaptopName = "Dell XPS 15 9520",
                            Price = 42000000,
                            StockQuantity = 3,
                            CategoryId = dell.CategoryId,
                            Status = true,
                            CreatedDate = DateTime.Now
                        },
                        new Laptop
                        {
                            LaptopCode = "HP-ENVY13",
                            LaptopName = "HP Envy 13-ba1030TU",
                            Price = 21500000,
                            StockQuantity = 15,
                            CategoryId = hp.CategoryId,
                            Status = true,
                            CreatedDate = DateTime.Now
                        },
                        new Laptop
                        {
                            LaptopCode = "LNV-LEGION",
                            LaptopName = "Lenovo Legion 5 Pro",
                            Price = 32000000,
                            StockQuantity = 7,
                            CategoryId = lenovo.CategoryId,
                            Status = true,
                            CreatedDate = DateTime.Now
                        },
                        new Laptop
                        {
                            LaptopCode = "MAC-AIRM2",
                            LaptopName = "MacBook Air M2 2022",
                            Price = 26900000,
                            StockQuantity = 12,
                            CategoryId = macbook.CategoryId,
                            Status = true,
                            CreatedDate = DateTime.Now
                        },
                        new Laptop
                        {
                            LaptopCode = "DELL-INS15",
                            LaptopName = "Dell Inspiron 15 3520",
                            Price = 13500000,
                            StockQuantity = 2,
                            CategoryId = dell.CategoryId,
                            Status = false, // Ngừng bán
                            CreatedDate = DateTime.Now
                        }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}
