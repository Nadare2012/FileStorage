using AutoMapper;
using AutoMapper.QueryableExtensions;
using FileStorage.RestApi.Data;
using FileStorage.RestApi.Models;
using FileStorage.RestApi.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileStorage.RestApi.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IMapper mapper;

        public FilesController(AppDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        // GET: api/Files
        [HttpGet("[controller]")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult<IEnumerable<FileDto>>> GetFiles()
        {
            return await context.Files.ProjectTo<FileDto>(mapper.ConfigurationProvider).ToListAsync().ConfigureAwait(false);
        }

        // GET: api/Files/5
        [HttpGet("[controller]/{id}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FileDto>> GetFile(int id)
        {
            var File = await context.Files.FindAsync(id);

            if (File == null)
            {
                return NotFound();
            }
            var FileDto = mapper.Map<FileDto>(File);
            return FileDto;
        }

        // GET: api/Users/5/Files
        [HttpGet("Users/{userId}/[controller]")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FileDto>>> GetFilesOfUser(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return mapper.Map<List<FileDto>>(user.Files);
        }

        // GET: api/Users/5/Files/5
        [HttpGet("Users/{userId}/[controller]/{fileId}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FileDto>> GetFileOfUser(int userId, int fileId)
        {
            var post = await context.Files.FirstOrDefaultAsync(file => file.UserId == userId && file.FileId == fileId).ConfigureAwait(false);
            if (post == null)
            {
                return NotFound();
            }
            var postDto = mapper.Map<FileDto>(post);
            return postDto;
        }

        // POST: api/Users/5/Files
        [Authorize]
        [HttpPost("Users/{userId}/[controller]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FileDto>> PostFileOfUser(FileDto fileDto, int userId)
        {
            if (GetUserId() != userId)
            {
                return Forbid();
            }
            if (!context.Users.Any(e => e.UserId == userId))
            {
                return NotFound();
            }
            fileDto.FileId = 0;
            fileDto.UserId = userId;
            var file = mapper.Map<File>(fileDto);
            context.Files.Add(file);
            await context.SaveChangesAsync().ConfigureAwait(false);
            fileDto = mapper.Map<FileDto>(file);
            return CreatedAtAction(nameof(GetFile), new { id = fileDto.FileId }, fileDto);
        }

        // DELETE: api/Users/5/Files/5
        [Authorize]
        [HttpDelete("Users/{userId}/[controller]/{fileId}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FileDto>> DeletePostOfUser(int userId, int fileId)
        {
            if (GetUserId() != userId)
            {
                return Forbid();
            }
            var file = await context.Files.FirstOrDefaultAsync(file => file.UserId == userId && file.FileId == fileId).ConfigureAwait(false);
            if (file == null)
            {
                return NotFound();
            }

            context.Files.Remove(file);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return NoContent();
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value, CultureInfo.InvariantCulture);
        }
    }
}