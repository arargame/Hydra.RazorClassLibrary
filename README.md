# Hydra Razor Class Library

## Genel Bakış (Overview)

**Hydra Razor Class Library**, Blazor projelerinizde kullanılmak üzere tasarlanmış, **kod tekrarını önlemeyi** ve **geliştirme sürecini standardize etmeyi** hedefleyen kapsamlı bir bileşen kütüphanesidir.

Bu projenin temel amacı, HTML elementlerinin (özellikle CRUD operasyonlarında sıkça kullanılan input, buton, tablo vb.) yönetimini tek bir merkezden sağlayarak, projeler arası tutarlılığı ve bakım kolaylığını artırmaktır.

## Temel Özellikler (Key Features)

-   **Standardizasyon:** Tüm görsel bileşenler ortak bir tabandan (`HtmlElementComponent`) türetilmiştir. Bu sayede `Id` yönetimi, `IsDisabled`, `IsVisible` gibi temel özellikler tüm bileşenlerde standart bir şekilde çalışır.
-   **Model Tabanlı Yaklaşım:** Bileşenlerin mantığı (`ComponentModels`) ile görsel kısmı (`Components`) birbirinden ayrılarak daha temiz ve test edilebilir bir yapı sunulmuştur.
-   **CRUD Odaklı:** Veri giriş ve listeleme işlemleri için özelleştirilmiş bileşenler (Input, Checkbox, Dropdown, Table vb.) hazır sunulur.
-   **Test ve Debug Desteği:** Bileşenler, geliştirme aşamasında kolaylık sağlamak amacıyla dahili `Debugger` ve test özelliklerine (`IsTested`) sahiptir.

## Mimari ve Bileşenler (Architecture & Components)

Kütüphane aşağıdaki ana dizinlerden oluşur:

### 1. ComponentModels (`Hydra.RazorClassLibrary/ComponentModels`)

Bileşenlerin "Business Logic" katmanıdır. Tüm bileşen modelleri `IHtmlElementComponent` arayüzünü uygular ve genellikle `HtmlElementComponent` sınıfından türer.

-   **`HtmlElementComponent`**: Temel sınıf. Benzersiz ID atama, görünürlük kontrolü, CSS sınıfı ve stil yönetimi gibi ortak işlevleri barındırır.
-   **`HtmlElementComponentWithValue<T>`**: Değer taşıyan (Input, Checkbox gibi) bileşenler için generic temel sınıf. `Value` ve `ValueChanged` eventlerini yönetir.

### 2. Components (`Hydra.RazorClassLibrary/Components`)

Razor dosyalarının (.razor) bulunduğu dizindir. Modelleri görselleştirir.

-   **`InputComponent`**: Metin, sayı, tarih vb. girişleri için genel amaçlı input bileşeni. Label desteği vardır.
-   **`ButtonComponent`**: Standart buton işlemleri için.
-   **`TableComponent`**: Dinamik veri listeleme için tablo yapısı.
-   **`DropdownListComponent`**: Seçim listeleri için.
-   **`CheckboxComponent`**: Onay kutuları için.
-   **`SpinnerComponent`**: Yükleniyor durumları için görsel bildirim.

### 3. Utils (`Hydra.RazorClassLibrary/Utils`)

Yardımcı sınıflar.
-   **`BlazorHelper`**: `RenderFragment` oluşturma gibi dinamik bileşen render işlemleri için yardımcı metotlar içerir.

## Kullanım Örneği (Usage)

Bir bileşeni sayfanızda kullanmak için `Hydra.RazorClassLibrary.Components` namespace'ini eklemeniz yeterlidir.

### Basit Input Kullanımı

```razor
@using Hydra.RazorClassLibrary.Components

<!-- Label ile birlikte kullanım -->
<InputComponent T="string" 
                WithLabel="true" 
                @bind-Value="userName" 
                LabelValue="Kullanıcı Adı" 
                Placeholder="Adınızı giriniz" />

<!-- Sadece input olarak kullanım -->
<InputComponent T="int" 
                @bind-Value="userAge" 
                Placeholder="Yaşınız" />

@code {
    private string userName;
    private int userAge;
}
```

## Katkıda Bulunma

Eğer kütüphaneyi incelerken geliştirebileceğiniz bir nokta (örneğin yeni bir form elemanı veya mevcut bir bileşene yeni bir özellik) fark ederseniz, ilgili `ComponentModel` sınıfını güncelleyip ardından Razor bileşenine yansıtarak katkıda bulunabilirsiniz.
