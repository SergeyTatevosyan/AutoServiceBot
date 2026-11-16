namespace AutoServiceBot.Configuration;

/// <summary>
/// Каталог услуг автосервиса.
/// </summary>
public static class AutoServiceCatalog
{
    /// <summary>
    /// Список доступных услуг автосервиса.
    /// </summary>
    public static readonly Dictionary<string, string> Services = new()
    {
        ["maintenance"] = "🛠 Техническое обслуживание (ТО)",
        ["diagnostics"] = "🔍 Диагностика автомобиля",
        ["engine"] = "🔧 Ремонт двигателя",
        ["suspension"] = "🚘 Ремонт ходовой части",
        ["transmission"] = "⚙️ Ремонт трансмиссии",
        ["electrics"] = "⚡ Автоэлектрика",
        ["tire"] = "🛞 Шиномонтаж",
        ["other"] = "📋 Другая услуга"
    };
}