using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(string? username, RegisterDto registerDto)
    {
        if (username != null)
        {
            return BadRequest("UserName is Already Exixts in Params !!");
        }
        if (await IsUserNameExixting(registerDto.UserName))
        {
            return BadRequest("UserName Already Exixts !!");
        }
        using var hmac = new HMACSHA512();
        var user = new AppUser
        {
            UserName = registerDto.UserName,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key,
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return new UserDto
        {
            Username = registerDto.UserName,
            Token = tokenService.CreateToken(user),
        };


    }

    private async Task<bool> IsUserNameExixting(string userName)
    {
        var isExixtingUser = await context.Users.AnyAsync(x => x.UserName.ToLower() == userName.ToLower());
        return isExixtingUser;
    }
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.UserName);

        if (user == null) return Unauthorized("Invalid User");

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        if (passwordHash != null && passwordHash.Length > 0 && user.PasswordHash != null && user.PasswordHash.Length > 0)
        {
            for (int i = 0; i < passwordHash?.Length; i++)
            {
                if (passwordHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Password Dose Not Match !");
                }
            }
        }
        else
        {
            return Unauthorized("Invalid User !!");
        }
        return Ok(new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user),
        });
    }

}
