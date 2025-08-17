using Api.Data; using Api.Models; using Api.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc; using System.IdentityModel.Tokens.Jwt;

namespace Api.Controllers{
  [ApiController][Route("api/[controller]")]
  public class AuthController:ControllerBase{
    private readonly UserManager<AppUser> _users; private readonly SignInManager<AppUser> _signIn; private readonly TokenService _tokens; private readonly ApplicationDbContext _db;
    public AuthController(UserManager<AppUser> u,SignInManager<AppUser> s,TokenService t,ApplicationDbContext db){_users=u;_signIn=s;_tokens=t;_db=db;}

    [HttpPost("register")] public async Task<IActionResult> Register(RegisterDto dto){
      var u=new AppUser{UserName=dto.Email,Email=dto.Email,DisplayName=dto.DisplayName};
      var res=await _users.CreateAsync(u,dto.Password); if(!res.Succeeded) return BadRequest(res.Errors);
      await _users.AddToRoleAsync(u,"User"); return Ok();
    }

    [HttpPost("login")] public async Task<IActionResult> Login(LoginDto dto){
      var u=await _users.FindByEmailAsync(dto.Email); if(u==null||u.IsDisabled) return Unauthorized();
      var ok=await _signIn.CheckPasswordSignInAsync(u,dto.Password,true); if(!ok.Succeeded) return Unauthorized();
      var roles=await _users.GetRolesAsync(u); var access=_tokens.CreateAccessToken(u,roles); var refresh=Guid.NewGuid().ToString("N");
      _db.RefreshTokens.Add(new RefreshToken{UserId=u.Id,Token=refresh,ExpiresAt=DateTime.UtcNow.AddDays(14),Device=dto.Device??"web"}); await _db.SaveChangesAsync();
      return Ok(new{accessToken=access,refreshToken=refresh});
    }

    [HttpPost("refresh")] public async Task<IActionResult> Refresh(RefreshDto dto){
      var rt=_db.RefreshTokens.SingleOrDefault(x=>x.Token==dto.RefreshToken);
      if(rt==null||rt.ExpiresAt<=DateTime.UtcNow||rt.RevokedAt!=null) return Unauthorized();
      var u=await _users.FindByIdAsync(rt.UserId); if(u==null||u.IsDisabled) return Unauthorized();
      var roles=await _users.GetRolesAsync(u); var access=_tokens.CreateAccessToken(u,roles); var newRt=Guid.NewGuid().ToString("N");
      rt.RevokedAt=DateTime.UtcNow; rt.ReplacedByToken=newRt; _db.RefreshTokens.Add(new RefreshToken{UserId=u.Id,Token=newRt,ExpiresAt=DateTime.UtcNow.AddDays(14),Device=rt.Device}); await _db.SaveChangesAsync();
      return Ok(new{accessToken=access,refreshToken=newRt});
    }

    [HttpGet("test")] public IActionResult Test(){
      return Ok(new{ message="API is working!", timestamp=DateTime.UtcNow });
    }

    [Authorize][HttpGet("me")] public async Task<IActionResult> Me(){
      var id=User.Claims.First(c=>c.Type==JwtRegisteredClaimNames.Sub).Value; var u=await _users.FindByIdAsync(id); var roles=await _users.GetRolesAsync(u!);
      return Ok(new{ user=new{ u!.Email, u.DisplayName, roles } });
    }

    public record RegisterDto(string Email,string Password,string DisplayName);
    public record LoginDto(string Email,string Password,string? Device);
    public record RefreshDto(string RefreshToken);
  }
}
