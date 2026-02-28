using Microsoft.EntityFrameworkCore;
using ms_notification.Models;
using ms_notification.Persistence;

namespace ms_notification.Repositories;

public class EmployeeRepository:IEmployeeRepository
{
    private readonly NotificationDbContext _context;

    public EmployeeRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByCinAsync(string cin)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Cin == cin);
    }
}
