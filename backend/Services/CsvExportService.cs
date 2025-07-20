using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _241RunnersAwareness.BackendAPI.DBContext.Models;

namespace _241RunnersAwareness.BackendAPI.Services
{
    public interface ICsvExportService
    {
        byte[] ExportIndividualsToCsv(IEnumerable<Individual> individuals);
        string GenerateCsvContent(IEnumerable<Individual> individuals);
    }

    public class CsvExportService : ICsvExportService
    {
        public byte[] ExportIndividualsToCsv(IEnumerable<Individual> individuals)
        {
            var csvContent = GenerateCsvContent(individuals);
            return Encoding.UTF8.GetBytes(csvContent);
        }

        public string GenerateCsvContent(IEnumerable<Individual> individuals)
        {
            var csv = new StringBuilder();
            
            // Add headers
            csv.AppendLine("Individual ID,Full Name,Date of Birth,Age,Gender,Special Needs,Current Status,Has Been Adopted,Adoption Date,Placement Status,Date Added,Address,City,State,Zip Code,Latitude,Longitude,Notes");
            
            // Add data rows
            foreach (var individual in individuals)
            {
                var age = CalculateAge(individual.DateOfBirth);
                var adoptionDate = individual.AdoptionDate?.ToString("yyyy-MM-dd") ?? "";
                var dateAdded = individual.DateAdded.ToString("yyyy-MM-dd");
                var dateOfBirth = individual.DateOfBirth.ToString("yyyy-MM-dd");
                
                csv.AppendLine($"\"{individual.IndividualId}\"," +
                             $"\"{EscapeCsvField(individual.FullName)}\"," +
                             $"\"{dateOfBirth}\"," +
                             $"\"{age}\"," +
                             $"\"{EscapeCsvField(individual.Gender)}\"," +
                             $"\"{EscapeCsvField(individual.SpecialNeedsDescription)}\"," +
                             $"\"{EscapeCsvField(individual.CurrentStatus)}\"," +
                             $"\"{individual.HasBeenAdopted}\"," +
                             $"\"{adoptionDate}\"," +
                             $"\"{EscapeCsvField(individual.PlacementStatus)}\"," +
                             $"\"{dateAdded}\"," +
                             $"\"{EscapeCsvField(individual.Address)}\"," +
                             $"\"{EscapeCsvField(individual.City)}\"," +
                             $"\"{EscapeCsvField(individual.State)}\"," +
                             $"\"{EscapeCsvField(individual.ZipCode)}\"," +
                             $"\"{individual.Latitude?.ToString() ?? ""}\"," +
                             $"\"{individual.Longitude?.ToString() ?? ""}\"," +
                             $"\"{EscapeCsvField(individual.Notes)}\"");
            }
            
            return csv.ToString();
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            
            // Replace double quotes with two double quotes and wrap in quotes if needed
            var escaped = field.Replace("\"", "\"\"");
            return escaped;
        }
    }
} 