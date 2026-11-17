using System.Text;
using KeyEventLoggerApp.Models;

namespace KeyEventLoggerApp.Services;

/// <summary>
/// Yakalanan klavye olaylarını ve istatistik özetini .txt dosyasına StreamWriter ile kaydeder.
/// </summary>
public sealed class LogFileExporter
{
    public void ExportToTextFile(
        string filePath,
        IReadOnlyList<KeyLogEntry> entries,
        IReadOnlyList<KeyUsageStatistic> topKeys,
        int totalKeyDowns)
    {
        using StreamWriter writer = new(filePath, append: false, Encoding.UTF8);

        writer.WriteLine("================================================================================");
        writer.WriteLine(" YMH 219 — KEY EVENT LOGGER (EĞİTSEL KLAVYE OLAY KAYDEDİCİ) RAPORU");
        writer.WriteLine($" Dışa Aktarım Zamanı : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine($" Toplam Olay Sayısı  : {entries.Count}");
        writer.WriteLine($" Toplam Tuş Basımı   : {totalKeyDowns}");
        writer.WriteLine("================================================================================");
        writer.WriteLine();

        if (topKeys.Count > 0)
        {
            writer.WriteLine("--- EN SIK KULLANILAN TUŞLAR İSTATİSTİĞİ ---");
            foreach (KeyUsageStatistic stat in topKeys)
            {
                writer.WriteLine($"  • {stat.KeyName,-18} : {stat.Count,4} kez (%{stat.Percentage:0.0})");
            }
            writer.WriteLine();
        }

        writer.WriteLine("--- ZAMAN DAMGALI KLAVYE OLAY KAYITLARI ---");
        foreach (KeyLogEntry entry in entries)
        {
            writer.WriteLine(entry.ToFormattedLogLine());
        }
    }
}
