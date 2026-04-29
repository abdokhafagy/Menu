using System.Globalization;

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using MudBlazor;

namespace Menu.UI.State;

public sealed class AppState
{
    private const string DarkModeStorageKey = "menu.ui.darkMode";
    private const string CultureStorageKey = "menu.ui.culture";

    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigationManager;
    private bool _isInitialized;

    public AppState(ILocalStorageService localStorage, IJSRuntime jsRuntime, NavigationManager navigationManager)
    {
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
        _navigationManager = navigationManager;
    }

    public event Action? OnChange;

    public bool IsDarkMode { get; private set; }

    public string Culture { get; private set; } = "en";

    public bool IsRtl => Culture.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

    public MudTheme Theme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#205b4e",
            Secondary = "#d96f32",
            Background = "#f8f4eb",
            Surface = "#ffffff",
            AppbarBackground = "#ffffff",
            DrawerBackground = "#ffffff"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#89d0be",
            Secondary = "#ffb185"
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = new[] { "Space Grotesk", "Segoe UI", "sans-serif" }
            }
        }
    };

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        try
        {
            IsDarkMode = await _localStorage.GetItemAsync<bool>(DarkModeStorageKey);
        }
        catch
        {
            IsDarkMode = false;
        }

        try
        {
            var savedCulture = await _localStorage.GetItemAsync<string?>(CultureStorageKey);
            Culture = NormalizeCulture(savedCulture);
        }
        catch
        {
            Culture = "en";
        }

        // Culture is already applied to CultureInfo statics in Program.Main before RunAsync.
        // Here we only sync the document direction and notify subscribers.
        await ApplyLanguageDirectionAsync();
        NotifyStateChanged();
    }

    public async Task SetDarkModeAsync(bool isDarkMode)
    {
        if (IsDarkMode == isDarkMode)
        {
            return;
        }

        IsDarkMode = isDarkMode;

        try
        {
            await _localStorage.SetItemAsync(DarkModeStorageKey, IsDarkMode);
        }
        catch
        {
            // Ignore persistence failures and keep in-memory state.
        }

        NotifyStateChanged();
    }

    public async Task SetCultureAsync(string culture)
    {
        var normalizedCulture = NormalizeCulture(culture);
        if (string.Equals(Culture, normalizedCulture, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            await _localStorage.SetItemAsync(CultureStorageKey, normalizedCulture);
        }
        catch
        {
            // If persistence fails we cannot guarantee a correct reload, so abort.
            return;
        }

        // Force a full app reboot so:
        //  - Program.Main re-reads localStorage and sets CultureInfo before any component renders
        //  - Every IStringLocalizer<T> instance is recreated under the new UICulture
        //  - MudBlazor's internal text and any cached strings are refreshed
        _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad: true);
    }

    private static string NormalizeCulture(string? culture)
    {
        return culture?.StartsWith("ar", StringComparison.OrdinalIgnoreCase) == true ? "ar" : "en";
    }

    private async Task ApplyLanguageDirectionAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("menuUi.setLanguage", Culture, IsRtl ? "rtl" : "ltr");
        }
        catch
        {
            // JS interop can fail during startup transitions; app remains usable.
        }
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}
