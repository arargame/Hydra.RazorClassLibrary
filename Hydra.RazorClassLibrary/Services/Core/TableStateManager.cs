using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hydra.DataModels;
using Hydra.DataModels.Filter;
using Hydra.DTOs;
using Hydra.RazorClassLibrary.Services.Storage;

namespace Hydra.RazorClassLibrary.Services.Core
{
    public class TableStateManager : ITableStateManager
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ConcurrentDictionary<string, ViewPreferenceState> _memoryCache = new();

        public TableStateManager(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        private static string StorageKey(string viewKey) => $"hydra_view_pref_{viewKey}";

        public async Task SaveStateAsync(string viewKey, TableDTO table, Guid? systemUserId = null)
        {
            if (string.IsNullOrWhiteSpace(viewKey) || table == null)
                return;

            var state = new ViewPreferenceState
            {
                ViewKey = viewKey,
                PageNumber = Math.Max(table.PageNumber, 1),
                PageSize = table.PageSize > 0 ? table.PageSize : 10
            };

            // 1. Görünür kolonlar
            if (table.MetaColumns != null)
            {
                state.VisibleColumns = table.GetSelectedMetaColumnsExceptPrimaryKeyWithIncludes
                    .Where(c => !c.IsJoinedTableKey && !string.IsNullOrEmpty(c.Name))
                    .Select(c => c.Name!)
                    .ToList();
            }

            // 2. Aktif filtreler
            var filteredCols = table.GetFilteredMetaColumnsWithIncludes?
                .Where(c => c.FilterDTO != null && c.FilterDTO.Parameters != null &&
                            c.FilterDTO.Parameters.Any(p => p.Value != null && !string.IsNullOrWhiteSpace(p.Value.ToString())))
                .ToList() ?? new List<MetaColumnDTO>();

            foreach (var col in filteredCols)
            {
                var paramValues = col.FilterDTO!.Parameters
                    .Where(p => p.Value != null && !string.IsNullOrWhiteSpace(p.Value.ToString()))
                    .Select(p => p.Value!.ToString())
                    .ToList();

                if (paramValues.Any())
                {
                    state.Filters.Add(new ViewFilterState
                    {
                        TableName = col.TableName ?? string.Empty,
                        ColumnName = col.Name ?? string.Empty,
                        Values = paramValues
                    });
                }
            }

            // 3. Sıralama
            var orderedCol = table.GetOrderedMetaColumnsWithIncludes?
                .FirstOrDefault(c => c.IsOrdered);

            if (orderedCol != null)
            {
                state.OrderedColumnName = orderedCol.Name;
                state.SortingDirection = orderedCol.Direction;
            }

            // In-memory cache güncelle (SPA içi anında geçiş)
            _memoryCache[viewKey] = state;

            // LocalStorage güncelle (sayfa yenileme / F5 dayanıklılığı)
            try
            {
                await _localStorage.SetItemAsync(StorageKey(viewKey), state);
            }
            catch
            {
                // LocalStorage erişim istisnaları yutulur
            }
        }

        public async Task<ViewPreferenceState?> GetStateAsync(string viewKey, Guid? systemUserId = null)
        {
            if (string.IsNullOrWhiteSpace(viewKey))
                return null;

            // 1. Önce in-memory cache'e bak
            if (_memoryCache.TryGetValue(viewKey, out var state))
                return state;

            // 2. Yoksa LocalStorage'dan yükle
            try
            {
                var stored = await _localStorage.GetItemAsync<ViewPreferenceState>(StorageKey(viewKey));
                if (stored != null)
                {
                    _memoryCache[viewKey] = stored;
                    return stored;
                }
            }
            catch
            {
            }

            return null;
        }

        public void ApplyStateToRequest(TableDTO request, ViewPreferenceState state)
        {
            if (request == null || state == null)
                return;

            request.PageNumber = state.PageNumber;
            request.PageSize = state.PageSize;

            if (state.Filters != null && state.Filters.Any())
            {
                foreach (var filterState in state.Filters)
                {
                    var existing = request.MetaColumns?.FirstOrDefault(c => c.Name == filterState.ColumnName);
                    if (existing?.FilterDTO != null)
                    {
                        existing.FilterDTO.SetParameters(filterState.Values.Cast<object?>().ToList());
                    }
                    else
                    {
                        // Request henüz boş MetaColumns listesine sahipse, dinamik filter DTO ekle
                        if (filterState.Values.Count == 1 && !string.IsNullOrEmpty(filterState.Values[0]))
                        {
                            var filterCol = MetaColumnDTO.CreateColumnDTOWithContainsFilter(
                                name: filterState.ColumnName,
                                alias: null,
                                value: filterState.Values[0]!
                            );
                            filterCol.TableName = filterState.TableName;
                            request.AlterOrAddMetaColumn(filterCol);
                        }
                        else if (filterState.Values.Count >= 2 && !string.IsNullOrEmpty(filterState.Values[0]) && !string.IsNullOrEmpty(filterState.Values[1]))
                        {
                            var filterCol = MetaColumnDTO.CreateColumnDTOWithFilter(
                                name: filterState.ColumnName,
                                alias: null,
                                priority: 0,
                                filterTypeName: nameof(BetweenFilter),
                                filterPriority: 0,
                                createFilterComponentFromThis: true,
                                values: filterState.Values.Cast<object?>().ToList()
                            );
                            filterCol.TableName = filterState.TableName;
                            request.AlterOrAddMetaColumn(filterCol);
                        }
                    }
                }
            }

            // Sıralama
            if (!string.IsNullOrWhiteSpace(state.OrderedColumnName) && state.SortingDirection.HasValue)
            {
                var orderCol = MetaColumnDTO.CreateColumnDTOToOrder(
                    name: state.OrderedColumnName,
                    alias: null,
                    sortingDirection: state.SortingDirection.Value
                );
                request.AlterOrAddMetaColumn(orderCol);
            }
        }

        public void ApplyStateToTable(TableDTO table, ViewPreferenceState state)
        {
            if (table == null || state == null || table.MetaColumns == null)
                return;

            // Filtreleri bağla
            if (state.Filters != null && state.Filters.Any())
            {
                foreach (var filterState in state.Filters)
                {
                    var matchingCols = table.MetaColumns
                        .Where(c => c.Name == filterState.ColumnName &&
                                   (string.IsNullOrEmpty(filterState.TableName) || c.TableName == filterState.TableName))
                        .ToList();

                    foreach (var col in matchingCols.Where(c => c.FilterDTO != null))
                    {
                        var paramObjects = filterState.Values.Cast<object?>().ToList();
                        col.FilterDTO!.SetParameters(paramObjects);
                    }
                }
            }

            // Sıralamayı bağla
            if (!string.IsNullOrWhiteSpace(state.OrderedColumnName) && state.SortingDirection.HasValue)
            {
                foreach (var ordered in table.GetOrderedMetaColumnsWithIncludes)
                {
                    if (ordered.Name == state.OrderedColumnName)
                    {
                        ordered.IsOrdered = true;
                        ordered.Direction = state.SortingDirection.Value;
                    }
                    else
                    {
                        ordered.IsOrdered = false;
                    }
                }
            }
        }

        public async Task ClearStateAsync(string viewKey)
        {
            if (string.IsNullOrWhiteSpace(viewKey))
                return;

            _memoryCache.TryRemove(viewKey, out _);

            try
            {
                await _localStorage.RemoveItemAsync(StorageKey(viewKey));
            }
            catch
            {
            }
        }
    }
}
