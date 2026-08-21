namespace Hydra.RazorClassLibrary.Components.CRUD;

/// <summary>
/// GenericDetailsView'ın altındaki CollectionViewSection'ları sekme (tab) olarak yönetmesini
/// sağlayan köprü arayüzü. GenericDetailsView, kendini CascadingValue ile bu arayüz üzerinden
/// aşağı taşır; CollectionViewSection bunu bulursa kendini bir sekme olarak kaydeder ve sadece
/// aktif sekme iken görünür olur. Bulamazsa (ör. GenericDetailsView dışında tek başına
/// kullanılıyorsa) eskisi gibi normal, her zaman görünür şekilde render olur — geriye dönük
/// uyumluluk bozulmaz.
/// </summary>
public interface IDetailsTabsHost
{
    /// <summary>
    /// Bir CollectionViewSection'ı sekme olarak kaydeder. <paramref name="key"/> genelde
    /// bileşenin kendisidir (this) — tekil ve kararlı bir kimlik olması yeterli.
    /// İlk kaydedilen sekme otomatik olarak aktif sekme olur.
    /// </summary>
    void RegisterTab(object key, string title);

    /// <summary>Sekme başlığındaki kayıt sayısı rozetini günceller (veri yüklendikten sonra).</summary>
    void UpdateTabCount(object key, int count);

    /// <summary>Şu an aktif olan sekmenin key'i.</summary>
    object? ActiveTabKey { get; }
}
