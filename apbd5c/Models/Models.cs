using System.ComponentModel.DataAnnotations;

namespace apbd5c.Models
{
    public class Patient
    {
        public int IdPatient { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        [Required]
        public DateTime Birthdate { get; set; }
        
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
    
    public class Doctor
    {
        public int IdDoctor { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }
        
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
    
    public class Medicament
    {
        public int IdMedicament { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Type { get; set; }
        
        public virtual ICollection<PrescriptionMedicament> PrescriptionMedicaments { get; set; } = new List<PrescriptionMedicament>();
    }
    
    public class Prescription
    {
        public int IdPrescription { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }
        
        public int IdPatient { get; set; }
        public virtual Patient Patient { get; set; }
        
        public int IdDoctor { get; set; }
        public virtual Doctor Doctor { get; set; }
        
        public virtual ICollection<PrescriptionMedicament> PrescriptionMedicaments { get; set; } = new List<PrescriptionMedicament>();
    }
    
    public class PrescriptionMedicament
    {
        public int IdPrescription { get; set; }
        public virtual Prescription Prescription { get; set; }
        
        public int IdMedicament { get; set; }
        public virtual Medicament Medicament { get; set; }
        
        public int Dose { get; set; }
        
        [MaxLength(500)]
        public string Details { get; set; }
    }
}

