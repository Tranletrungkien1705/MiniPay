using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services;

/// <summary>
/// Dịch vụ Lịch Làm Việc & Hạn Nộp Cọc (Payment / Deposit Duty Calendar).
/// Tương ứng bảng Mst_Calendar trong hệ nguồn 2010.HTC (BizHTC.MasterData.cs:
/// Mst_Calendar_Get / Mst_Calendar_GetForDepositDuty / Mst_Calendar_ResetYear /
/// Mst_Calendar_UpdateStatusValue; BizHTC.Common.cs: mySql_GetClauseSelect_Mst_Calendar_GetForDayT).
///
/// Nghiệp vụ: quản lý lịch làm việc theo từng ngày (CalendarType + Date + StatusValue) và dùng
/// lịch này để tính HẠN NỘP CỌC của đơn hàng mua xe: từ 1 ngày mốc, cộng thêm N ngày làm việc
/// (DayT - tham số CALENDAR.DEPOSITDUTY.DAYT) để ra ngày hạn nộp cọc, bỏ qua ngày nghỉ/lễ.
/// </summary>
public sealed class PaymentCalendarService(AppDbContext db)
{
    /// <summary>Loại lịch mặc định (TConst.CalendarType.WorkingDay).</summary>
    public const string WorkingDayType = "WORKINGDAY";

    /// <summary>Số ngày làm việc cộng thêm mặc định cho hạn nộp cọc (CALENDAR.DEPOSITDUTY.DAYT).</summary>
    public const int DefaultDepositDutyDayT = 7;

    /// <summary>
    /// Lấy danh sách ngày trong lịch làm việc kèm bộ lọc (loại lịch, khoảng ngày, trạng thái).
    /// Tương ứng Mst_Calendar_Get.
    /// </summary>
    public async Task<List<CalendarEntry>> GetListAsync(
        Guid orgId,
        string? calendarType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CalendarDayStatus? statusValue = null)
    {
        var q = db.CalendarEntries.Where(x => x.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(calendarType))
        {
            string ct = calendarType.Trim().ToUpperInvariant();
            q = q.Where(x => x.CalendarType == ct);
        }

        if (fromDate.HasValue)
            q = q.Where(x => x.Date >= fromDate.Value.Date);

        if (toDate.HasValue)
            q = q.Where(x => x.Date <= toDate.Value.Date);

        if (statusValue.HasValue)
            q = q.Where(x => x.StatusValue == statusValue.Value);

        return await q.OrderBy(x => x.Date).ToListAsync();
    }

    /// <summary>Lấy 1 ngày trong lịch theo (loại lịch, ngày).</summary>
    public async Task<CalendarEntry?> GetByDateAsync(Guid orgId, string calendarType, DateTime date)
    {
        string ct = calendarType.Trim().ToUpperInvariant();
        var d = date.Date;
        return await db.CalendarEntries.FirstOrDefaultAsync(x => x.OrgId == orgId && x.CalendarType == ct && x.Date == d);
    }

    /// <summary>
    /// Sinh lại toàn bộ lịch 1 năm theo trạng thái mặc định của từng thứ trong tuần.
    /// Tương ứng Mst_Calendar_ResetYear: xóa toàn bộ ngày trong năm rồi tạo lại theo DayOfWeek.
    /// </summary>
    public async Task<int> ResetYearAsync(Guid orgId, ResetCalendarYearDto dto)
    {
        string calendarType = string.IsNullOrWhiteSpace(dto.CalendarType)
            ? WorkingDayType
            : dto.CalendarType.Trim().ToUpperInvariant();

        int year = dto.Year;
        if (year < DateTime.Now.Year || year > 2100)
            throw new ArgumentException($"Năm {year} không hợp lệ (phải >= năm hiện tại {DateTime.Now.Year} và <= 2100).");

        var firstDay = new DateTime(year, 1, 1);
        var nextYearFirstDay = new DateTime(year + 1, 1, 1);

        // Xóa toàn bộ ngày trong năm của loại lịch này (Clear Old Data).
        var old = await db.CalendarEntries
            .Where(x => x.OrgId == orgId && x.CalendarType == calendarType
                && x.Date >= firstDay && x.Date < nextYearFirstDay)
            .ToListAsync();
        if (old.Count > 0)
            db.CalendarEntries.RemoveRange(old);

        // Bảng trạng thái mặc định theo thứ trong tuần.
        var byDayOfWeek = new Dictionary<DayOfWeek, CalendarDayStatus>
        {
            [DayOfWeek.Monday] = dto.Monday,
            [DayOfWeek.Tuesday] = dto.Tuesday,
            [DayOfWeek.Wednesday] = dto.Wednesday,
            [DayOfWeek.Thursday] = dto.Thursday,
            [DayOfWeek.Friday] = dto.Friday,
            [DayOfWeek.Saturday] = dto.Saturday,
            [DayOfWeek.Sunday] = dto.Sunday
        };

        var now = DateTime.Now;
        string createdBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "Admin_HTC" : dto.CreatedBy.Trim();

        int count = 0;
        for (var scan = firstDay; scan < nextYearFirstDay; scan = scan.AddDays(1))
        {
            db.CalendarEntries.Add(new CalendarEntry
            {
                OrgId = orgId,
                CalendarType = calendarType,
                Date = scan,
                StatusValue = byDayOfWeek[scan.DayOfWeek],
                CreatedBy = createdBy,
                CreatedAt = now
            });
            count++;
        }

        await db.SaveChangesAsync();
        return count;
    }

