using EquipmentProject.Data;
using Microsoft.EntityFrameworkCore;
using SharedEquipment.Models;
using System.Net.Http.Json;
using static EquipmentProject.Data.LocalDbContext;

namespace EquipmentProject.Services
{
    public class SyncService
    {
        private readonly LocalDbContext _localDb;
        private readonly HttpClient _http;

        public SyncService(LocalDbContext localDb, HttpClient http)
        {
            _localDb = localDb;
            _http = http;
        }

        public async Task<SyncResult> SyncCustomersAsync()
        {
            try
            {
                var lastSyncLog = await _localDb.SyncLogs.FirstOrDefaultAsync();
                var lastSyncDate = lastSyncLog?.LastSyncDate ?? DateTime.MinValue;

                var localChanges = await _localDb.Customers
                    .Where(c => c.CreatedAt > lastSyncDate || c.UpdatedAt > lastSyncDate)
                    .OrderBy(c => c.Id)
                    .ToListAsync();

                if (localChanges.Any())
                {
                    var response = await _http.PostAsJsonAsync("api/customers/sync-local", localChanges);
                    if (!response.IsSuccessStatusCode)
                    {
                        return new SyncResult
                        {
                            IsSuccess = false,
                            Message = "فشل إرسال تغييرات العملاء إلى الخادم."
                        };
                    }
                }

                var encodedLastSyncDate = Uri.EscapeDataString(lastSyncDate.ToUniversalTime().ToString("o"));
                var serverUpdates = await _http.GetFromJsonAsync<List<Customer>>($"api/customers/sync/{encodedLastSyncDate}");

                if (serverUpdates != null)
                {
                    foreach (var serverCustomer in serverUpdates)
                    {
                        var localCustomer = await _localDb.Customers
                            .FirstOrDefaultAsync(c => c.IdentityNumber == serverCustomer.IdentityNumber);

                        if (localCustomer == null)
                        {
                            await _localDb.Customers.AddAsync(new Customer
                            {
                                FullName = serverCustomer.FullName,
                                PhoneNumber = serverCustomer.PhoneNumber,
                                Address = serverCustomer.Address,
                                Email = serverCustomer.Email,
                                IdentityNumber = serverCustomer.IdentityNumber,
                                RegistraionDate = serverCustomer.RegistraionDate,
                                CreatedAt = serverCustomer.CreatedAt,
                                UpdatedAt = serverCustomer.UpdatedAt,
                                CreatedBy = serverCustomer.CreatedBy
                            });

                            continue;
                        }

                        localCustomer.FullName = serverCustomer.FullName;
                        localCustomer.PhoneNumber = serverCustomer.PhoneNumber;
                        localCustomer.Address = serverCustomer.Address;
                        localCustomer.Email = serverCustomer.Email;
                        localCustomer.RegistraionDate = serverCustomer.RegistraionDate;
                        localCustomer.CreatedAt = serverCustomer.CreatedAt;
                        localCustomer.UpdatedAt = serverCustomer.UpdatedAt;
                        localCustomer.CreatedBy = serverCustomer.CreatedBy;
                    }
                }

                if (lastSyncLog == null)
                {
                    _localDb.SyncLogs.Add(new SyncLog { LastSyncDate = DateTime.UtcNow });
                }
                else
                {
                    lastSyncLog.LastSyncDate = DateTime.UtcNow;
                }

                await _localDb.SaveChangesAsync();

                return new SyncResult
                {
                    IsSuccess = true,
                    Message = "تمت مزامنة العملاء بنجاح."
                };
            }
            catch (Exception ex)
            {
                return new SyncResult
                {
                    IsSuccess = false,
                    Message = $"حدث خطأ أثناء المزامنة: {ex.Message}"
                };
            }
        }
    }

    public class SyncResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
