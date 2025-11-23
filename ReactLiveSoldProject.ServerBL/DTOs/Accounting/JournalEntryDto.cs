namespace ReactLiveSoldProject.ServerBL.DTOs.Accounting
{
    public class JournalEntryDto
    {
        public Guid Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string Description { get; set; }
        public string ReferenceNumber { get; set; }
        public List<JournalEntryLineDto> Lines { get; set; } = new List<JournalEntryLineDto>();
    }
}
