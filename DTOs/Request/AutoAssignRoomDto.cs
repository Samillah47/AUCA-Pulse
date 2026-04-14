namespace AUCAPulse.DTOs.Request
{
    public class AutoAssignRoomDto
    {
        // Optional filter: LECTURE_HALL, LAB, MEETING_ROOM, OFFICE.
        // Leave null to pick from any available room.
        public string? RoomType { get; set; }

        // How long the reservation should last, in minutes. Default 120.
        public int DurationMinutes { get; set; } = 120;
    }
}
