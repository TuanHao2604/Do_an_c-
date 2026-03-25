using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

public class RevenueService : IRevenueService
{
    private readonly IAppDbContext _db;
    public RevenueService(IAppDbContext db) => _db = db;

    public async Task<RevenueStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.Payments.AsQueryable();
        if (startDate.HasValue) query = query.Where(p => p.CreatedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(p => p.CreatedAt <= endDate.Value);

        var payments = await query.ToListAsync();

        return new RevenueStatistics
        {
            TotalRevenue = payments.Where(p => p.Status == "Completed").Sum(p => p.Amount),
            TotalPayments = payments.Count,
            CompletedPayments = payments.Count(p => p.Status == "Completed"),
            PendingPayments = payments.Count(p => p.Status == "Pending"),
            AveragePaymentAmount = payments.Count > 0 ? payments.Average(p => p.Amount) : 0,
        };
    }

    public async Task<PagedResponse<Payment>> GetPaymentsAsync(DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 10)
    {
        var query = _db.Payments.Include(p => p.User).Include(p => p.Package).AsQueryable();
        if (startDate.HasValue) query = query.Where(p => p.CreatedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(p => p.CreatedAt <= endDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<Payment>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
    }
}
