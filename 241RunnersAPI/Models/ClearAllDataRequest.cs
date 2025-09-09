using System.ComponentModel.DataAnnotations;

namespace _241RunnersAPI.Models
{
    /// <summary>
    /// Request model for clearing all user data
    /// </summary>
    public class ClearAllDataRequest
    {
        /// <summary>
        /// Confirmation flag - must be true to proceed
        /// </summary>
        [Required]
        public bool ConfirmAction { get; set; }

        /// <summary>
        /// Reason for clearing data (optional)
        /// </summary>
        public string? Reason { get; set; }
    }
}
