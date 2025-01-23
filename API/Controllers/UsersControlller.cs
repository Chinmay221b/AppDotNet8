using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(DataContext context) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<AppUser>> GetUsers()
    {
        var users = context.Users.ToArray();
        return users;
    }

    // [HttpGet("{id:int}")]
    // public ActionResult<AppUser> GetUsers(int id)
    // {
    //     var user = context.Users.Find(id);
    //     if (user == null) return NotFound();
    //     return user;
    // }
}