
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using System.Text;
using System.Text.Json.Serialization;
using WebAPI.Data;
using WebAPI.Entities;
using WebAPI.Repositories;
using WebAPI.Services;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Add database context
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Add repository pattern
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            builder.Services.AddScoped<IPhysicalProductRepository, PhysicalProductRepository>();
            builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            //builder.Services.AddScoped<ICartRepository, CartRepository>();
            //builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();

            //Add services
            //builder.Services.AddScoped<ICustomerService, CustomerService>();
            //builder.Services.AddScoped<ICartService, CartService>();
            //builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IWarehouseService, WarehouseService>();
            builder.Services.AddScoped<IFeedbackService, FeedbackService>();

            //Add singleton pattern
            builder.Services.AddSingleton<SystemConfigService>();

            //Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowClient", policy =>
                {
                    policy.WithOrigins("https://localhost:7137", "http://localhost:5247")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            //Add JWT authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            // Add services to the container.
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Product>("Products");
            modelBuilder.EntitySet<ProductVariant>("ProductVariants").EntityType.HasKey(p => p.VariantId);
            modelBuilder.EntitySet<Feedback>("Feedbacks");
            modelBuilder.EntitySet<Category>("Categories");
            modelBuilder.EntitySet<Customer>("Customers");
            modelBuilder.EntitySet<PhysicalProduct>("PhysicalProducts").EntityType.HasKey(p => p.PhysicalId);
            modelBuilder.EntitySet<Branch>("Branches").EntityType.HasKey(b => b.BranchId);
            modelBuilder.EntitySet<Order>("Orders").EntityType.HasKey(o => o.OrderId);
            var edmModel = modelBuilder.GetEdmModel();

            builder.Services.AddControllers()
                .AddOData(options => options
                    .Select()
                    .Filter()
                    .OrderBy()
                    .Expand()
                    .Count()
                    .SetMaxTop(100)
                    .AddRouteComponents("odata", edmModel)
                )
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowClient");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Auto-create database on startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    // Tự động tạo DB mới
                    context.Database.EnsureCreated();
                    DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Có lỗi xảy ra khi tự động khởi tạo database.");
                }
            }

            app.Run();
        }
    }
}
