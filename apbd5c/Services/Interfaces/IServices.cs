using apbd5c.DTOs;


namespace apbd5c.Services.Interfaces
{
    public interface IPrescriptionService
    {
        Task<int> CreatePrescriptionAsync(CreatePrescriptionDto prescriptionDto);
        Task<PatientDetailsDto> GetPatientDetailsAsync(int patientId);
    }
    
    public interface IPatientService
    {
        Task<int> CreateOrUpdatePatientAsync(PatientDto patientDto);
        Task<bool> PatientExistsAsync(int patientId);
    }
    
    public interface IMedicamentService
    {
        Task<bool> MedicamentExistsAsync(int medicamentId);
        Task<bool> AllMedicamentsExistAsync(IEnumerable<int> medicamentIds);
    }
}