namespace KeyEventLoggerApp.Models;

public enum KeyEventKind
{
    KeyDown,
    KeyPress,
    KeyUp
}

/// <summary>
/// Uygulama penceresi odaktayken yakalanan tek bir klavye olayını temsil eder.
/// </summary>
public sealed class KeyLogEntry
{
    public DateTime Timestamp { get; }
    public KeyEventKind EventKind { get; }
    public string KeyName { get; }
    public int KeyValue { get; }
    public string Modifiers { get; }
    public string? Character { get; }

    public KeyLogEntry(
        DateTime timestamp,
        KeyEventKind eventKind,
        string keyName,
        int keyValue,
        string modifiers,
        string? character = null)
    {
        Timestamp = timestamp;
        EventKind = eventKind;
        KeyName = keyName;
        KeyValue = keyValue;
        Modifiers = string.IsNullOrWhiteSpace(modifiers) ? "Yok" : modifiers;
        Character = character;
    }

    public string ToFormattedLogLine()
    {
        string charPart = !string.IsNullOrEmpty(Character) ? $" | Karakter: '{Character}'" : string.Empty;
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{EventKind,-8}] Tuş: {KeyName,-12} (Kod: {KeyValue,3}) | Mod: {Modifiers,-10}{charPart}";
    }

    public override string ToString() => ToFormattedLogLine();
}

/// <summary>
/// En sık kullanılan tuş istatistiğini tutan veri modeli.
/// </summary>
public sealed class KeyUsageStatistic
{
    public string KeyName { get; }
    public int Count { get; }
    public double Percentage { get; }

    public KeyUsageStatistic(string keyName, int count, double percentage)
    {
        KeyName = keyName;
        Count = count;
        Percentage = percentage;
    }
}
