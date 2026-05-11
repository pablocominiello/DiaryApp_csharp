using System;
using System.IO;
#if ANDROID
using Android.Content;
using Android.Graphics;
using Android.Provider;
#endif

namespace DiaryApp.Mobile.Services;

/// <summary>
/// Handler estático para gestionar imágenes compartidas desde otras apps (solo Android)
/// </summary>
public static class SharedImageHandler
{
#if ANDROID
    private static Android.Net.Uri? _sharedImageUri;
    
    public static void SetSharedImage(Android.Net.Uri? uri)
    {
        _sharedImageUri = uri;
        System.Diagnostics.Debug.WriteLine($"✅ Shared image URI set: {uri}");
    }

    public static Android.Net.Uri? GetSharedImageUri()
    {
        return _sharedImageUri;
    }

    public static void ClearSharedImage()
    {
        _sharedImageUri = null;
        System.Diagnostics.Debug.WriteLine("🗑️ Shared image URI cleared");
    }

    /// <summary>
    /// Convierte la URI compartida a byte array para subirla al servidor
    /// </summary>
    public static async Task<byte[]?> GetSharedImageBytesAsync()
    {
        if (_sharedImageUri is null)
            return null;

        try
        {
            var context = Android.App.Application.Context;
            using var inputStream = context.ContentResolver?.OpenInputStream(_sharedImageUri);
            
            if (inputStream == null)
                return null;

            using var memoryStream = new MemoryStream();
            await inputStream.CopyToAsync(memoryStream);
            
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error reading shared image: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene el nombre del archivo compartido
    /// </summary>
    public static string GetSharedImageFileName()
    {
        if (_sharedImageUri is null)
            return "shared_image.jpg";

        try
        {
            var context = Android.App.Application.Context;
            var cursor = context.ContentResolver?.Query(_sharedImageUri, null, null, null, null);
            
            if (cursor != null && cursor.MoveToFirst())
            {
                var nameIndex = cursor.GetColumnIndex(OpenableColumns.DisplayName);
                if (nameIndex >= 0)
                {
                    var name = cursor.GetString(nameIndex);
                    cursor.Close();
                    return name ?? $"shared_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                }
                cursor.Close();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Could not get file name: {ex.Message}");
        }

        return $"shared_{DateTime.Now:yyyyMMddHHmmss}.jpg";
    }
#else
    // ✅ Stubs para otras plataformas (no hacen nada, solo evitan errores de compilación)
    public static void SetSharedImage(object? uri)
    {
        // No-op en plataformas no-Android
    }

    public static void ClearSharedImage()
    {
        // No-op en plataformas no-Android
    }

    public static Task<byte[]?> GetSharedImageBytesAsync()
    {
        return Task.FromResult<byte[]?>(null);
    }

    public static string GetSharedImageFileName()
    {
        return "shared_image.jpg";
    }
#endif
}