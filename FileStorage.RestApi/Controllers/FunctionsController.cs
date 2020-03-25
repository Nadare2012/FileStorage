using AutoMapper;
using FileStorage.RestApi.Data;
using FileStorage.RestApi.Models;
using FileStorage.RestApi.Models.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            await Authorize(user).ConfigureAwait(false);
            userRegisterDto = mapper.Map<UserRegisterDto>(user);
            return CreatedAtRoute(nameof(UsersController.GetUser), new { id = userRegisterDto.UserId }, userRegisterDto);
        }

        [HttpPost(nameof(SignIn))]
        public async Task<IActionResult> SignIn(UserSignInDto userSignInDto)
        {
            User user = await GetUser(userSignInDto.Email, userSignInDto.Password).ConfigureAwait(false);
            if (user == null)
            {
                return NotFound();
            }
            await Authorize(user).ConfigureAwait(false);
            return RedirectToRoute(nameof(UsersController.GetUser), new { id = user.UserId });
        }

        [HttpGet(nameof(SignOut))]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return NoContent();
        }

        [Authorize]
        [HttpPost(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile(IFormFile formFile)
        {
            CreateFilePaths(formFile, out string filePath, out string fullFilePath);
            using (var stream = System.IO.File.Create(fullFilePath))
            {
                await formFile.CopyToAsync(stream).ConfigureAwait(false);
            }

            return CreatedAtAction(nameof(UploadFile), filePath);
        }

        private async Task Authorize(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)).ConfigureAwait(false);
        }

        private async Task<User> GetUser(string email, string password)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password).ConfigureAwait(false);
        }

        private void CreateFilePaths(IFormFile formFile, out string filePath, out string fullFilePath)
        {
            string fileName;
            do
            {
                fileName = CreateFileName(formFile);
                filePath = Path.Combine(configuration["StoredFilesDirectory"], fileName);
                fullFilePath = Path.Combine(configuration["StoredFilesPath"], fileName);

            } while (System.IO.File.Exists(fullFilePath));
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
    }
}