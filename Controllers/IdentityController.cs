namespace Exam.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Exam.Dtos;
using Exam.Data;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Exam.Options;
using Exam.Entity;

[ApiController]
public class IdentityController : ControllerBase
{
    private readonly JwtOptions jwtOptions;
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly MyDbContext dbContext;

    public IdentityController(
        IOptionsSnapshot<JwtOptions> jwtOptionsSnapshot,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole> roleManager,
        MyDbContext dbContext
        )
    {
        this.jwtOptions = jwtOptionsSnapshot.Value;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.roleManager = roleManager;
        this.dbContext = dbContext;
    }

    [HttpPost("api/[controller]/[action]")]
    public async Task<IActionResult> SignIn(LoginDto dto) {
        var foundUser = await userManager.FindByEmailAsync(dto.Login);

        if(foundUser == null) {
            return base.BadRequest("Incorrect Login or Password");
        }

        var signInResult = await this.signInManager.PasswordSignInAsync(foundUser, dto.Password, true, true);

        if(signInResult.IsLockedOut) {
            return base.BadRequest("User locked");
        }

        if(signInResult.Succeeded == false) {
            return base.BadRequest("Incorrect Login or Password");
        }

        var roles = await userManager.GetRolesAsync(foundUser);

        var claims = roles
            .Select(roleStr => new Claim(ClaimTypes.Role, roleStr))
            .Append(new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString()))
            .Append(new Claim(ClaimTypes.Email, foundUser.Email ?? "not set"))
            .Append(new Claim(ClaimTypes.Name, foundUser.UserName ?? "not set"));

        var signingKey = new SymmetricSecurityKey(jwtOptions.KeyInBytes);
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddYears(1),
            signingCredentials: signingCredentials
        );

        var handler = new JwtSecurityTokenHandler();
        var tokenStr = handler.WriteToken(token);

        return Ok(new {
            jwt = tokenStr
        });
    }

    [HttpPost("api/[controller]/[action]")]
    public async Task<IActionResult> SignUp(RegistrationDto dto) {
        var newUser = new User() {
            Email = dto.Email,
            UserName = dto.Username,
            AvatarUrl = "https://emilbabayevstorage.blob.core.windows.net/emilcontainer/default.jpg"
        };

        var result = await userManager.CreateAsync(newUser, dto.Password);

        if(result.Succeeded == false) {
            return base.BadRequest(result.Errors);
        }

        if ((await roleManager.RoleExistsAsync("user")) == false)
        {
            await roleManager.CreateAsync(new IdentityRole("user"));
        }
        await userManager.AddToRoleAsync(newUser, "user");

        return Ok();
    }
}