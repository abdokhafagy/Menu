using Blazored.LocalStorage;

using Menu.UI.Auth;

using Menu.UI.Services;
using Menu.UI.State;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor;
using MudBlazor.Services;
using System.Globalization;

namespace Menu.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
            builder.Services.AddAuthorizationCore();

            builder.Services.AddTransient<JwtAuthorizationHandler>();

            var apiBaseUrl = builder.HostEnvironment.IsDevelopment()
                ? "https://localhost:7106/"
                : builder.Configuration["ApiBaseUrl"] ?? "https://menu-api.runasp.net/";

            builder.Services.AddHttpClient("MenuApi", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            }).AddHttpMessageHandler<JwtAuthorizationHandler>();

            builder.Services.AddHttpClient("MenuApiNoAuth", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MenuApi"));

            builder.Services.AddScoped<IApiService, ApiService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<RestaurantService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<RoleService>();
            builder.Services.AddScoped<PermissionService>();
            builder.Services.AddScoped<MenuService>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<MenuItemService>();
            builder.Services.AddScoped<ItemOptionService>();
            builder.Services.AddScoped<OptionValueService>();
            builder.Services.AddScoped<PublicMenuService>();
            builder.Services.AddScoped<ImageUploadService>();

            builder.Services.AddScoped<AppState>();
            builder.Services.AddScoped<UserState>();
            builder.Services.AddScoped<CartState>();
            builder.Services.AddScoped<UserContextService>();
            builder.Services.AddScoped<AuthorizationService>();

            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 4000;
                config.SnackbarConfiguration.HideTransitionDuration = 300;
                config.SnackbarConfiguration.ShowTransitionDuration = 300;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            var host = builder.Build();

            // Resolve the user's saved culture BEFORE the first render so IStringLocalizer
            // and every component get the correct CultureInfo on their initial pass.
            await ApplyInitialCultureAsync(host.Services);

            await host.RunAsync();
        }

        private static async Task ApplyInitialCultureAsync(IServiceProvider services)
        {
            const string CultureStorageKey = "menu.ui.culture";
            string cultureName = "en";

            try
            {
                var localStorage = services.GetRequiredService<Blazored.LocalStorage.ILocalStorageService>();
                var saved = await localStorage.GetItemAsync<string?>(CultureStorageKey);
                if (!string.IsNullOrWhiteSpace(saved))
                {
                    cultureName = saved.StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
                }
            }
            catch
            {
                // localStorage unavailable on first boot or in pre-render; fall back to default.
            }

            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }
}