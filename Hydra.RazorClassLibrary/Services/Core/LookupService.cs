using Hydra.DataModels;
using Hydra.DTOs;
using Hydra.DTOs.ViewDTOs;
using Hydra.RazorClassLibrary.ComponentModels;
using Hydra.RazorClassLibrary.Services.Http;

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

            var enumType = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .FirstOrDefault(t => t.IsEnum && t.Name == enumTypeName);

            if (enumType == null)
                return new List<DropdownListOption>();

            var options = Enum.GetValues(enumType)
                .Cast<object>()
                .Select(v => new DropdownListOption(
                    key: Convert.ToInt32(v).ToString(),
                    value: v.ToString()))
                .ToList();

            _cache[$"enum/{enumTypeName}"] = options;

            return WithEmptyOption(options, addEmptyOption);
        }

        public List<DropdownListOption> GetBooleanOptions(bool addEmptyOption = false)
        {
            var options = new List<DropdownListOption>
            {
                new DropdownListOption("true", "Evet"),
                new DropdownListOption("false", "Hayır")
            };

            return WithEmptyOption(options, addEmptyOption);
        }

        public void ClearCache() => _cache.Clear();

        private static List<DropdownListOption> WithEmptyOption(List<DropdownListOption> options, bool addEmptyOption)
        {
            var list = options.Select(o => new DropdownListOption(o.Key, o.Value, o.IsSelected)).ToList();

            if (addEmptyOption)
                list.Insert(0, new DropdownListOption(string.Empty, "— Seçiniz —"));

            return list;
        }
    }
}
