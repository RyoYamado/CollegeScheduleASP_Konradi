using CollegeSchedule.DTO;
namespace CollegeSchedule.Services
{
    public interface IScheduleService
    {
        // Существующий метод
        Task<List<ScheduleByDateDto>> GetScheduleForGroup(string groupName, DateTime startDate, DateTime endDate);

        // Новые методы
        Task<List<ScheduleByDateDto>> GetScheduleForTeacher(int teacherId, DateTime startDate, DateTime endDate);
        Task<List<ScheduleByDateDto>> GetScheduleForClassroom(int classroomId, DateTime startDate, DateTime endDate);
        Task<List<TeacherDto>> GetAllTeachers();
        Task<List<StudentGroupDto>> GetAllGroups();
        Task<List<ClassroomDto>> GetAllClassrooms();
        Task<List<SubjectDto>> GetAllSubjects();
        Task<ScheduleDetailsDto> GetScheduleById(int scheduleId);
    }
}