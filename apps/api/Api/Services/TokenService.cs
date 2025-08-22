using System.IdentityModel.Tokens.Jwt; using System.Security.Claims; using Microsoft.IdentityModel.Tokens; using System.Text; using Api.Models;
namespace Api.Services{
  public class TokenService{
    private readonly IConfiguration _cfg; public TokenService(IConfiguration cfg)=>_cfg=cfg;
    public string CreateAccessToken(AppUser user, IList<string> roles){
      var claims = new List<Claim>{
        new(JwtRegisteredClaimNames.Sub, user.Id),
        new(JwtRegisteredClaimNames.Email, user.Email??""),
        new(ClaimTypes.Name, user.UserName??"")
      };
      foreach(var r in roles) claims.Add(new Claim(ClaimTypes.Role,r));
      var jwt=_cfg.GetSection("Jwt"); var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
      var creds=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
      var token=new JwtSecurityToken(issuer:jwt["Issuer"],audience:jwt["Audience"],claims:claims,
        expires:DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessMinutes"]!)), signingCredentials:creds);
      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
