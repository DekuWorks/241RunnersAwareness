using System.ComponentModel.DataAnnotations;

public class EmergencyContact
{
    [Key]
    public int Id { get; set; }  // 👈 PRIMARY KEY

    public string Name { get; set; }
    public string Phone { get; set; }
}


