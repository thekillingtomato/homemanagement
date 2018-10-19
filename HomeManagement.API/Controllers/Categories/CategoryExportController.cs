﻿using System;
using System.Collections.Generic;
using System.Linq;
using HomeManagement.API.Exportation;
using HomeManagement.API.Extensions;
using HomeManagement.API.Filters;
using HomeManagement.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeManagement.API.Controllers.Categories
{
    [Authorization]
    [EnableCors("SiteCorsPolicy")]
    [Produces("application/json")]
    [Route("api/Category")]
    public class CategoryExportController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IUserCategoryRepository userCategoryRepository;
        private readonly IExportableCategory exportableCategory;

        public CategoryExportController(ICategoryRepository categoryRepository,
                                    IUserRepository userRepository,
                                    IUserCategoryRepository userCategoryRepository,
                                    IExportableCategory exportableCategory)
        {
            this.categoryRepository = categoryRepository;
            this.userRepository = userRepository;
            this.userCategoryRepository = userCategoryRepository;
            this.exportableCategory = exportableCategory;
        }

        [HttpGet("download")]
        public IActionResult DownloadCategories()
        {
            var email = HttpContext.GetEmailClaim();

            var categories = (from category in categoryRepository.All
                              join userCategory in userCategoryRepository.All
                              on category.Id equals userCategory.CategoryId
                              join user in userRepository.All
                              on userCategory.UserId equals user.Id
                              where user.Email.Equals(email.Value)
                              select category).ToList();

            var csv = exportableCategory.ToCsv(categories);

            var filename = "categories_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";

            var result = this.CreateCsvFile(csv, filename);

            return result;
        }

        [HttpPost("upload")]
        public IActionResult UploadCategories()
        {
            var basePath = AppContext.BaseDirectory + "\\{0}";
            if (Request.Form == null) return BadRequest();

            var emailClaim = userRepository.FirstOrDefault(x => x.Email.Equals(HttpContext.GetEmailClaim().Value));

            foreach (var f in Request.Form.Files)
            {
                var formFile = f as IFormFile;

                var stream = formFile.OpenReadStream();

                var bytes = stream.GetBytes();

                var entities = exportableCategory.ToEntities(bytes);

                foreach (var entity in entities)
                {
                    if (entity == null) continue;

                    if (categoryRepository.Exists(entity)) continue;

                    entity.Id = 0;

                    categoryRepository.Add(entity, emailClaim);
                }
            }

            return Ok();
        }
    }
}