    /// <summary>
    /// Đổi trạng thái 1 ngày trong lịch (đánh dấu nghỉ/lễ hoặc làm việc).
    /// Tương ứng Mst_Calendar_UpdateStatusValue.
    /// </summary>
    public async Task<CalendarEntry> UpdateStatusValueAsync(Guid orgId, UpdateCalendarDayDto dto)
    {
        string calendarType = string.IsNullOrWhiteSpace(dto.CalendarType)
            ? WorkingDayType
            : dto.CalendarType.Trim().ToUpperInvariant();
        var date = dto.Date.Date;

        var entity = await db.CalendarEntries
            .FirstOrDefaultAsync(x => x.OrgId == orgId && x.CalendarType == calendarType && x.Date == date)
            ?? throw new KeyNotFoundException($"Không tìm thấy ngày {date:dd/MM/yyyy} trong lịch {calendarType}.");

        entity.StatusValue = dto.StatusValue;
        if (dto.Remark != null) entity.Remark = dto.Remark.Trim();
        entity.UpdatedBy = string.IsNullOrWhiteSpace(dto.UpdatedBy) ? "Admin_HTC" : dto.UpdatedBy.Trim();
        entity.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Tính hạn nộp cọc cho các ngày làm việc kể từ ngày mốc (fromDate).
    /// Tương ứng Mst_Calendar_GetForDepositDuty + mySql_GetClauseSelect_Mst_Calendar_GetForDayT:
    /// với mỗi ngày làm việc, hạn nộp cọc = ngày làm việc thứ (DayT) kế tiếp (bỏ qua ngày nghỉ).
    /// </summary>
    public async Task<List<DepositDutyResultDto>> GetForDepositDutyAsync(
        Guid orgId,
        DateTime fromDate,
        int? dayT = null)
    {
        int n = dayT ?? DefaultDepositDutyDayT;
        if (n < 0) n = 0;

        var from = fromDate.Date;

        // Chỉ lấy các ngày LÀM VIỆC (StatusValue = WorkingDay) từ ngày mốc trở đi, sắp theo ngày.
        var workingDays = await db.CalendarEntries
            .Where(x => x.OrgId == orgId && x.CalendarType == WorkingDayType
                && x.StatusValue == CalendarDayStatus.WorkingDay && x.Date >= from)
            .OrderBy(x => x.Date)
            .Select(x => x.Date)
            .ToListAsync();

        var result = new List<DepositDutyResultDto>();
        for (int i = 0; i < workingDays.Count; i++)
        {
            int targetIdx = i + n;
            result.Add(new DepositDutyResultDto
            {
                WorkingDate = workingDays[i],
                DepositDutyDate = targetIdx < workingDays.Count ? workingDays[targetIdx] : null,
                DayT = n
            });
        }

        return result;
    }

    /// <summary>Báo cáo tổng hợp lịch làm việc (số ngày làm việc / nghỉ, thống kê theo tháng).</summary>
    public async Task<CalendarSummaryDto> GetSummaryAsync(Guid orgId, int? year = null)
    {
        int y = year ?? DateTime.Now.Year;
        var from = new DateTime(y, 1, 1);
        var to = new DateTime(y, 12, 31);

        var list = await db.CalendarEntries
            .Where(x => x.OrgId == orgId && x.CalendarType == WorkingDayType && x.Date >= from && x.Date <= to)
            .ToListAsync();

        var summary = new CalendarSummaryDto
        {
            TotalDays = list.Count,
            WorkingDayCount = list.Count(x => x.StatusValue == CalendarDayStatus.WorkingDay),
            HolidayCount = list.Count(x => x.StatusValue == CalendarDayStatus.Holiday),
            Year = y,
            DayT = DefaultDepositDutyDayT
        };

        summary.ByMonth = list
            .GroupBy(x => x.Date.Month)
            .Select(g => new CalendarMonthStatDto
            {
                Month = g.Key,
                WorkingDayCount = g.Count(x => x.StatusValue == CalendarDayStatus.WorkingDay),
                HolidayCount = g.Count(x => x.StatusValue == CalendarDayStatus.Holiday)
            })
            .OrderBy(x => x.Month)
            .ToList();

        return summary;
    }
}
