namespace WebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<WebClient.Handlers.AuthHeaderHandler>();

            // Add HttpClient for making API calls
            builder.Services.AddHttpClient("WebAPI", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddHttpMessageHandler<WebClient.Handlers.AuthHeaderHandler>();

            // 3. ??NG K� DEPENDENCY INJECTION CHO C�C SERVICE (WEB CLIENT)
            // L?u �: B?n c?n thay th? c�c t�n Interface v� Class d??i ?�y 
            // sao cho kh?p v?i t�n file th?c t? b?n t?o trong th? m?c Services c?a project WebClient.

            // V� d? ??ng k� c�c Service g?i API:
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

            // C?u h�nh Middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}