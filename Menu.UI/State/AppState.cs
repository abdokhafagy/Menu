using System.Globalization;

using Blazored.LocalStorage;

using Microsoft.JSInterop;

using MudBlazor;

namespace Menu.UI.State;

public sealed class AppState
{
    private const string DarkModeStorageKey = "menu.ui.darkMode";
    private const string CultureStorageKey = "menu.ui.culture";

    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized;

    public AppState(ILocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
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

        ApplyCulture(Culture);
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
        var hasChanged = !string.Equals(Culture, normalizedCulture, StringComparison.Ordinal);

        Culture = normalizedCulture;
        ApplyCulture(Culture);

        try
        {
            await _localStorage.SetItemAsync(CultureStorageKey, Culture);
        }
        catch
        {
            // Ignore persistence failures and keep in-memory state.
        }

        await ApplyLanguageDirectionAsync();

        if (hasChanged)
        {
            NotifyStateChanged();
        }
    }

    private static string NormalizeCulture(string? culture)
    {
        return culture?.StartsWith("ar", StringComparison.OrdinalIgnoreCase) == true ? "ar" : "en";
    }

    private static void ApplyCulture(string culture)
    {
        var cultureInfo = CultureInfo.GetCultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
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
