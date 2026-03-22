namespace CollegeSchedule.DTO
{
    public class ScheduleDetailsDto
    {
        public int ScheduleId { get; set; }
        public DateTime LessonDate { get; set; }
        public string Weekday { get; set; } = null!;
        public int LessonNumber { get; set; }
        public string LessonTime { get; set; } = null!;
        public string Group { get; set; } = null!;
        public string GroupPart { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Teacher { get; set; } = null!;
        public string TeacherPosition { get; set; } = null!;
        public string Classroom { get; set; } = null!;
        public string Building { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
}