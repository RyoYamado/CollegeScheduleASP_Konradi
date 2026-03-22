using CollegeSchedule.Data;
using CollegeSchedule.Models;
using CollegeSchedule.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeSchedule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _service;

        public ScheduleController(IScheduleService service)
        {
            _service = service;
        }
        
        // GET: api/schedule/group/{groupName}?start=2026-01-12&end=2026-01-17
        [HttpGet("group/{groupName}")]
        public async Task<IActionResult> GetScheduleByGroup(string groupName, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var result = await _service.GetScheduleForGroup(groupName, start.Date, end.Date);
            return Ok(result);
        }

        // GET: api/schedule/teacher/{teacherId}?start=2026-01-12&end=2026-01-17
        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetScheduleByTeacher(int teacherId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var result = await _service.GetScheduleForTeacher(teacherId, start.Date, end.Date);
            return Ok(result);
        }

        // GET: api/schedule/classroom/{classroomId}?start=2026-01-12&end=2026-01-17
        [HttpGet("classroom/{classroomId}")]
        public async Task<IActionResult> GetScheduleByClassroom(int classroomId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var result = await _service.GetScheduleForClassroom(classroomId, start.Date, end.Date);
            return Ok(result);
        }

        // GET: api/schedule/teachers
        [HttpGet("teachers")]
        public async Task<IActionResult> GetAllTeachers()
        {
            var result = await _service.GetAllTeachers();
            return Ok(result);
        }

        // GET: api/schedule/groups
        [HttpGet("groups")]
        public async Task<IActionResult> GetAllGroups()
        {
            var result = await _service.GetAllGroups();
            return Ok(result);
        }

        // GET: api/schedule/classrooms
        [HttpGet("classrooms")]
        public async Task<IActionResult> GetAllClassrooms()
        {
            var result = await _service.GetAllClassrooms();
            return Ok(result);
        }

        // GET: api/schedule/subjects
        [HttpGet("subjects")]
        public async Task<IActionResult> GetAllSubjects()
        {
            var result = await _service.GetAllSubjects();
            return Ok(result);
        }

        // GET: api/schedule/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var result = await _service.GetScheduleById(id);
            return Ok(result);
        }
    }
}