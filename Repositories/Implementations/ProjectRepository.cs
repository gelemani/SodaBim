using Microsoft.EntityFrameworkCore;
using B.Models;
using B.Repositories.Interfaces;
using B.Data;

namespace B.Repositories.Implementations
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly DatabaseContext _context;

        public ProjectRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects
                .Include(p => p.Creator)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(int userId)
        {
            return await _context.Projects
                .Include(p => p.Creator)
                .Include(p => p.ProjectAccesses)
                .Where(p => p.CreatorId == userId || p.ProjectAccesses.Any(pa => pa.UserId == userId))
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task CreateAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Project project)
        {
            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddProjectFilesAsync(int projectId, IEnumerable<ProjectFile> files)
        {
            foreach (var file in files)
            {
                file.ProjectId = projectId;
                file.CreatedAt = DateTime.UtcNow;
                file.LastModified = DateTime.UtcNow;
                _context.ProjectFiles.Add(file);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProjectFile>> GetProjectFilesAsync(int projectId)
        {
            if (projectId == 0)
            {
                // Return all files if projectId is 0 (used for file download by fileId)
                return await _context.ProjectFiles.ToListAsync();
            }
            return await _context.ProjectFiles
                .Where(f => f.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<ProjectFile?> GetProjectFileByIdAsync(int fileId)
        {
            return await _context.ProjectFiles.FindAsync(fileId);
        }

        public async Task DeleteProjectFileAsync(int fileId)
        {
            var file = await _context.ProjectFiles.FindAsync(fileId);
            if (file != null)
            {
                _context.ProjectFiles.Remove(file);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateProjectFileAsync(ProjectFile file)
        {
            file.LastModified = DateTime.UtcNow;
            _context.Entry(file).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProjectAccess>> GetProjectAccessAsync(int projectId)
        {
            return await _context.ProjectAccesses
                .Include(pa => pa.User)
                .Where(pa => pa.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task UpdateProjectAccessAsync(ProjectAccess access)
        {
            var existingAccess = await _context.ProjectAccesses
                .FirstOrDefaultAsync(pa => pa.ProjectId == access.ProjectId && pa.UserId == access.UserId);

            if (existingAccess != null)
            {
                existingAccess.AccessLevel = access.AccessLevel;
                existingAccess.GrantedAt = DateTime.UtcNow;
                _context.Entry(existingAccess).State = EntityState.Modified;
            }
            else
            {
                access.GrantedAt = DateTime.UtcNow;
                _context.ProjectAccesses.Add(access);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveProjectAccessAsync(int projectId, int userId)
        {
            var access = await _context.ProjectAccesses
                .FirstOrDefaultAsync(pa => pa.ProjectId == projectId && pa.UserId == userId);

            if (access != null)
            {
                _context.ProjectAccesses.Remove(access);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<List<IfcCollision>> GetCollisionsByFileIdAsync(int fileId)
        {
            return await _context.IfcCollisions.Where(c => c.FileId == fileId).ToListAsync();
        }

        public async Task AddCollisionsAsync(IEnumerable<IfcCollision> collisions)
        {
            _context.IfcCollisions.AddRange(collisions);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCollisionsByFileIdAsync(int fileId)
        {
            var collisions = _context.IfcCollisions.Where(c => c.FileId == fileId);
            _context.IfcCollisions.RemoveRange(collisions);
            await _context.SaveChangesAsync();
        }
    }
}
