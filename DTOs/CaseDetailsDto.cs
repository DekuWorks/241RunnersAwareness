using System;

namespace _241RunnersAwareness.DTOs
{
    public class CaseDetailsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Image { get; set; }
    }

    public class CaseImageUpdateDto
    {
        public string Image { get; set; }
    }
} 