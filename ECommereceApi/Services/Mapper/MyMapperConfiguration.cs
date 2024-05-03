﻿using AutoMapper;
using ECommereceApi.DTOs;
using ECommereceApi.Models;

namespace ECommereceApi.Services.Mapper
{
    public class MyMapperConfiguration : Profile
    {
        public MyMapperConfiguration()
        {
            CreateMap<Product, ProductDisplayDTO>()
                .ForMember(p => p.CategoryName, option => option.MapFrom(p => p.Category.Name))
                .ReverseMap();

            CreateMap<Product, ProductAddDTO>().ReverseMap();
            CreateMap<CategoryDTO, Category>().ReverseMap();

            CreateMap<SubCategory, CategoryDTO>()
                .ForMember(dest => dest.CategoryId, option => option.MapFrom(src => src.SubId));
           
        }
    }
}
