using CBTW_TEST.Domain.Models.Dto;

namespace CBTW_TEST.Domain.Interfaces
{
    public interface ILibraryDiscoveryService
    {
        Task<WorkflowResultDto> ExecuteDiscoveryAsync(string messyBlob);
    }
}