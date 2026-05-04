namespace SmartDashboardAPI.Dto
{
    public class RelationshipDto
    {
        public string Id { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string Join { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string On { get; set; } = string.Empty;
        public string FromLabel { get; set; } = string.Empty;
        public string JoinLabel { get; set; } = string.Empty;
    }
}
