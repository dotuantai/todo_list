using System.Threading.Tasks;
using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardResponse> GetDashboardStatsAsync();
    }
}
