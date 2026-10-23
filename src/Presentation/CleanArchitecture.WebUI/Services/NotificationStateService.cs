using CleanArchitecture.WebUI.Services.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace CleanArchitecture.WebUI.Services;

/// <summary>
/// State service terpusat untuk mengelola notifikasi in-app dan sinkronisasi real-time badge count
/// </summary>
public class NotificationStateService
{
    private readonly NotificationsClient _client;
    private readonly AuthenticationStateProvider _authStateProvider;
    private List<NotificationDto> _notifications = new();

    public NotificationStateService(
        NotificationsClient client,
        AuthenticationStateProvider authStateProvider)
    {
        _client = client;
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Event yang ditembakkan saat ada perubahan state notifikasi (unread count / list)
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Jumlah notifikasi yang belum dibaca
    /// </summary>
    public int UnreadCount { get; private set; }

    /// <summary>
    /// Daftar notifikasi aktif saat ini
    /// </summary>
    public IReadOnlyList<NotificationDto> Notifications => _notifications.AsReadOnly();

    /// <summary>
    /// Status loading pengambilan notifikasi
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Memperbarui jumlah unread count dari WebAPI dan memicu re-render
    /// </summary>
    public async Task RefreshAsync()
    {
        try
        {
            var auth = await _authStateProvider.GetAuthenticationStateAsync();
            if (auth.User.Identity?.IsAuthenticated != true)
            {
                UnreadCount = 0;
                _notifications.Clear();
                NotifyStateChanged();
                return;
            }

            var result = await _client.GetUnreadCountAsync();
            if (result.IsSuccess)
            {
                if (UnreadCount != result.Data)
                {
                    UnreadCount = result.Data;
                    NotifyStateChanged();
                }
            }
        }
        catch
        {
            // Abaikan kesalahan koneksi background polling
        }
    }

    /// <summary>
    /// Memuat daftar notifikasi terbaru dari server
    /// </summary>
    public async Task LoadNotificationsAsync(int pageSize = 20)
    {
        IsLoading = true;
        NotifyStateChanged();

        try
        {
            var auth = await _authStateProvider.GetAuthenticationStateAsync();
            if (auth.User.Identity?.IsAuthenticated != true)
            {
                _notifications.Clear();
                UnreadCount = 0;
                return;
            }

            var result = await _client.GetNotificationsAsync(pageNumber: 1, pageSize: pageSize);
            if (result.IsSuccess && result.Data != null)
            {
                _notifications = result.Data.Items.ToList();

                var countResult = await _client.GetUnreadCountAsync();
                UnreadCount = countResult.IsSuccess ? countResult.Data : _notifications.Count(n => !n.IsRead);
            }
        }
        catch
        {
            // Error ditangani graceful
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Mengurangi jumlah unread count secara instan (optimistic update)
    /// </summary>
    public void DecrementUnread()
    {
        if (UnreadCount > 0)
        {
            UnreadCount--;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Mengosongkan unread count secara instan (optimistic update)
    /// </summary>
    public void ResetUnread()
    {
        if (UnreadCount > 0)
        {
            UnreadCount = 0;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Menandai satu notifikasi sebagai sudah dibaca
    /// </summary>
    public async Task MarkAsReadAsync(Guid id)
    {
        var item = _notifications.FirstOrDefault(n => n.Id == id);
        if (item != null && !item.IsRead)
        {
            item.IsRead = true;
            DecrementUnread();
        }

        try
        {
            var result = await _client.MarkAsReadAsync(id);
            if (!result.IsSuccess)
            {
                // Revert jika gagal
                await RefreshAsync();
            }
        }
        catch
        {
            await RefreshAsync();
        }
    }

    /// <summary>
    /// Menandai seluruh notifikasi sebagai sudah dibaca
    /// </summary>
    public async Task MarkAllAsReadAsync()
    {
        foreach (var item in _notifications)
        {
            item.IsRead = true;
        }
        ResetUnread();

        try
        {
            var result = await _client.MarkAllAsReadAsync();
            if (!result.IsSuccess)
            {
                await LoadNotificationsAsync();
            }
        }
        catch
        {
            await LoadNotificationsAsync();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
