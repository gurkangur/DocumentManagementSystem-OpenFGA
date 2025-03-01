namespace DocumentManagement.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public Guid OwnerId { get; set; }
    }
}
