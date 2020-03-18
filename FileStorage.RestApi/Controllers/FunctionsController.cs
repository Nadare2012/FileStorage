using AutoMapper;
using FileStorage.RestApi.Data;
using FileStorage.RestApi.Models;
using FileStorage.RestApi.Models.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileStorage.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunctionsController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public FunctionsController(AppDbContext context, IMapper mapper, IConfiguration configuration)
        {
            this.context = context;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [HttpPost(nameof(Register))]
        public async Task<ActionResult<UserRegisterDto>> Register(UserRegisterDto userRegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            userRegisterDto.UserId = 0;
            var user = mapper.Map<User>(userRegisterDto);
            context.Users.Add(user);
            try
            {
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateException)
            {
                if (context.Users.Any(u => u.UserName == user.UserName || u.Email == user.Email))
                {
                    return Conflict();
                }
                throw;
            }
            userRegisterDto = mapper.Map<UserRegisterDto>(user);
            return CreatedAtRoute(nameof(UsersController.GetUser), new { id = userRegisterDto.UserId }, userRegisterDto);
        }

        [HttpPost(nameof(SignIn))]
        public async Task<IActionResult> SignIn(UserRegisterDto userRegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return Unauthorized();
            }

            User user = await AuthtenticateUser(userRegisterDto.Email, userRegisterDto.Password).ConfigureAwait(false);
            if (user == null)
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)).ConfigureAwait(false);

            return RedirectToRoute(nameof(UsersController.GetUser), new { id = user.UserId });
        }

        [HttpGet(nameof(SignOut))]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPost(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile(IFormFile formFile)
        {
            string fileName, filePath, fullFilePath;
            do
            {
                fileName = CreateFileName(formFile);
                filePath = Path.Combine(configuration["StoredFilesDirectory"], fileName);
                fullFilePath = Path.Combine(configuration["StoredFilesPath"], fileName);

            } while (System.IO.File.Exists(fullFilePath));
            using (var stream = System.IO.File.Create(fullFilePath))
            {
                await formFile.CopyToAsync(stream).ConfigureAwait(false);
            }
            
            return Created(new Uri(filePath), filePath);
        }

        private static string CreateFileName(IFormFile formFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(formFile.FileName);
            fileName = WebUtility.HtmlEncode(fileName);
            var fileNameRandomSuffix = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "_" + Guid.NewGuid().ToString();
            fileName = fileName + "_" + fileNameRandomSuffix;
            fileName += Path.GetExtension(formFile.FileName);
            return fileName;
        }

        private async Task<User> AuthtenticateUser(string email, string password)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password).ConfigureAwait(false);
        }
    }
}