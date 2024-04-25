using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentPicAPI.Models.DTO;
using StudentPicAPI.Models;
using StudentPicAPI.Repository.IRepository;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace StudentPicAPI.Controllers.v1
{
    [Route("api/v{version:apiVersion}/StudentAPI")]
    [ApiController]
    //API version
    [ApiVersion("1.0")]
    public class StudentAPIController : ControllerBase
    {
        //---------------------Implementation-----------------------
        protected APIResponse _response;
        //use repository
        private readonly IStudentRepository _dbStudent;
        //automapper
        private readonly IMapper _mapper;

        public StudentAPIController(IStudentRepository dbStudent, IMapper mapper)
        {
            _dbStudent = dbStudent;
            _mapper = mapper;
            _response = new();
        }

        //---------------------GET----------------------------------
        //GET ALL
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> GetStudents()
        {
            try
            {
                IEnumerable<Student> studentList = await _dbStudent.GetAllAsync();
                _response.Result = _mapper.Map<List<StudentDTO>>(studentList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.Message };
            }
            return _response;
        }

        //GET By ID
        [HttpGet("{id:int}", Name = "GetStudent")]
        //Response Type
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<APIResponse>> GetStudent(int id)
        {
            try
            {
                //check validation
                if (id == 0)
                {
                    //_logger.LogError("Get student error with id" + id);
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                //repository
                var student = await _dbStudent.GetAsync(u => u.Id == id);

                if (student == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                //automapping + API response
                _response.Result = _mapper.Map<StudentDTO>(student);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.Message };
            }
            return _response;
        }

        //---------------------ADD----------------------------------
        [Authorize(Roles = "admin")]
        //ADD http verb
        [HttpPost]
        //Response type
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> CreateStudent([FromBody] StudentCreateDTO createDTO)
        {
            try
            {
                //check if the name exists
                if (await _dbStudent.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessage.Add("Student Name Exists");
                    return BadRequest(_response);
                }

                //check if the value is null
                if (createDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }

                //auto mapper
                Student student = _mapper.Map<Student>(createDTO);
                student.CreatedDate = DateTime.Now;
                //use repository
                await _dbStudent.CreateAsync(student);
                //api response
                _response.Result = _mapper.Map<StudentCreateDTO>(student);
                _response.StatusCode = HttpStatusCode.Created;
                //return CreatedAtRoute("GetStudent", new { id = student.Id }, _response);
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage.Add(ex.Message);
            }
            return _response;
        }

        //---------------------DELETE-------------------------------
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id:int}", Name = "DeleteStudent")]
        public async Task<ActionResult<APIResponse>> DeleteStudent(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                //with no repository
                //var villa=await _db.Villas.FirstOrDefaultAsync(u=> u.Id==id);
                //if (villa==null)
                //{
                //    return NotFound();
                //}
                //_db.Villas.Remove(villa);
                //await _db.SaveChangesAsync();

                //with repository
                var student = await _dbStudent.GetAsync(u => u.Id == id);
                if (student == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                await _dbStudent.RemoveAsync(student);

                //use IActionResult
                //return NoContent();

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.Message };
            }
            return _response;
        }

        //---------------------UPDATE-------------------------------
        [Authorize(Roles = "admin")]
        //Response Type
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id:int}", Name = "UpdateStudent")]
        public async Task<ActionResult<APIResponse>> UpdateStudent(int id, [FromBody] StudentUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.Id)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                //custom validation with repository
                //if (await _dbStudent.GetAsync(u => u.Name.ToLower() == updateDTO.Name.ToLower()) != null)
                //{
                //    ModelState.AddModelError("ErrorMessage", "Student Name is existed");
                //    return BadRequest(ModelState);
                //}

                Student model = _mapper.Map<Student>(updateDTO);
                await _dbStudent.UpdateAsync(model);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.Message };
            }
            return _response;
        }
    }

}
