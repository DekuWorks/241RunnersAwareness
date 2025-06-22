using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // 👈 Add this if not already

namespace _241RunnersAwareness.BackendAPI.Models
{
    public class Individual
    {
        [Key]
        public Guid IndividualId { get; set; }  // 👈 This is your PRIMARY KEY

        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string SpecialNeedsDescription { get; set; }
        public string PhotoPath { get; set; }
        public string FingerprintPath { get; set; }
        public string ThumbprintPath { get; set; }
        public string CurrentStatus { get; set; }
        public bool HasBeenAdopted { get; set; }
        public DateTime? AdoptionDate { get; set; }
        public string PlacementStatus { get; set; }
        public DateTime DateAdded { get; set; }
        public string Notes { get; set; }

        public List<EmergencyContact> EmergencyContacts { get; set; }
    }
}
