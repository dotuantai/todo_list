using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_v2.Models.DTOs;

namespace API_v2.Services.Interfaces
{
    public interface IProjectColumnService
    {
        Task<List<ProjectColumnResponse>> GetColumnsAsync(Guid projectId, Guid userId);
        Task<ProjectColumnResponse> CreateColumnAsync(Guid projectId, CreateProjectColumnRequest req, Guid currentUserId);
        Task<ProjectColumnResponse> UpdateColumnAsync(int columnId, UpdateProjectColumnRequest req, Guid currentUserId);
        Task<string> DeleteColumnAsync(int columnId, Guid currentUserId);
    }
}
