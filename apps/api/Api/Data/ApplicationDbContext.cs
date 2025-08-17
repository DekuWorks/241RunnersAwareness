using Microsoft.AspNetCore.Identity.EntityFrameworkCore; using Microsoft.EntityFrameworkCore;
using Api.Models;
namespace Api.Data{
  public class RefreshToken{ public int Id{get;set;} public string UserId{get;set;}=""; public string Token{get;set;}=""; public DateTime ExpiresAt{get;set;} public DateTime? RevokedAt{get;set;} public string? ReplacedByToken{get;set;} public string Device{get;set;}="";}
  public class ApplicationDbContext:IdentityDbContext<AppUser, Microsoft.AspNetCore.Identity.IdentityRole, string>{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opt):base(opt){}
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
  }
}
