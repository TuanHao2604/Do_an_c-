using SmartTourApp.Domain.Entities;

namespace SmartTourApp.Domain.Interfaces;

public class RevenueStatistics
{
    public decimal TotalRevenue { get; set; }
    public int TotalPayments { get; set; }
    public int CompletedPayments { get; set; }
    public int PendingPayments { get; set; }
    public decimal AveragePaymentAmount { get; set; }
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public interface IRevenueService
{
    Task<RevenueStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<PagedResponse<Payment>> GetPaymentsAsync(DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 10);
}
