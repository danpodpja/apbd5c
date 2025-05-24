using Microsoft.AspNetCore.Mvc;
using apbd5c.DTOs;
using apbd5c.Services.Interfaces;

namespace apbd5c.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        
        public PrescriptionsController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }
        
        [HttpPost]
        public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionDto prescriptionDto)
        {
            try
            {
                var prescriptionId = await _prescriptionService.CreatePrescriptionAsync(prescriptionDto);
                return CreatedAtAction(nameof(GetPatientDetails), new { patientId = prescriptionDto.Patient.IdPatient }, new { IdPrescription = prescriptionId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPatientDetails(int patientId)
        {
            try
            {
                var patientDetails = await _prescriptionService.GetPatientDetailsAsync(patientId);
                return Ok(patientDetails);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}