using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.GenericTable;
using api_custo_prato.src.Shared.DTOs;
using api_custo_prato.src.Shared.Utils;
using api_custo_prato.src.Utils;
using MongoDB.Bson;

namespace api_custo_prato.src.Services
{
    public class GenericTableService(
        IGenericTableRepository repository
    ) : IGenericTableService
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllDTO request)
        {
            try
            {
                PaginationUtil<GenericTable> pagination = new(request.QueryParams);
                List<BsonDocument> pipeline = [
                    new("$match", pagination.PipelineFilter),
                    new("$project", new BsonDocument {
                        {"_id", 0},
                        {"id", MongoUtil.ToString("$_id")},
                        {"code", 1},
                        {"name", 1}
                    })
                ];

                List<dynamic> entities = await repository.GetSelectAsync(pipeline);
                return new(entities, 200, "Operações listados com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<GenericTable?>> CreateAsync(CreateGenericTableRequest request)
        {
            try
            {
                GenericTable GenericTable = ObjectMapper.Map<CreateGenericTableRequest, GenericTable>(request);

                GenericTable? response = await repository.CreateAsync(GenericTable);
                if (response is null) return new(null, 400, "Falha ao salvar anexo.");

                return new(response, 201, "Anexo criado com sucesso.");
            }
            catch
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }

        #endregion

        #region UPDATE
        public async Task<ResponseApi<GenericTable?>> UpdateAsync(UpdateGenericTableRequest request)
        {
            try
            {
                GenericTable? existed = await repository.GetByIdAsync(request.Id);
                if (existed is null) return new(null, 404, "Falha ao atualizar");

                existed.UpdatedAt = DateTime.UtcNow;
                existed.UpdatedBy = request.UpdatedBy;

                GenericTable? response = await repository.UpdateAsync(existed);
                if (response is null) return new(null, 400, "Falha ao atualizar");

                return new(response, 200, "Atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        // #region DELETE
        // public async Task<ResponseApi<GenericTable>> DeleteAsync(DeleteDTO request)
        // {
        //     try
        //     {
        //         ResponseApi<GenericTable> GenericTable = await repository.DeleteAsync(request);
        //         if (!GenericTable.IsSuccess) return new(null, 400, GenericTable.Message);
        //         return new(GenericTable.Data, 204, "Anexo excluído com sucesso");
        //     }
        //     catch (Exception ex)
        //     {
        //         return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
        //     }
        // }
        // #endregion        
    }
}