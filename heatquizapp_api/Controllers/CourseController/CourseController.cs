﻿using AutoMapper;
using HeatQuizAPI.Database;
using HeatQuizAPI.Models.BaseModels;
using heatquizapp_api.Models.BaseModels;
using heatquizapp_api.Models.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeatQuizAPI.Utilities;
using static heatquizapp_api.Utilities.Utilities;

namespace heatquizapp_api.Controllers.CourseController
{
    [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<User> _userManager;

        public CourseController(
           ApplicationDbContext applicationDbContext,
           IMapper mapper,
           IHttpContextAccessor contextAccessor,
           UserManager<User> userManager
           )
        {
            _applicationDbContext = applicationDbContext;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("[action]")]
        //Change type and name in vs code -- original GetAllCourses_PORTAL
        public async Task<IActionResult> GetAllCourses([FromBody] DatapoolCarrierViewModel VM)
        {
            if (!ModelState.IsValid)
                return BadRequest(Constants.HTTP_REQUEST_INVALID_DATA);

            var DPExists = await _applicationDbContext.DataPools
                .AnyAsync(d => d.Id == VM.DatapoolId);

            if (!DPExists)
                return NotFound("Datapool does not exist");

            var Courses = await _applicationDbContext.Courses
                .OrderBy(c => c.Name)
                .Include(c => c.AddedBy)
                .Where(c => c.DataPoolId == VM.DatapoolId)
                .ToListAsync();

            return Ok(_mapper.Map<List<Course>, List<CourseViewModel>>(Courses));
        }

        [HttpGet("[action]/{Id}")]
        //Change name in vs code -- original: GetCourseById_PORTAL
        public async Task<IActionResult> GetCourseById(int Id)
        {
            var Course = await _applicationDbContext.Courses
                .Include(c => c.AddedBy)
                //.Include(c => c.CourseMaps)
                .FirstOrDefaultAsync(c => c.Id == Id);

            if (Course is null)
                return NotFound("Course not found");

            return Ok(_mapper.Map<Course, CourseViewModel>(Course));
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> AddCourseSingleStep(string Name, string Code, IFormFile Picture, int DataPoolId)
        {
            if (!ModelState.IsValid)
                return BadRequest(Constants.HTTP_REQUEST_INVALID_DATA);

            //Check name not null
            if (string.IsNullOrEmpty(Name))
                return BadRequest("Name can't be empty");

            //Check code not null
            if (string.IsNullOrEmpty(Code))
                return BadRequest("Code can't be empty");

            //Check datapool exists
            var DP = await _applicationDbContext.DataPools
               .FirstOrDefaultAsync(dp => dp.Id == DataPoolId);

            if (DP is null)
                return NotFound("Datapool not found");

            //Check name not Taken 
            var nameTaken = await _applicationDbContext.Courses
                .AnyAsync(c => c.Name == Name && c.DataPoolId == DP.Id);

            if (nameTaken)
                return BadRequest("Name taken, choose different name");

            //Check code not Taken 
            var codeTaken = await _applicationDbContext.Courses
                .AnyAsync(c => c.Code == Code);

            if (codeTaken)
                return BadRequest("Code taken, choose different code");

            //Check picture
            if (Picture is null)
                return BadRequest("Please provide a picture");

            //Verify Extension
            var extensionIsValid = await validateImageExtension(Picture);
        
            if (!extensionIsValid)
                return BadRequest("Picture extenstion not valid");

            //Get adder
            var Adder = await getCurrentUser(_contextAccessor, _userManager);

            //Create Course
            var course = new Course()
            {
                Name = Name,
                Code = Code,
                AddedBy = Adder,
                DataPoolId = DP.Id
            };

            //Save picture and url path for it
            var URL = await SaveFile(Picture);

            course.URL = URL;
            course.Size = Picture.Length;

            _applicationDbContext.Courses.Add(course);
            await _applicationDbContext.SaveChangesAsync();

            return Ok(_mapper.Map<Course, CourseViewModel>(course));
        }

        [HttpPut("[action]")]
        //Change type on vs code
        public async Task<IActionResult> EditCourseSingleStep(int CourseId, string Name, string Code, IFormFile Picture, bool SameImage, int DataPoolId)
        {
            //Check Name Not Null
            if (string.IsNullOrEmpty(Name))
                return BadRequest("Name can't be empty");

            if (string.IsNullOrEmpty(Code))
                return BadRequest("Code  can't be empty");

            //Check course exists
            var Course = await _applicationDbContext.Courses
                .FirstOrDefaultAsync(c => c.Id == CourseId);

            if (Course is null)
                return NotFound("Course not found");

            //Check datapool exists
            var DP = await _applicationDbContext.DataPools
               .FirstOrDefaultAsync(dp => dp.Id == DataPoolId);

            if (DP is null)
                return NotFound("Datapool not found");

            //Check Name/Code Not Taken 
            var nameTaken = await _applicationDbContext.Courses
                .AnyAsync(c => c.Name == Name && c.Id != CourseId && c.DataPoolId == DP.Id);

            if (nameTaken)
                return BadRequest("Name taken, choose different name");

            var codeTaken = await _applicationDbContext.Courses
                .AnyAsync(c => c.Code == Code && c.Id != CourseId && c.DataPoolId == DP.Id);

            if (codeTaken)
                return BadRequest("Code taken, choose different code");

            //Edit Course Name
            Course.Name = Name;
            Course.Code = Code;

            if (!SameImage)
            {

                //Check Picture
                if (Picture is null)
                    return BadRequest("Please provide a picture");

                //Verify Extension
                var extensionIsValid = await validateImageExtension(Picture);

                if (!extensionIsValid)
                    return BadRequest("Picture extenstion not valid");

                //Save picture and url path for it
                var URL = await SaveFile(Picture);

                Course.URL = URL;
                Course.Size = Picture.Length;
            }

            await _applicationDbContext.SaveChangesAsync();

            return Ok(_mapper.Map<Course, CourseViewModel>(Course));
        }

    }
}