using SmartTourApp.Domain.Entities;

namespace SmartTourApp.Domain.Interfaces;

public class RequestCounts
{
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Total { get; set; }
}

public interface IPoiRequestService
{
    Task<RequestCounts> GetRequestCountsAsync(Guid? userId = null);
    Task<PagedResponse<PoiRequest>> GetAllPagedAsync(string? status, int pageNumber, int pageSize);
    Task<PagedResponse<PoiRequest>> GetByUserPagedAsync(Guid userId, string? status, int pageNumber, int pageSize);
    Task<PoiRequest?> GetByIdAsync(Guid id);
    Task<PoiRequest> SubmitRequestAsync(PoiRequest request, Guid userId);
    Task<bool> ApproveAsync(Guid id, Guid adminId, string? note);
    Task<bool> RejectAsync(Guid id, Guid adminId, string reason);
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    Task<bool> UpdateRequestAsync(Guid id, string requestData, Guid userId);
}
