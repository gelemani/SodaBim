using B.Models;

namespace B.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<IEnumerable<Project>> GetProjectsByUserIdAsync(int userId);

        Task<Project?> GetByIdAsync(int id);
        Task CreateAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(int id);

        Task AddProjectFilesAsync(int projectId, IEnumerable<ProjectFile> files);

        Task<IEnumerable<ProjectFile>> GetProjectFilesAsync(int projectId);
        Task<ProjectFile?> GetProjectFileByIdAsync(int fileId);
        Task DeleteProjectFileAsync(int fileId);
        Task UpdateProjectFileAsync(ProjectFile file);

        // Project access management
        Task<IEnumerable<ProjectAccess>> GetProjectAccessAsync(int projectId);
        Task UpdateProjectAccessAsync(ProjectAccess access);
        Task RemoveProjectAccessAsync(int projectId, int userId);

        // Получение списка пользователей (для удобства проверки)
        Task<IEnumerable<User>> GetUsersAsync();

        // IFC Collisions
        Task<List<IfcCollision>> GetCollisionsByFileIdAsync(int fileId);
        Task AddCollisionsAsync(IEnumerable<IfcCollision> collisions);
        Task DeleteCollisionsByFileIdAsync(int fileId);
    }
}
