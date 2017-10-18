using AutoMapper;
using Axe.Models;
using Axe.ViewModels.CompilerVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axe
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CodeBlock, CodeBlockVm>().ReverseMap();
            CreateMap<CodeBlock, CodeBlockSolveVm>().ForMember(dest => dest.SourceCode, conf => conf.MapFrom(src => src.Technology.Template));

            CreateMap<CodeBlock, CodeBlockCreateVm>().ForMember(dest => dest.SelectedTechnologyId, conf => conf.MapFrom(src => src.Technology.Id));
        }
    }
}
