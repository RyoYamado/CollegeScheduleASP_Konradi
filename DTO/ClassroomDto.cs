namespace CollegeSchedule.DTO
{
    public class ClassroomDto
    {
        public int ClassroomId { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string Building { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
}