using EquipmentAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedEquipment.Models;
using System.Globalization;

namespace EquipmentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CustomersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost("sync-local")]
        public async Task<IActionResult> SyncLocal([FromBody] List<Customer> customers)
        {
            foreach (var customer in customers)
            {
                if (string.IsNullOrWhiteSpace(customer.IdentityNumber))
                {
                    continue;
                }

                var existingCustomer = await _db.Customers
                    .FirstOrDefaultAsync(c => c.IdentityNumber == customer.IdentityNumber);

                if (existingCustomer == null)
                {
                    await _db.Customers.AddAsync(new Customer
                    {
                        FullName = customer.FullName,
                        PhoneNumber = customer.PhoneNumber,
                        Address = customer.Address,
                        Email = customer.Email,
                        IdentityNumber = customer.IdentityNumber,
                        RegistraionDate = customer.RegistraionDate == default ? DateTime.UtcNow : customer.RegistraionDate,
                        CreatedAt = customer.CreatedAt == default ? DateTime.UtcNow : customer.CreatedAt,
                        UpdatedAt = customer.UpdatedAt == default ? DateTime.UtcNow : customer.UpdatedAt,
                        CreatedBy = customer.CreatedBy
                    });

                    continue;
                }

                existingCustomer.FullName = customer.FullName;
                existingCustomer.PhoneNumber = customer.PhoneNumber;
                existingCustomer.Address = customer.Address;
                existingCustomer.Email = customer.Email;
                existingCustomer.RegistraionDate = customer.RegistraionDate == default
                    ? existingCustomer.RegistraionDate
                    : customer.RegistraionDate;
                existingCustomer.UpdatedAt = customer.UpdatedAt == default ? DateTime.UtcNow : customer.UpdatedAt;
                existingCustomer.CreatedBy = customer.CreatedBy;
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("sync/{lastSyncUtc}")]
        public async Task<ActionResult<List<Customer>>> GetServerUpdates(string lastSyncUtc)
        {
            if (!DateTime.TryParse(
                    lastSyncUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var lastSyncDate))
            {
                return BadRequest("Invalid sync date.");
            }

            var customers = await _db.Customers
                .Where(c => c.CreatedAt > lastSyncDate || c.UpdatedAt > lastSyncDate)
                .OrderBy(c => c.UpdatedAt)
                .ToListAsync();

            return Ok(customers);
        }
    }
}
