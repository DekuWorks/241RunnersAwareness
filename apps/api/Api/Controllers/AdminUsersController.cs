using Api.Models; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers{
  [ApiController][Route("api/admin/users")][Authorize(Roles="Admin,Manager")]
  public class AdminUsersController:ControllerBase{
    private readonly UserManager<AppUser> _users; public AdminUsersController(UserManager<AppUser> u){_users=u;}
    [HttpGet] public IActionResult List(string? q=null,int page=1,int pageSize=20){
      var baseQ=_users.Users.AsQueryable();
      if(!string.IsNullOrWhiteSpace(q)) baseQ=baseQ.Where(u=>(u.Email??"").Contains(q)||(u.DisplayName??"").Contains(q));
      var total=baseQ.Count(); var items=baseQ.OrderBy(u=>u.Email).Skip((page-1)*pageSize).Take(pageSize)
        .Select(u=> new{u.Id,u.Email,u.DisplayName,u.IsDisabled}).ToList();
      return Ok(new{total,items});
    }
    [HttpPost("{id}/disable")] public async Task<IActionResult> Disable(string id){ var u=await _users.FindByIdAsync(id); if(u==null) return NotFound(); u.IsDisabled=true; await _users.UpdateAsync(u); return Ok(); }
    [HttpPost("{id}/roles")] public async Task<IActionResult> SetRoles(string id,[FromBody] string[] roles){ var u=await _users.FindByIdAsync(id); if(u==null) return NotFound(); var cur=await _users.GetRolesAsync(u); await _users.RemoveFromRolesAsync(u,cur); await _users.AddToRolesAsync(u,roles); return Ok(); }
  }
}
