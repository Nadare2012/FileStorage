using AutoMapper;
using AutoMapper.QueryableExtensions;
using FileStorage.RestApi.Data;
using FileStorage.RestApi.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileStorage.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IMapper mapper;

        public UsersController(AppDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            return await context.Users.ProjectTo<UserDto>(mapper.ConfigurationProvider).ToListAsync().ConfigureAwait(false);
        }

        // GET: api/Users/5
        [HttpGet("{id}", Name = nameof(GetUser))]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var userDto = mapper.Map<UserDto>(user);
            return userDto;
        }


        // PUT: api/Users/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserDto userDto)
        {
            if (id != userDto.UserId)
            {
                return BadRequest();
            }
            if (GetUserId() != id)
            {
                return Forbid();
            }
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.UserName = userDto.UserName;
            user.Email = userDto.Email;
            context.Entry(user).State = EntityState.Modified;
            try
            {
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateException)
            {
                if (context.Users.Any(u => u.UserName == userDto.UserName || u.Email == userDto.Email))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value, CultureInfo.InvariantCulture);
        }
    }
}