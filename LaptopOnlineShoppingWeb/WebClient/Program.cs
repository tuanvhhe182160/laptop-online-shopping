namespace WebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Cung c?p HttpContext ?? ??c Token/Cookie trong các Service
            builder.Services.AddHttpContextAccessor();

            // 2. Add HttpClient for making API calls (Named Client)
            builder.Services.AddHttpClient("WebAPI", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // 3. ??NG KÝ DEPENDENCY INJECTION CHO CÁC SERVICE (WEB CLIENT)
            // L?u ý: B?n c?n thay th? các tên Interface và Class d??i ?ây 
            // sao cho kh?p v?i tên file th?c t? b?n t?o trong th? m?c Services c?a project WebClient.

            // Ví d? ??ng ký các Service g?i API:
            // builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
            // builder.Services.AddScoped<IProductApiClient, ProductApiClient>();
            // builder.Services.AddScoped<ICartApiClient, CartApiClient>();
            // builder.Services.AddScoped<IOrderApiClient, OrderApiClient>();
            // builder.Services.AddScoped<ICategoryApiClient, CategoryApiClient>();

            // 4. Authentication
            builder.Services.AddAuthentication("MyCookieAuth")
                .AddCookie("MyCookieAuth", options =>
                {
                    options.Cookie.Name = "LaptopShop.AuthCookie";
                    options.LoginPath = "/Auth/Login";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });

            // 5. Add services to the container (Razor Pages)
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AddPageRoute("/Storefront/Index", "");
            }).AddCookieTempDataProvider();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // C?u hình Middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}