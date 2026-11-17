using KeyEventLoggerApp.Models;

namespace KeyEventLoggerApp.Services;

/// <summary>
/// Klavye olaylarının kaydını, filtrelenmesini ve tuş kullanım istatistiklerini yönetir.
/// </summary>
public sealed class KeyLoggerSession
{
    private readonly List<KeyLogEntry> _entries = [];

    public bool IsRecording { get; private set; }
    public bool LogKeyUpEvents { get; set; } = true;
    public bool LogKeyPressEvents { get; set; } = true;

    public IReadOnlyList<KeyLogEntry> Entries => _entries;

    public int TotalKeyDownCount => _entries.Count(e => e.EventKind == KeyEventKind.KeyDown);
    public int TotalEventsCount => _entries.Count;

    public void StartRecording()
    {
        IsRecording = true;
    }

    public void StopRecording()
    {
        IsRecording = false;
    }

    public void Clear()
    {
        _entries.Clear();
    }

    public KeyLogEntry? RecordKeyDown(Keys keyCode, int keyValue, Keys modifiers, DateTime? customTimestamp = null)
    {
        if (!IsRecording)
        {
            return null;
        }

        KeyLogEntry entry = new(
            customTimestamp ?? DateTime.Now,
            KeyEventKind.KeyDown,
            FormatKeyName(keyCode),
            keyValue,
            FormatModifiers(modifiers));

        _entries.Add(entry);
        return entry;
    }

    public KeyLogEntry? RecordKeyUp(Keys keyCode, int keyValue, Keys modifiers, DateTime? customTimestamp = null)
    {
        if (!IsRecording || !LogKeyUpEvents)
        {
            return null;
        }

        KeyLogEntry entry = new(
            customTimestamp ?? DateTime.Now,
            KeyEventKind.KeyUp,
            FormatKeyName(keyCode),
            keyValue,
            FormatModifiers(modifiers));

        _entries.Add(entry);
        return entry;
    }

    public KeyLogEntry? RecordKeyPress(char keyChar, DateTime? customTimestamp = null)
    {
        if (!IsRecording || !LogKeyPressEvents || char.IsControl(keyChar))
        {
            return null;
        }

        KeyLogEntry entry = new(
            customTimestamp ?? DateTime.Now,
            KeyEventKind.KeyPress,
            $"Char({keyChar})",
            keyChar,
            "Yok",
            keyChar.ToString());

        _entries.Add(entry);
        return entry;
    }

    public IReadOnlyList<KeyLogEntry> FilterEntries(DateTime? startTime, DateTime? endTime, string? keyFilter)
    {
        IEnumerable<KeyLogEntry> query = _entries;

        if (startTime.HasValue)
        {
            query = query.Where(e => e.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(e => e.Timestamp <= endTime.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyFilter))
        {
            string term = keyFilter.Trim();
            query = query.Where(e =>
                e.KeyName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                e.EventKind.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (e.Character != null && e.Character.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        return query.ToList();
    }

    public IReadOnlyList<KeyUsageStatistic> GetTopKeys(int topCount = 6)
    {
        List<KeyLogEntry> keyDowns = _entries.Where(e => e.EventKind == KeyEventKind.KeyDown).ToList();
        int total = keyDowns.Count;
        if (total == 0)
        {
            return [];
        }

        return keyDowns
            .GroupBy(e => e.KeyName)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Take(topCount)
            .Select(g => new KeyUsageStatistic(g.Key, g.Count(), Math.Round(g.Count() * 100.0 / total, 1)))
            .ToList();
    }

    private static string FormatKeyName(Keys key) => key switch
    {
        Keys.Space => "Space (Boşluk)",
        Keys.Return => "Enter",
        Keys.Back => "Backspace",
        Keys.Escape => "Esc",
        Keys.Capital => "CapsLock",
        Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey => "Shift",
        Keys.ControlKey or Keys.LControlKey or Keys.RControlKey => "Ctrl",
        Keys.Menu or Keys.LMenu or Keys.RMenu => "Alt",
        >= Keys.D0 and <= Keys.D9 => $"Rakam {(int)(key - Keys.D0)}",
        _ => key.ToString()
    };

    private static string FormatModifiers(Keys modifiers)
    {
        if (modifiers == Keys.None)
        {
            return "Yok";
        }

        List<string> parts = [];
        if (modifiers.HasFlag(Keys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(Keys.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(Keys.Alt)) parts.Add("Alt");
        return string.Join("+", parts);
    }
}
