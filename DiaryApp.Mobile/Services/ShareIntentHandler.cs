namespace DiaryApp.Mobile.Services;

/// <summary>
/// Servicio para gestionar el estado de share intents
/// </summary>
public static class ShareIntentHandler
{
    private static bool _hasSharedIntent = false;
    
    public static void SetSharedIntentReceived(bool value)
    {
        _hasSharedIntent = value;
        System.Diagnostics.Debug.WriteLine($"📌 Shared intent received flag set to: {value}");
    }
    
    public static bool HasSharedIntent()
    {
        return _hasSharedIntent;
    }
    
    public static void Clear()
    {
        _hasSharedIntent = false;
        System.Diagnostics.Debug.WriteLine("🗑️ Shared intent flag cleared");
    }
}