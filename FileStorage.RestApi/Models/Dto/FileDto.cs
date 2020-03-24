using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileStorage.RestApi.Models.Dto
{
    public class FileDto
    {
        public int FileId { get; set; }
        [Required]
        public string FilePath { get; set; }
        [Required]
        public int UserId { get; set; }
    }
}
