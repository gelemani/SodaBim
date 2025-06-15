using Microsoft.EntityFrameworkCore;
using B.Data;
using B.Models;
using B.Repositories.Interfaces;

namespace B.Repositories.Implementations
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DatabaseContext _context;

        public CommentRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByFileIdAsync(int fileId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.FileId == fileId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(Comment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;
            comment.LastModified = DateTime.UtcNow;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Comment comment)
        {
            comment.LastModified = DateTime.UtcNow;
            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }
    }
} 