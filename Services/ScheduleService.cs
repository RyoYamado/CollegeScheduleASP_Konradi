using CollegeSchedule.Data;
using CollegeSchedule.DTO;
using CollegeSchedule.Models;
using Microsoft.EntityFrameworkCore;

namespace CollegeSchedule.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly AppDbContext _db;

        public ScheduleService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ScheduleByDateDto>> GetScheduleForGroup(string groupName, DateTime startDate, DateTime endDate)
        {
            ValidateDates(startDate, endDate);
            var group = await GetGroupByName(groupName);
            var schedules = await LoadSchedules(s => s.GroupId == group.GroupId, startDate, endDate);
            return BuildScheduleDto(startDate, endDate, schedules);
        }

        public async Task<List<ScheduleByDateDto>> GetScheduleForTeacher(int teacherId, DateTime startDate, DateTime endDate)
        {
            ValidateDates(startDate, endDate);

            var teacher = await _db.Teachers
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
                throw new KeyNotFoundException($"Преподаватель с ID {teacherId} не найден.");

            var schedules = await LoadSchedules(s => s.TeacherId == teacherId, startDate, endDate);
            return BuildScheduleDto(startDate, endDate, schedules);
        }

        public async Task<List<ScheduleByDateDto>> GetScheduleForClassroom(int classroomId, DateTime startDate, DateTime endDate)
        {
            ValidateDates(startDate, endDate);

            var classroom = await _db.Classrooms
                .Include(c => c.Building)
                .FirstOrDefaultAsync(c => c.ClassroomId == classroomId);

            if (classroom == null)
                throw new KeyNotFoundException($"Аудитория с ID {classroomId} не найдена.");

            var schedules = await LoadSchedules(s => s.ClassroomId == classroomId, startDate, endDate);
            return BuildScheduleDto(startDate, endDate, schedules);
        }

        public async Task<List<TeacherDto>> GetAllTeachers()
        {
            return await _db.Teachers
                .Select(t => new TeacherDto
                {
                    TeacherId = t.TeacherId,
                    FullName = $"{t.LastName} {t.FirstName} {t.MiddleName ?? ""}".Trim(),
                    Position = t.Position
                })
                .OrderBy(t => t.FullName)
                .ToListAsync();
        }

        public async Task<List<StudentGroupDto>> GetAllGroups()
        {
            return await _db.StudentGroups
                .Include(g => g.Specialty)
                .Select(g => new StudentGroupDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    Course = g.Course,
                    Specialty = g.Specialty.Name
                })
                .OrderBy(g => g.GroupName)
                .ToListAsync();
        }

        public async Task<List<ClassroomDto>> GetAllClassrooms()
        {
            return await _db.Classrooms
                .Include(c => c.Building)
                .Select(c => new ClassroomDto
                {
                    ClassroomId = c.ClassroomId,
                    RoomNumber = c.RoomNumber,
                    Building = c.Building.Name,
                    Address = c.Building.Address
                })
                .OrderBy(c => c.Building)
                .ThenBy(c => c.RoomNumber)
                .ToListAsync();
        }

        public async Task<List<SubjectDto>> GetAllSubjects()
        {
            return await _db.Subjects
                .Select(s => new SubjectDto
                {
                    SubjectId = s.SubjectId,
                    Name = s.Name
                })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<ScheduleDetailsDto> GetScheduleById(int scheduleId)
        {
            var schedule = await _db.Schedules
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Group)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Classroom)
                    .ThenInclude(c => c.Building)
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

            if (schedule == null)
                throw new KeyNotFoundException($"Расписание с ID {scheduleId} не найдено.");

            return new ScheduleDetailsDto
            {
                ScheduleId = schedule.ScheduleId,
                LessonDate = schedule.LessonDate,
                Weekday = schedule.Weekday.Name,
                LessonNumber = schedule.LessonTime.LessonNumber,
                LessonTime = $"{schedule.LessonTime.TimeStart:hh\\:mm}-{schedule.LessonTime.TimeEnd:hh\\:mm}",
                Group = schedule.Group.GroupName,
                GroupPart = schedule.GroupPart.ToString(),
                Subject = schedule.Subject.Name,
                Teacher = $"{schedule.Teacher.LastName} {schedule.Teacher.FirstName} {schedule.Teacher.MiddleName}".Trim(),
                TeacherPosition = schedule.Teacher.Position,
                Classroom = schedule.Classroom.RoomNumber,
                Building = schedule.Classroom.Building.Name,
                Address = schedule.Classroom.Building.Address
            };
        }

        private static void ValidateDates(DateTime start, DateTime end)
        {
            if (start > end)
                throw new ArgumentOutOfRangeException(nameof(start), "Дата начала больше даты окончания.");
        }

        private async Task<StudentGroup> GetGroupByName(string groupName)
        {
            var group = await _db.StudentGroups
                .FirstOrDefaultAsync(g => g.GroupName == groupName);

            if (group == null)
                throw new KeyNotFoundException($"Группа {groupName} не найдена.");

            return group;
        }

        private async Task<List<Schedule>> LoadSchedules(System.Linq.Expressions.Expression<Func<Schedule, bool>> predicate, DateTime start, DateTime end)
        {
            return await _db.Schedules
                .Where(predicate)
                .Where(s => s.LessonDate >= start && s.LessonDate <= end)
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Classroom)
                    .ThenInclude(c => c.Building)
                .Include(s => s.Group)
                .OrderBy(s => s.LessonDate)
                .ThenBy(s => s.LessonTime.LessonNumber)
                .ThenBy(s => s.GroupPart)
                .ToListAsync();
        }

        private static List<ScheduleByDateDto> BuildScheduleDto(DateTime startDate, DateTime endDate, List<Schedule> schedules)
        {
            var scheduleByDate = schedules
                .GroupBy(s => s.LessonDate)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<ScheduleByDateDto>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                if (!scheduleByDate.TryGetValue(date, out var daySchedules))
                {
                    result.Add(BuildEmptyDayDto(date));
                }
                else
                {
                    result.Add(BuildDayDto(daySchedules));
                }
            }

            return result;
        }

        private static ScheduleByDateDto BuildDayDto(List<Schedule> daySchedules)
        {
            var lessons = daySchedules
                .GroupBy(s => new { s.LessonTime.LessonNumber, s.LessonTime.TimeStart, s.LessonTime.TimeEnd })
                .Select(BuildLessonDto)
                .ToList();

            return new ScheduleByDateDto
            {
                LessonDate = daySchedules.First().LessonDate,
                Weekday = daySchedules.First().Weekday.Name,
                Lessons = lessons
            };
        }

        private static LessonDto BuildLessonDto(IGrouping<dynamic, Schedule> lessonGroup)
        {
            var lessonDto = new LessonDto
            {
                LessonNumber = lessonGroup.Key.LessonNumber,
                Time = $"{lessonGroup.Key.TimeStart:hh\\:mm}-{lessonGroup.Key.TimeEnd:hh\\:mm}",
                GroupParts = new Dictionary<LessonGroupPart, LessonPartDto?>()
            };

            foreach (var part in lessonGroup)
            {
                lessonDto.GroupParts[part.GroupPart] = new LessonPartDto
                {
                    Subject = part.Subject.Name,
                    Teacher = $"{part.Teacher.LastName} {part.Teacher.FirstName} {part.Teacher.MiddleName}".Trim(),
                    TeacherPosition = part.Teacher.Position,
                    Classroom = part.Classroom.RoomNumber,
                    Building = part.Classroom.Building.Name,
                    Address = part.Classroom.Building.Address
                };
            }

            if (!lessonDto.GroupParts.ContainsKey(LessonGroupPart.FULL))
                lessonDto.GroupParts.TryAdd(LessonGroupPart.FULL, null);

            return lessonDto;
        }

        private static ScheduleByDateDto BuildEmptyDayDto(DateTime date)
        {
            return new ScheduleByDateDto
            {
                LessonDate = date,
                Weekday = date.DayOfWeek.ToString(),
                Lessons = new List<LessonDto>()
            };
        }
    }
}