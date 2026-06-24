using System;
using System.Linq;
using WebAPI.Entities;

namespace WebAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // 1. Seed Chi nhánh (Branches) trước tiên vì User và PhysicalProduct cần nó
            if (!context.Branches.Any())
            {
                context.Branches.AddRange(
                    new Branch { BranchName = "Chi nhánh Hà Nội", Address = "Tôn Thất Thuyết, Cầu Giấy, HN" },
                    new Branch { BranchName = "Chi nhánh TP.HCM", Address = "Quận 1, TP.HCM" }
                );
                context.SaveChanges();
            }

            // 2. Seed Roles (Thêm Role quản lý kho)
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Staff" },
                    new Role { RoleName = "WarehouseManager" }
                );
                context.SaveChanges();
            }

            // 3. Seed Users (Gắn BranchId cho Staff và Manager)
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                var staffRole = context.Roles.FirstOrDefault(r => r.RoleName == "Staff");
                var wmRole = context.Roles.FirstOrDefault(r => r.RoleName == "WarehouseManager");

                var branchHN = context.Branches.FirstOrDefault(b => b.BranchName == "Chi nhánh Hà Nội");

                if (adminRole != null && staffRole != null && wmRole != null && branchHN != null)
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
                            RoleId = adminRole.RoleId,
                            BranchId = null // Admin tổng không thuộc chi nhánh nào
                        },
                        new User
                        {
                            Username = "staff",
                            PasswordHash = hashedPassword,
                            FullName = "Nhân viên bán hàng",
                            Email = "staff@laptopshop.com",
                            IsActive = true,
                            RoleId = staffRole.RoleId,
                            BranchId = branchHN.BranchId // Thuộc chi nhánh HN
                        },
                        new User
                        {
                            Username = "manager",
                            PasswordHash = hashedPassword,
                            FullName = "Quản lý kho",
                            Email = "manager@laptopshop.com",
                            IsActive = true,
                            RoleId = wmRole.RoleId,
                            BranchId = branchHN.BranchId // Thuộc chi nhánh HN
                        }
                    );
                    context.SaveChanges();
                }
            }

            // 4. Seed Categories
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

            // 5. Seed Products, Variants và PhysicalProducts (Tái cấu trúc từ Laptops cũ)
            if (!context.Products.Any())
            {
                var asus = context.Categories.FirstOrDefault(c => c.CategoryName == "ASUS");
                var dell = context.Categories.FirstOrDefault(c => c.CategoryName == "Dell");
                var macbook = context.Categories.FirstOrDefault(c => c.CategoryName == "MacBook");

                if (asus != null && dell != null && macbook != null)
                {
                    // A. Tạo Mẫu sản phẩm chung (Products)
                    var p1 = new Product { ProductCode = "ASUS-ROG", ProductName = "Asus ROG Strix G15", CategoryId = asus.CategoryId, Status = true, CreatedDate = DateTime.Now };
                    var p2 = new Product { ProductCode = "DELL-XPS15", ProductName = "Dell XPS 15 9520", CategoryId = dell.CategoryId, Status = true, CreatedDate = DateTime.Now };
                    var p3 = new Product { ProductCode = "MAC-AIRM2", ProductName = "MacBook Air M2", CategoryId = macbook.CategoryId, Status = true, CreatedDate = DateTime.Now };

                    context.Products.AddRange(p1, p2, p3);
                    context.SaveChanges();

                    // B. Tạo Cấu hình chi tiết và Giá (ProductVariants)
                    var v1 = new ProductVariant { ProductId = p1.ProductId, Cpu = "Core i7", Ram = "16GB", Ssd = "512GB", Color = "Eclipse Gray", Price = 28500000 };
                    var v2 = new ProductVariant { ProductId = p2.ProductId, Cpu = "Core i9", Ram = "32GB", Ssd = "1TB", Color = "Silver", Price = 42000000 };
                    var v3 = new ProductVariant { ProductId = p3.ProductId, Cpu = "Apple M2", Ram = "8GB", Ssd = "256GB", Color = "Midnight", Price = 26900000 };

                    context.ProductVariants.AddRange(v1, v2, v3);
                    context.SaveChanges();

                    // C. Nhập Kho Vật Lý (PhysicalProducts) - Sinh ra các máy có số Seri thực tế
                    var branchHN = context.Branches.FirstOrDefault(b => b.BranchName == "Chi nhánh Hà Nội");
                    if (branchHN != null)
                    {
                        context.PhysicalProducts.AddRange(
                            // 2 chiếc Asus ROG ở kho HN
                            new PhysicalProduct { VariantId = v1.VariantId, BranchId = branchHN.BranchId, SerialNumber = "ROG-SN001", Status = "InStock" },
                            new PhysicalProduct { VariantId = v1.VariantId, BranchId = branchHN.BranchId, SerialNumber = "ROG-SN002", Status = "InStock" },

                            // 1 chiếc Dell XPS ở kho HN
                            new PhysicalProduct { VariantId = v2.VariantId, BranchId = branchHN.BranchId, SerialNumber = "XPS-SN001", Status = "InStock" },

                            // 2 chiếc MacBook ở kho HN
                            new PhysicalProduct { VariantId = v3.VariantId, BranchId = branchHN.BranchId, SerialNumber = "MAC-SN001", Status = "InStock" },
                            new PhysicalProduct { VariantId = v3.VariantId, BranchId = branchHN.BranchId, SerialNumber = "MAC-SN002", Status = "InStock" }
                        );
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}