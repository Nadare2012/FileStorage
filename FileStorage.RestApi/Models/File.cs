using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileStorage.RestApi.Models
{
    public class File
    {
        public int FileId { get; set; }
        public string FilePath { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
