using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

public class PoiRequestService : IPoiRequestService
{
    private readonly IAppDbContext _db;
    public PoiRequestService(IAppDbContext db) => _db = db;

    public async Task<RequestCounts> GetRequestCountsAsync(Guid? userId = null)
    {
        var query = _db.PoiRequests.AsQueryable();
        if (userId.HasValue) query = query.Where(r => r.UserId == userId.Value);

        var all = await query.ToListAsync();
        return new RequestCounts
        {
            Pending = all.Count(r => r.Status == "Pending"),
            Approved = all.Count(r => r.Status == "Approved"),
            Rejected = all.Count(r => r.Status == "Rejected"),
            Total = all.Count,
        };
    }

    public async Task<PagedResponse<PoiRequest>> GetAllPagedAsync(string? status, int pageNumber, int pageSize)
    {
        var query = _db.PoiRequests.Include(r => r.User).AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<PoiRequest>
        {
            Items = items, TotalCount = totalCount,
            PageNumber = pageNumber, PageSize = pageSize,
        };
    }

    public async Task<PagedResponse<PoiRequest>> GetByUserPagedAsync(Guid userId, string? status, int pageNumber, int pageSize)
    {
        var query = _db.PoiRequests.Where(r => r.UserId == userId).AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<PoiRequest>
        {
            Items = items, TotalCount = totalCount,
            PageNumber = pageNumber, PageSize = pageSize,
        };
    }

    public async Task<PoiRequest?> GetByIdAsync(Guid id)
        => await _db.PoiRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<PoiRequest> SubmitRequestAsync(PoiRequest request, Guid userId)
    {
        request.Id = Guid.NewGuid();
        request.UserId = userId;
        request.Status = "Pending";
        request.CreatedAt = DateTime.UtcNow;
        _db.PoiRequests.Add(request);
        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<bool> ApproveAsync(Guid id, Guid adminId, string? note)
    {
        var request = await _db.PoiRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request is null || request.Status != "Pending") return false;

        request.Status = "Approved";
        request.ReviewedBy = adminId;
        request.AdminNote = note;
        request.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectAsync(Guid id, Guid adminId, string reason)
    {
        var request = await _db.PoiRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request is null || request.Status != "Pending") return false;

        request.Status = "Rejected";
        request.ReviewedBy = adminId;
        request.AdminNote = reason;
        request.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var request = await _db.PoiRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request is null) return false;
        if (!isAdmin && request.UserId != userId) return false;

        _db.PoiRequests.Remove(request);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateRequestAsync(Guid id, string requestData, Guid userId)
    {
        var request = await _db.PoiRequests.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId && r.Status == "Pending");
        if (request is null) return false;

        request.RequestData = requestData;
        await _db.SaveChangesAsync();
        return true;
    }
}
