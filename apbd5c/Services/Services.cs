using Microsoft.EntityFrameworkCore;
using apbd5c.Data;
using apbd5c.DTOs;
using apbd5c.Models;
using apbd5c.Services.Interfaces;

namespace apbd5c.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly PrescriptionDbContext _context;
        private readonly IPatientService _patientService;
        private readonly IMedicamentService _medicamentService;
        
        public PrescriptionService(PrescriptionDbContext context, IPatientService patientService, IMedicamentService medicamentService)
        {
            _context = context;
            _patientService = patientService;
            _medicamentService = medicamentService;
        }
        
        public async Task<int> CreatePrescriptionAsync(CreatePrescriptionDto prescriptionDto)
        {
            // Walidacja daty
            if (prescriptionDto.DueDate < prescriptionDto.Date)
            {
                throw new ArgumentException("DueDate musi być większa lub równa Date");
            }
            
            // Walidacja liczby leków
            if (prescriptionDto.Medicaments.Count > 10)
            {
                throw new ArgumentException("Recepta może zawierać maksymalnie 10 leków");
            }
            
            // Sprawdzenie czy wszystkie leki istnieją
            var medicamentIds = prescriptionDto.Medicaments.Select(m => m.IdMedicament);
            if (!await _medicamentService.AllMedicamentsExistAsync(medicamentIds))
            {
                throw new ArgumentException("Jeden lub więcej leków nie istnieje");
            }
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Utworzenie lub aktualizacja pacjenta
                var patientId = await _patientService.CreateOrUpdatePatientAsync(prescriptionDto.Patient);
                
                // Utworzenie recepty
                var prescription = new Prescription
                {
                    Date = prescriptionDto.Date,
                    DueDate = prescriptionDto.DueDate,
                    IdPatient = patientId,
                    IdDoctor = prescriptionDto.IdDoctor
                };
                
                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();
                
                // Dodanie leków do recepty
                foreach (var medicamentDto in prescriptionDto.Medicaments)
                {
                    var prescriptionMedicament = new PrescriptionMedicament
                    {
                        IdPrescription = prescription.IdPrescription,
                        IdMedicament = medicamentDto.IdMedicament,
                        Dose = medicamentDto.Dose,
                        Details = medicamentDto.Description
                    };
                    
                    _context.PrescriptionMedicaments.Add(prescriptionMedicament);
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return prescription.IdPrescription;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public async Task<PatientDetailsDto> GetPatientDetailsAsync(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.Prescriptions.OrderBy(pr => pr.DueDate))
                    .ThenInclude(pr => pr.Doctor)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.PrescriptionMedicaments)
                        .ThenInclude(pm => pm.Medicament)
                .FirstOrDefaultAsync(p => p.IdPatient == patientId);
            
            if (patient == null)
            {
                throw new ArgumentException($"Pacjent o ID {patientId} nie istnieje");
            }
            
            return new PatientDetailsDto
            {
                IdPatient = patient.IdPatient,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Birthdate = patient.Birthdate,
                Prescriptions = patient.Prescriptions.Select(p => new PrescriptionDto
                {
                    IdPrescription = p.IdPrescription,
                    Date = p.Date,
                    DueDate = p.DueDate,
                    Doctor = new DoctorDto
                    {
                        IdDoctor = p.Doctor.IdDoctor,
                        FirstName = p.Doctor.FirstName,
                        LastName = p.Doctor.LastName
                    },
                    Medicaments = p.PrescriptionMedicaments.Select(pm => new MedicamentDto
                    {
                        IdMedicament = pm.Medicament.IdMedicament,
                        Name = pm.Medicament.Name,
                        Dose = pm.Dose,
                        Description = pm.Details
                    }).ToList()
                }).ToList()
            };
        }
    }
    
    public class PatientService : IPatientService
    {
        private readonly PrescriptionDbContext _context;
        
        public PatientService(PrescriptionDbContext context)
        {
            _context = context;
        }
        
        public async Task<int> CreateOrUpdatePatientAsync(PatientDto patientDto)
        {
            if (patientDto.IdPatient.HasValue)
            {
                var existingPatient = await _context.Patients.FindAsync(patientDto.IdPatient.Value);
                if (existingPatient != null)
                {
                    return existingPatient.IdPatient;
                }
            }
            
            var newPatient = new Patient
            {
                FirstName = patientDto.FirstName,
                LastName = patientDto.LastName,
                Birthdate = patientDto.Birthdate
            };
            
            _context.Patients.Add(newPatient);
            await _context.SaveChangesAsync();
            
            return newPatient.IdPatient;
        }
        
        public async Task<bool> PatientExistsAsync(int patientId)
        {
            return await _context.Patients.AnyAsync(p => p.IdPatient == patientId);
        }
    }
    
    public class MedicamentService : IMedicamentService
    {
        private readonly PrescriptionDbContext _context;
        
        public MedicamentService(PrescriptionDbContext context)
        {
            _context = context;
        }
        
        public async Task<bool> MedicamentExistsAsync(int medicamentId)
        {
            return await _context.Medicaments.AnyAsync(m => m.IdMedicament == medicamentId);
        }
        
        public async Task<bool> AllMedicamentsExistAsync(IEnumerable<int> medicamentIds)
        {
            var existingIds = await _context.Medicaments
                .Where(m => medicamentIds.Contains(m.IdMedicament))
                .Select(m => m.IdMedicament)
                .ToListAsync();
            
            return medicamentIds.All(id => existingIds.Contains(id));
        }
    }
}
