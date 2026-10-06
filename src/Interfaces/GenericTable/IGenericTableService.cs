using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.GenericTable;
using api_custo_prato.src.Shared.DTOs;

namespace api_custo_prato.src.Interfaces
{
    public interface IGenericTableService
    {
        Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllDTO request);
        Task<ResponseApi<GenericTable?>> CreateAsync(CreateGenericTableRequest request);
        Task<ResponseApi<GenericTable?>> UpdateAsync(UpdateGenericTableRequest request);
        Task<ResponseApi<dynamic?>> DeleteAsync(string id, string userId);
    }
}