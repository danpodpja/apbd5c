using Microsoft.EntityFrameworkCore;
using apbd5c.Data;
using apbd5c.DTOs;
using apbd5c.Models;
using apbd5c.Services;
using Xunit;

namespace apbd5c.Tests
{
    public class PrescriptionServiceTests : IDisposable
    {
        private readonly PrescriptionDbContext _context;
        private readonly PrescriptionService _prescriptionService;
        private readonly PatientService _patientService;
        private readonly MedicamentService _medicamentService;
        
        public PrescriptionServiceTests()
        {
            var options = new DbContextOptionsBuilder<PrescriptionDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new PrescriptionDbContext(options);
            _patientService = new PatientService(_context);
            _medicamentService = new MedicamentService(_context);
            _prescriptionService = new PrescriptionService(_context, _patientService, _medicamentService);
            
            SeedTestData();
        }
        
        private void SeedTestData()
        {
            var doctor = new Doctor
            {
                IdDoctor = 1,
                FirstName = "Jan",
                LastName = "Kowalski",
                Email = "jan.kowalski@example.com"
            };
            
            var medicament = new Medicament
            {
                IdMedicament = 1,
                Name = "Aspiryna",
                Description = "Lek przeciwbólowy",
                Type = "Tabletki"
            };
            
            _context.Doctors.Add(doctor);
            _context.Medicaments.Add(medicament);
            _context.SaveChanges();
        }
        
        [Fact]
        public async Task CreatePrescription_ValidData_ShouldCreatePrescription()
        {
            // Arrange
            var prescriptionDto = new CreatePrescriptionDto
            {
                Patient = new PatientDto
                {
                    FirstName = "Anna",
                    LastName = "Nowak",
                    Birthdate = new DateTime(1990, 1, 1)
                },
                Medicaments = new List<MedicamentRequestDto>
                {
                    new MedicamentRequestDto { IdMedicament = 1, Dose = 2, Description = "Dwa razy dziennie" }
                },
                Date = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7),
                IdDoctor = 1
            };
            
            // Act
            var result = await _prescriptionService.CreatePrescriptionAsync(prescriptionDto);
            
            // Assert
            Assert.True(result > 0);
            var prescription = await _context.Prescriptions.FindAsync(result);
            Assert.NotNull(prescription);
        }
        
        [Fact]
        public async Task CreatePrescription_InvalidDueDate_ShouldThrowException()
        {
            // Arrange
            var prescriptionDto = new CreatePrescriptionDto
            {
                Patient = new PatientDto { FirstName = "Test", LastName = "Test", Birthdate = DateTime.Now },
                Medicaments = new List<MedicamentRequestDto>
                {
                    new MedicamentRequestDto { IdMedicament = 1, Dose = 1, Description = "Test" }
                },
                Date = DateTime.Now,
                DueDate = DateTime.Now.AddDays(-1),
                IdDoctor = 1
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _prescriptionService.CreatePrescriptionAsync(prescriptionDto));
        }
        
        [Fact]
        public async Task CreatePrescription_TooManyMedicaments_ShouldThrowException()
        {
            // Arrange
            var medicaments = new List<MedicamentRequestDto>();
            for (int i = 0; i < 11; i++)
            {
                medicaments.Add(new MedicamentRequestDto { IdMedicament = 1, Dose = 1, Description = "Test" });
            }
            
            var prescriptionDto = new CreatePrescriptionDto
            {
                Patient = new PatientDto { FirstName = "Test", LastName = "Test", Birthdate = DateTime.Now },
                Medicaments = medicaments,
                Date = DateTime.Now,
                DueDate = DateTime.Now.AddDays(1),
                IdDoctor = 1
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _prescriptionService.CreatePrescriptionAsync(prescriptionDto));
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
