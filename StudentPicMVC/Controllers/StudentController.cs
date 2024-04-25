using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Student_Utility;
using StudentPicMVC.Models;
using StudentPicMVC.Models.DTO;
using StudentPicMVC.Models.VM;
using StudentPicMVC.Services.IServices;

namespace StudentPicMVC.Controllers
{
    public class StudentController : Controller
    {
        //implement IStudentService and Automapper--------------------------------
        private readonly IStudentService _studentService;
        private readonly IMapper _mapper;

        public StudentController(IStudentService studentService, IMapper mapper)
        {
            _studentService = studentService;
            _mapper = mapper;
        }

        //View         
        //Index View for Student--------------------------------------------------
        //GET
        public async Task<IActionResult> IndexStudent()
        {
            //declare VM             
            //List<StudentDTO> list = new();

            StudentVM studentVM = new();

            //get response
            var response = await _studentService.GetAllAsync<APIResponse>();
            //validation to deserialize
            if (response != null && response.IsSuccess)
            {
                //list = JsonConvert.DeserializeObject<List<StudentDTO>>(Convert.ToString(response.Result));
                studentVM.StudentList = JsonConvert.DeserializeObject<List<StudentDTO>>(Convert.ToString(response.Result));

            }
            return View(studentVM);    
        }

        //Create View for Student------------------------------------------------
        //GET
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateStudent()
        {
            StudentCreateVM studentCreateVM = new();
            return View(studentCreateVM);
        }

        //POST
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(StudentCreateVM model)
        {
            //Model Static Validation
            if (ModelState.IsValid)
            {
                var response= await _studentService.CreateAsync<APIResponse>(model.Student);
                if (response != null && response.IsSuccess)
                {
                    //sweetalert2 noti
                    TempData["success"] = "Student created successfully";
                    return RedirectToAction(nameof(IndexStudent));
                }
            }

            //Sweetalert2 noti
            TempData["error"] = "Error Encountered!";
            return View(model);
        }

        //Update View for Student------------------------------------------------
        //GET
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateStudent(int id)
        {
            StudentUpdateVM studentUpdateVM = new();

            //get student info
            var response= await _studentService.GetAsync<APIResponse>(id);
            //validation to deserialize
            if (response != null && response.IsSuccess)
            {
                StudentUpdateDTO model = JsonConvert.DeserializeObject<StudentUpdateDTO>(Convert.ToString(response.Result));
                studentUpdateVM.Student = _mapper.Map<StudentUpdateDTO>(model);
            }

            return View(studentUpdateVM);
        }
        //POST
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStudent(StudentUpdateVM model)
        {
            //Model Static Validation
            if (ModelState.IsValid)
            {
                var response = await _studentService.UpdateAsync<APIResponse>(model.Student);
                if (response != null && response.IsSuccess)
                {
                    //sweetalert2 noti
                    TempData["success"] = "Student updated successfully";
                    return RedirectToAction(nameof(IndexStudent));
                }                
            }            

            //Sweetalert2 noti
            TempData["error"] = "Error Encountered!";
            return View(model);
        }

        //Delete View for Student------------------------------------------------
        //GET
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            StudentDeleteVM studentDeleteVM = new();

            //get student info
            var response = await _studentService.GetAsync<APIResponse>(id);
            //validation to deserialize
            if (response != null && response.IsSuccess)
            {
                StudentDTO model = JsonConvert.DeserializeObject<StudentDTO>(Convert.ToString(response.Result));
                studentDeleteVM.Student = model;
            }

            return View(studentDeleteVM);
        }
        //POST
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(StudentDeleteVM model)
        {
            var response = await _studentService.DeleteAsync<APIResponse>(model.Student.Id);
            //redirect back to Index view
            if (response != null && response.IsSuccess)
            {
                //sweetalert2 notification
                TempData["success"] = "Student info deleted!";
                return RedirectToAction(nameof(IndexStudent));
            }
            //return dto with error if it is invalid
            //sweetalert2 notification
            TempData["error"] = "Error Encountered!";
            return View(model);
        }

    }
}
