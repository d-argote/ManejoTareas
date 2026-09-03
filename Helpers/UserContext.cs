namespace ManejoTareas.Helpers;

/// <summary>
/// Almacena el usuario actual para propagarlo al interceptor RLS via AsyncLocal.
/// </summary>
public static class UserContext
{
    private static readonly AsyncLocal<int?> _currentUserId = new();
    private static readonly AsyncLocal<bool> _isAdmin = new();

    public static int? CurrentUserId
    {
        get => _currentUserId.Value;
        set => _currentUserId.Value = value;
    }

    public static bool IsAdmin
    {
        get => _isAdmin.Value;
        set => _isAdmin.Value = value;
    }

    public static void Clear()
    {
        _currentUserId.Value = null;
        _isAdmin.Value = false;
    }
}
