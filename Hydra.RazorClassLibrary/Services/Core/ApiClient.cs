using Hydra.RazorClassLibrary.Services.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hydra.RazorClassLibrary.Services.Core
{
    public interface IApiClient<T> where T : class
    {
    }

    public class ApiClient<T> : IApiClient<T> where T : class
    {
        protected readonly IHttpClientService Http;

        public string Controller => typeof(T).Name;

        protected string Url(string action = "")
                     => string.IsNullOrWhiteSpace(action)
                        ? Controller
                        : $"{Controller}/{action}";

        public ApiClient(IHttpClientService httpClientService)
        {
            Http = httpClientService;
        }

        // Optional wrapper if needed, but the pattern encourages using Http.GetEnvelopeAsync directly
        // for full control over return types.
        public async Task<TResponse?> GetAsync<TResponse>(string url)
        {
            return await Http.GetAsync<TResponse>(url);
        }

        public async Task<TResponse?> GetAsync<TResponse>(string controller, string action, Dictionary<string, string>? parameters = null, string? pathLikeParameter = null, string? queryString = null)
        {
            var url = Hydra.RazorClassLibrary.Utils.UrlHelper.BuildUrl(controller, action, parameters, pathLikeParameter, queryString);
            return await GetAsync<TResponse>(url);
        }

        public async Task<Hydra.DTOs.TableDTO?> SelectAsync(Hydra.DTOs.TableDTO? tableDTO = null)
        {
            tableDTO ??= new Hydra.DTOs.TableDTO();
            // Ensure Name alias is set if not provided, mimicking the service name
            if(string.IsNullOrEmpty(tableDTO.Name))
            {
                 tableDTO.Name = Controller;
            }

            return await Http.PostEnvelopeAsync<Hydra.DTOs.TableDTO>(
                controller: Controller, 
                action: "Select", 
                payload: tableDTO
            );
        }

        public async Task<T?> UpdateAsync(T entity)
        {
            return await Http.PutEnvelopeAsync<T>(
                controller: Controller, 
                action: "Update", 
                payload: entity
            );
        }
        
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await Http.DeleteEnvelopeAsync<bool>(
                controller: Controller,
                action: "Delete",
                pathLikeParameter: id.ToString()
            );
        }

        public async Task<T?> CreateAsync(T entity)
        {
            return await Http.PostEnvelopeAsync<T>(
                controller: Controller, 
                action: "Create", 
                payload: entity
            );
        }

        public async Task<Hydra.DTOs.TableDTO?> GetCreateViewAsync()
        {
            var request = new Hydra.DTOs.TableDTO 
            { 
                Name = Controller,
                ViewType = Hydra.DTOs.ViewDTOs.ViewType.CreateView 
            };
            
            return await SelectAsync(request);
        }

        public async Task<Hydra.DTOs.TableDTO?> GetUpdateViewAsync(Guid id)
        {
            var request = new Hydra.DTOs.TableDTO 
            { 
                Name = Controller,
                ViewType = Hydra.DTOs.ViewDTOs.ViewType.EditView 
            };

            // Add filter for ID
            request.AlterOrAddMetaColumn(
                Hydra.DTOs.MetaColumnDTO.CreateColumnDTOWithEqualFilter(
                    name: "Id", 
                    alias: null, 
                    value: id
                )
            );
            
            return await SelectAsync(request);
        }

        public async Task<Hydra.DTOs.TableDTO?> GetDetailsViewAsync(Guid id)
        {
            var container = await Http.GetEnvelopeAsync<DetailsContainer>(
                controller: Controller,
                action: "Details",
                pathLikeParameter: id.ToString()
            );

            return container?.Table;
        }

        private class DetailsContainer
        {
            public Hydra.DTOs.TableDTO? Table { get; set; }
            public T? Item { get; set; }
        }
    }
}
