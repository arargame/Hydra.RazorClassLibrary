using System.Collections.Generic;
using System.Threading.Tasks;
using Hydra.DataModels;
using Hydra.DTOs;

namespace Hydra.RazorClassLibrary.Services.Core
{
    public class ViewFilterState
    {
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public List<string?> Values { get; set; } = new();
    }

    public class ViewPreferenceState
    {
        public string ViewKey { get; set; } = string.Empty;
        public List<string> VisibleColumns { get; set; } = new();
        public List<ViewFilterState> Filters { get; set; } = new();
        public string? OrderedColumnName { get; set; }
        public SortingDirection? SortingDirection { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Kullanıcının sayfa bazlı grid tercihlerini (filtreler, sıralama, seçili sütunlar, sayfa numarası)
    /// oturum süresince (in-memory) ve kalıcı olarak (LocalStorage + UserViewPreference) yöneten servis.
    /// </summary>
    public interface ITableStateManager
    {
        Task SaveStateAsync(string viewKey, TableDTO table, Guid? systemUserId = null);
        Task<ViewPreferenceState?> GetStateAsync(string viewKey, Guid? systemUserId = null);
        void ApplyStateToRequest(TableDTO request, ViewPreferenceState state);
        void ApplyStateToTable(TableDTO table, ViewPreferenceState state);
        Task ClearStateAsync(string viewKey);
    }
}
