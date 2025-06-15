using Microsoft.AspNetCore.Mvc;
using B.Models;
using B.Repositories.Interfaces;
using System.Security.Claims;
using Xbim.Ifc;

namespace B.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICommentRepository _commentRepository;

        public ProjectController(IProjectRepository projectRepository, ICommentRepository commentRepository)
        {
            _projectRepository = projectRepository;
            _commentRepository = commentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int userId)
        {
            var projects = await _projectRepository.GetProjectsByUserIdAsync(userId);
            var result = projects.Select(p => new {
                p.Id,
                p.CreatorId,
                Creator = p.Creator,
                p.Title,
                p.CreatedAt,
                p.LastModified,
                AccessLevel = p.CreatorId == userId ? "Admin" : p.AccessLevel,
                p.ProjectAccesses,
                p.ProjectFiles
            });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
                return NotFound();
            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Project project)
        {
            // Convert projectAccessCreate to ProjectAccesses
            if (project.ProjectAccesses == null)
            {
                project.ProjectAccesses = new List<ProjectAccess>();
            }

            // Add creator as admin if not already in the list
            if (!project.ProjectAccesses.Any(pa => pa.UserId == project.CreatorId))
            {
                project.ProjectAccesses.Add(new ProjectAccess
                {
                    ProjectId = project.Id,
                    UserId = project.CreatorId,
                    AccessLevel = "Admin",
                    GrantedAt = DateTime.UtcNow
                });
            }

            await _projectRepository.CreateAsync(project);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Project project)
        {
            if (id != project.Id)
                return BadRequest();
            await _projectRepository.UpdateAsync(project);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _projectRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{projectId}/files")]
        public async Task<IActionResult> UploadFiles(int projectId, [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            var projectFiles = new List<ProjectFile>();

            foreach (var file in files)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                var projectFile = new ProjectFile
                {
                    FileName = file.FileName,
                    FileData = fileBytes,
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType) 
                        ? "application/octet-stream" 
                        : file.ContentType,
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    ProjectId = projectId
                };

                projectFiles.Add(projectFile);
            }

            await _projectRepository.AddProjectFilesAsync(projectId, projectFiles);

            foreach (var projectFile in projectFiles)
            {
                if (Path.GetExtension(projectFile.FileName).ToLowerInvariant() == ".ifc")
                {
                    // Заглушка: анализ IFC-файла (реализация через Xbim)
                    await _projectRepository.DeleteCollisionsByFileIdAsync(projectFile.Id);
                    var collisions = new List<IfcCollision>();
                    // TODO: Реализовать анализ через Xbim и заполнить collisions
                    // collisions = IfcCollisionService.FindCollisions(projectFile.FileData);
                    await _projectRepository.AddCollisionsAsync(collisions);
                }
            }

            return Ok(new { Message = $"{files.Count} files uploaded successfully." });
        }

        [HttpGet("{projectId}/files")]
        public async Task<IActionResult> GetFiles(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            var files = await _projectRepository.GetProjectFilesAsync(projectId);

            var fileDtos = files.Select(f => new
            {
                f.Id,
                f.FileName,
                f.CreatedAt,
                f.LastModified
            });

            return Ok(fileDtos);
        }

        [HttpGet("{projectId}/files/download")]
        public async Task<IActionResult> DownloadFiles(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            var files = await _projectRepository.GetProjectFilesAsync(projectId);

            using (var memoryStream = new System.IO.MemoryStream())
            {
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var zipEntry = archive.CreateEntry(file.FileName, System.IO.Compression.CompressionLevel.Fastest);
                        using (var zipStream = zipEntry.Open())
                        {
                            await zipStream.WriteAsync(file.FileData, 0, file.FileData.Length);
                        }
                    }
                }

                memoryStream.Position = 0;
                var zipFileName = $"project_{projectId}_files.zip";
                return File(memoryStream.ToArray(), "application/zip", zipFileName);
            }
        }

        [HttpGet("files/{fileId}/download")]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            var file = await _projectRepository.GetProjectFileByIdAsync(fileId);
            if (file == null)
            {
                return NotFound($"File with ID {fileId} not found.");
            }

            var contentType = string.IsNullOrWhiteSpace(file.ContentType) 
                ? "application/octet-stream" 
                : file.ContentType;

            var collisions = await _projectRepository.GetCollisionsByFileIdAsync(fileId);

            return File(file.FileData, contentType, file.FileName);
        }

        [HttpDelete("files/{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            var file = await _projectRepository.GetProjectFileByIdAsync(fileId);
            if (file == null)
            {
                return NotFound($"File with ID {fileId} not found.");
            }

            await _projectRepository.DeleteProjectFileAsync(fileId);
            return NoContent();
        }

        [HttpPut("files/{fileId}")]
        public async Task<IActionResult> UpdateFile(int fileId, [FromForm] List<IFormFile> newFile)
        {
            IFormFile file = newFile[0];
            if (file == null || newFile.Count == 0)
            {
                return BadRequest("No file provided.");
            }
            if (newFile.Count > 1)
            {
                return BadRequest("Only one file allowed.");
            }

            var existingFile = await _projectRepository.GetProjectFileByIdAsync(fileId);
            if (existingFile == null)
            {
                return NotFound($"File with ID {fileId} not found.");
            }

            // Проверяем расширение файла
            var existingExtension = Path.GetExtension(existingFile.FileName).ToLowerInvariant();
            var newExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (existingExtension != newExtension)
            {
                return BadRequest($"File extension must be the same. Expected: {existingExtension}, got: {newExtension}");
            }

            // Читаем содержимое нового файла
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            // Обновляем только содержимое файла, сохраняя оригинальное имя и projectId
            existingFile.FileData = fileBytes;
            existingFile.ContentType = string.IsNullOrWhiteSpace(file.ContentType) 
                ? "application/octet-stream" 
                : file.ContentType;
            existingFile.LastModified = DateTime.UtcNow;

            await _projectRepository.UpdateProjectFileAsync(existingFile);
            return Ok(new { Message = "File updated successfully.", FileId = existingFile.Id, FileName = existingFile.FileName });
        }

        [HttpPut("files/{fileId}/rename")]
        public async Task<IActionResult> RenameFile(int fileId, [FromBody] RenameFileRequest request)
        {
            var existingFile = await _projectRepository.GetProjectFileByIdAsync(fileId);
            if (existingFile == null)
            {
                return NotFound($"File with ID {fileId} not found.");
            }

            if (string.IsNullOrWhiteSpace(request.NewFileName))
            {
                return BadRequest("New file name cannot be empty.");
            }

            // Если новое имя не содержит расширение, добавляем его из текущего имени файла
            var newFileName = !Path.HasExtension(request.NewFileName)
                ? request.NewFileName + Path.GetExtension(existingFile.FileName)
                : request.NewFileName;

            // Проверяем, что расширение файла не изменилось
            var existingExtension = Path.GetExtension(existingFile.FileName).ToLowerInvariant();
            var newExtension = Path.GetExtension(newFileName).ToLowerInvariant();
            
            if (existingExtension != newExtension)
            {
                return BadRequest($"File extension must be the same. Expected: {existingExtension}, got: {newExtension}");
            }

            existingFile.FileName = newFileName;
            existingFile.LastModified = DateTime.UtcNow;

            await _projectRepository.UpdateProjectFileAsync(existingFile);
            return Ok(new { Message = "File renamed successfully.", FileName = newFileName });
        }

        [HttpPost("{projectId}/access")]
        public async Task<IActionResult> UpdateProjectAccess(int projectId, [FromBody] UpdateProjectAccessRequest request)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            // Get current user ID from claims
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Check if current user is the creator
            if (project.CreatorId != currentUserId)
            {
                return StatusCode(403, "Only project creator can manage access.");
            }

            var projectAccess = new ProjectAccess
            {
                ProjectId = projectId,
                UserId = request.UserId,
                AccessLevel = request.AccessLevel
            };

            await _projectRepository.UpdateProjectAccessAsync(projectAccess);
            return Ok(new { Message = "Project access updated successfully." });
        }

        [HttpDelete("{projectId}/access/{userId}")]
        public async Task<IActionResult> RemoveProjectAccess(int projectId, int userId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            // Get current user ID from claims
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Check if current user is the creator
            if (project.CreatorId != currentUserId)
            {
                return StatusCode(403, "Only project creator can manage access.");
            }

            await _projectRepository.RemoveProjectAccessAsync(projectId, userId);
            return NoContent();
        }

        [HttpGet("{projectId}/access")]
        public async Task<IActionResult> GetProjectAccess(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            var accessList = await _projectRepository.GetProjectAccessAsync(projectId);
            return Ok(accessList);
        }

        [HttpPost("files/{fileId}/comments")]
        public async Task<IActionResult> AddComment(int fileId, [FromBody] CommentRequest request)
        {
            var file = await _projectRepository.GetProjectFileByIdAsync(fileId);
            if (file == null)
            {
                return NotFound($"File with ID {fileId} not found.");
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var comment = new Comment
            {
                FileId = fileId,
                UserId = currentUserId,
                Text = request.Text,
                ElementName = request.ElementName,
                ElementId = request.ElementId
            };

            await _commentRepository.CreateAsync(comment);
            return CreatedAtAction(nameof(GetComment), new { commentId = comment.Id }, comment);
        }

        [HttpGet("comments/{commentId}")]
        public async Task<IActionResult> GetComment(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound($"Comment with ID {commentId} not found.");
            }
            return Ok(comment);
        }

        [HttpPut("comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] CommentRequest request)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound($"Comment with ID {commentId} not found.");
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (comment.UserId != currentUserId)
            {
                return StatusCode(403, "Only comment author can update the comment.");
            }

            comment.Text = request.Text;
            comment.ElementName = request.ElementName;
            comment.ElementId = request.ElementId;
            comment.LastModified = DateTime.UtcNow;

            await _commentRepository.UpdateAsync(comment);
            return Ok(comment);
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound($"Comment with ID {commentId} not found.");
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (comment.UserId != currentUserId)
            {
                return StatusCode(403, "Only comment author can delete the comment.");
            }

            await _commentRepository.DeleteAsync(commentId);
            return NoContent();
        }

        [HttpGet("files/{fileId}/comments")]
        public async Task<IActionResult> GetFileComments(int fileId)
        {
            var file = await _projectRepository.GetProjectFileByIdAsync(fileId);
            if (file == null)
            {
                return NotFound($"File with ID {fileId} not found.");
            }

            var comments = await _commentRepository.GetCommentsByFileIdAsync(fileId);
            return Ok(comments);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetProjectsList([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var projects = await _projectRepository.GetAllAsync();
            var total = projects.Count();
            var pagedProjects = projects.Skip(skip).Take(take).Select(p => new { p.Id, p.Title, CreatorId = p.CreatorId, p.CreatedAt, p.LastModified });
            return Ok(new { Total = total, Projects = pagedProjects });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsersList([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var users = await _projectRepository.GetUsersAsync();
            var total = users.Count();
            var pagedUsers = users.Skip(skip).Take(take).Select(u => new { u.Id, u.Login, u.Email, u.UserName, u.UserSurname });
            return Ok(new { Total = total, Users = pagedUsers });
        }

        [HttpGet("files/{fileId}/collisions")]
        public async Task<IActionResult> GetFileCollisions(int fileId)
        {
            var file = await _projectRepository.GetProjectFileByIdAsync(fileId);
            if (file == null)
            {
                return NotFound($"File with ID {fileId} not found.");
            }
            var collisions = await _projectRepository.GetCollisionsByFileIdAsync(fileId);
            return Ok(collisions);
        }
    }

    public class RenameFileRequest
    {
        public string NewFileName { get; set; } = string.Empty;
    }

    public class UpdateProjectAccessRequest
    {
        public int UserId { get; set; }
        public string AccessLevel { get; set; } = "View";
    }

    public class CommentRequest
    {
        public string Text { get; set; } = string.Empty;
        public string ElementName { get; set; } = string.Empty;
        public int ElementId { get; set; }
    }
}
