namespace ReactLiveSoldProject.ServerBL.DTOs.Accounting
{
    public class JournalEntryLineDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountCode { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}
