using AutoMapper;
using FileStorage.RestApi.Models;
using FileStorage.RestApi.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileStorage.RestApi.Mappings
{
    public class FileMappingsProfile : Profile
    {
        public FileMappingsProfile()
        {
            CreateMap<File, FileDto>();
            CreateMap<FileDto, File>();
        }
    }
}
