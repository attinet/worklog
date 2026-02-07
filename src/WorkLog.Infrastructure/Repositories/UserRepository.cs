using Microsoft.EntityFrameworkCore;
using WorkLog.Domain.Entities;
using WorkLog.Domain.Interfaces;
using WorkLog.Infrastructure.Data;

namespace WorkLog.Infrastructure.Repositories;

/// <summary>
/// 使用者 Repository 實作
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FindAsync(id);

    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IReadOnlyList<User>> GetAllAsync()
        => await _context.Users.OrderBy(u => u.Id).AsNoTracking().ToListAsync();

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
