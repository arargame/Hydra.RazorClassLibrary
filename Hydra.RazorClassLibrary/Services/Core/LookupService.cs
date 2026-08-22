using Hydra.DataModels;
using Hydra.DTOs;
using Hydra.DTOs.ViewDTOs;
using Hydra.RazorClassLibrary.ComponentModels;
using Hydra.RazorClassLibrary.Services.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Hydra.RazorClassLibrary.Services.Core
{
    /// <summary>
    /// Foreign key / enum / boolean alanları için dropdown seçeneklerini üretir.
    /// FK'lar için hedef tablodan (NavigationColumnInfoDTO) Id + görüntülenecek kolonu çeker.
    /// </summary>
    public class LookupService
    {
        private readonly IHttpClientService _http;

        //Scoped yaşam döngüsünde (circuit başına) basit cache
        private readonly Dictionary<string, List<DropdownListOption>> _cache = new();

        public LookupService(IHttpClientService http)
        {
            _http = http;
        }

        /// <summary>
        /// FK alanı için hedef tablodan key(Id) / value(display kolonu) listesi çeker.
        /// </summary>
        public async Task<List<DropdownListOption>> GetNavigationOptionsAsync(
            NavigationColumnInfoDTO navigation,
            bool addEmptyOption = true,
            int pageSize = 1000,
            bool useCache = true)
        {
            var tableName = navigation.RightTableName;

            var displayColumn = string.IsNullOrEmpty(navigation.NameToDisplay) || navigation.NameToDisplay == "Id"
                                    ? "Name"
                                    : navigation.NameToDisplay;

            var cacheKey = $"{tableName}/{displayColumn}";

            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return WithEmptyOption(cached, addEmptyOption);

            var tableDTO = new TableDTO(tableName)
            {
                PageNumber = 1,
                PageSize = pageSize,
                ViewType = ViewType.LookupView
            };

            tableDTO.SetMetaColumns(
                MetaColumnDTO.CreateColumnDTOToSelect("Id", null).SetAsPrimaryKey(),
                MetaColumnDTO.CreateColumnDTOToSelect(displayColumn, null),
                MetaColumnDTO.CreateColumnDTOToOrder(displayColumn, null, SortingDirection.Ascending));

            var result = await _http.PostEnvelopeAsync<TableDTO>(
                controller: tableName,
                action: "Select",
                payload: tableDTO,
                parameters: new Dictionary<string, string> { ["viewType"] = ViewType.LookupView.ToString() });

            var options = result?.Rows?
                .Select(r => new DropdownListOption(
                    key: r.Id != Guid.Empty ? r.Id.ToString() : r.GetValueByColumnName("Id")?.ToString() ?? string.Empty,
                    value: r.GetValueByColumnName(displayColumn)?.ToString() ?? r.Id.ToString()))
                .Where(o => !string.IsNullOrEmpty(o.Key))
                .ToList() ?? new List<DropdownListOption>();

            _cache[cacheKey] = options;

            return WithEmptyOption(options, addEmptyOption);
        }

        /// <summary>
        /// Enum tipindeki alanlar için (PropertyTypeName üzerinden) seçenek üretir.
        /// Enum tipi yüklü assembly'lerde isimle aranır.
        /// </summary>
        public List<DropdownListOption> GetEnumOptions(string? enumTypeName, bool addEmptyOption = false)
        {
            if (string.IsNullOrWhiteSpace(enumTypeName))
                return new List<DropdownListOption>();

            if (_cache.TryGetValue($"enum/{enumTypeName}", out var cached))
                return WithEmptyOption(cached, addEmptyOption);

            var enumType = ResolveEnumType(enumTypeName);

            if (enumType == null)
                return new List<DropdownListOption>();

            var options = Enum.GetValues(enumType)
                .Cast<object>()
                .Select(v => new DropdownListOption(
                    key: Convert.ToInt32(v).ToString(),
                    value: GetEnumMemberLabel(enumType, v)))
                .ToList();

            _cache[$"enum/{enumTypeName}"] = options;

            return WithEmptyOption(options, addEmptyOption);
        }

        public List<DropdownListOption> GetBooleanOptions(bool addEmptyOption = false)
        {
            var options = new List<DropdownListOption>
            {
                new DropdownListOption("true", "Yes"),
                new DropdownListOption("false", "No")
            };

            return WithEmptyOption(options, addEmptyOption);
        }

        /// <summary>
        /// Bir grid/detay hücresinin ham değerini kullanıcıya gösterilecek metne çevirir.
        ///
        /// Bunun asıl sebebi enum'lar: veritabanından gelen değer ham hâliyle 0/1/2 (ya da bazı
        /// yazıcılarda doğrudan "Error" gibi isim) olur. MetaColumnDTO zaten ValueType=Enum ve
        /// PropertyTypeName (ör. "LogProcessType") bilgisini taşıdığı için, dönüşüm burada
        /// merkezî olarak yapılabilir ve bütün grid'ler bundan faydalanır.
        /// </summary>
        public string? GetDisplayText(MetaColumnDTO column, object? rawValue)
        {
            var text = rawValue?.ToString();

            if (string.IsNullOrEmpty(text))
                return text;

            return column.ValueType switch
            {
                ColumnValueType.Enum => GetEnumDisplayText(column.PropertyTypeName, text),
                ColumnValueType.Boolean => GetBooleanDisplayText(text),
                _ => text
            };
        }

        /// <summary>
        /// Enum'un ham değerini (int ya da isim) okunabilir isme çevirir.
        /// Enum tipi yüklü assembly'lerde bulunamazsa ham değer aynen döner — veri asla kaybolmaz.
        /// </summary>
        public string? GetEnumDisplayText(string? enumTypeName, string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(enumTypeName) || string.IsNullOrWhiteSpace(rawValue))
                return rawValue;

            var options = GetEnumOptions(enumTypeName); //cache'li

            if (options.Count == 0)
                return rawValue;

            //1) int olarak saklanmış (Repository/EF yolu)
            var match = options.FirstOrDefault(o => o.Key == rawValue);

            //2) zaten etiketin kendisi
            match ??= options.FirstOrDefault(o => string.Equals(o.Value, rawValue, StringComparison.OrdinalIgnoreCase));

            //3) üye adı olarak saklanmış (ör. LogDbWriterService, Type'ı ToString() ile yazıyor).
            //   Etiket [Display] ile değiştirilmişse üye adı seçenek listesinde yer almaz,
            //   bu yüzden önce enum'a parse edip int karşılığından eşleştir.
            if (match == null)
            {
                var enumType = ResolveEnumType(enumTypeName);

                if (enumType != null && Enum.TryParse(enumType, rawValue, ignoreCase: true, out var parsed) && parsed != null)
                {
                    var key = Convert.ToInt32(parsed).ToString();

                    match = options.FirstOrDefault(o => o.Key == key);
                }
            }

            return match?.Value ?? rawValue;
        }

        public string? GetBooleanDisplayText(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return rawValue;

            if (bool.TryParse(rawValue, out var parsed))
                return parsed ? "Yes" : "No";

            return rawValue switch
            {
                "1" => "Yes",
                "0" => "No",
                _ => rawValue
            };
        }

        /// <summary>
        /// Bir enum üyesinin kullanıcıya gösterilecek etiketini döner.
        ///
        /// Öncelik sırası: [Display(Name = "...")] → [Description("...")] → üyenin kendi adı.
        /// Böylece "InProgress" yerine "Devam Ediyor" yazmak için enum'un üzerine tek satır
        /// attribute koymak yeterli olur; hem dropdown'lar hem grid'ler hem filtreler aynı
        /// etiketi kullanır (bu metot GetEnumOptions üzerinden hepsini besler).
        ///
        /// Etiket, enum'un tanımlandığı assembly'de durur. Bu yüzden ortak Hydra çekirdeğindeki
        /// enum'lara dile özgü etiket KOYULMAZ (üye adları nötr İngilizce kalır); etiketleme
        /// uygulamanın kendi enum'larında yapılır.
        /// </summary>
        /// <summary>Enum tipini yüklü assembly'lerde isimle arar (ör. "LogProcessType").</summary>
        private static Type? ResolveEnumType(string enumTypeName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .FirstOrDefault(t => t.IsEnum && t.Name == enumTypeName);

        private static string GetEnumMemberLabel(Type enumType, object value)
        {
            var name = value.ToString() ?? string.Empty;

            var member = enumType.GetMember(name).FirstOrDefault();

            if (member == null)
                return name;

            var display = member.GetCustomAttribute<DisplayAttribute>()?.Name;

            if (!string.IsNullOrWhiteSpace(display))
                return display;

            var description = member.GetCustomAttribute<DescriptionAttribute>()?.Description;

            return string.IsNullOrWhiteSpace(description) ? name : description;
        }

        public void ClearCache() => _cache.Clear();

        private static List<DropdownListOption> WithEmptyOption(List<DropdownListOption> options, bool addEmptyOption)
        {
            var list = options.Select(o => new DropdownListOption(o.Key, o.Value, o.IsSelected)).ToList();

            if (addEmptyOption)
                list.Insert(0, new DropdownListOption(string.Empty, "— Select —"));

            return list;
        }
    }
}
