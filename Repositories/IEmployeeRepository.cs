// Repositories/IEmployeeRepository.cs
using ms_notification.Models;

namespace ms_notification.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByCinAsync(string cin);
}