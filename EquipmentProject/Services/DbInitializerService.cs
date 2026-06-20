using System.Data;
using System.Data.Common;
using EquipmentProject.Data;
using Microsoft.EntityFrameworkCore;
using SharedEquipment.Models;

namespace EquipmentProject.Services
{
    public sealed class DbInitializerService
    {
        private const string DefaultDateText = "0001-01-01T00:00:00";
        private const string DefaultAdminPhoneNumber = "0945596122";
        private const string DefaultAdminPassword = "yousef";
        private const string DefaultAdminUsername = "admin";

        private readonly LocalDbContext _db;
        private readonly PasswordHasher _passwordHasher;

        public DbInitializerService(LocalDbContext db, PasswordHasher passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        public async Task InitializeAsync()
        {
            await _db.Database.EnsureCreatedAsync();
            await EnsureUsersSchemaAsync();
            await EnsureRentalWorkflowSchemaAsync();
            await SeedDefaultAdminAsync();
        }

        private async Task EnsureUsersSchemaAsync()
        {
            var connection = _db.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                if (!await TableExistsAsync(connection, "Users"))
                {
                    return;
                }

                await EnsureColumnAsync(connection, "Users", "PhoneNumber", "TEXT NOT NULL DEFAULT ''");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private async Task EnsureRentalWorkflowSchemaAsync()
        {
            var connection = _db.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                if (await TableExistsAsync(connection, "Equipments"))
                {
                    await EnsureColumnAsync(connection, "Equipments", "SerialNumber", "TEXT NULL");
                    await EnsureColumnAsync(connection, "Equipments", "DailyRentalRate", "TEXT NOT NULL DEFAULT 0");
                    await EnsureColumnAsync(connection, "Equipments", "IsRented", "INTEGER NOT NULL DEFAULT 0");
                    await EnsureColumnAsync(connection, "Equipments", "CurrentRentalContractId", "INTEGER NULL");

                    await ExecuteNonQueryAsync(connection, """
                        UPDATE Equipments
                        SET DailyRentalRate = TotalCost
                        WHERE CAST(IFNULL(DailyRentalRate, 0) AS REAL) = 0
                          AND IFNULL(TotalCost, '') <> '';
                        """);

                    await ExecuteNonQueryAsync(connection, """
                        UPDATE Equipments
                        SET IsRented = 1
                        WHERE CustomerId IS NOT NULL
                          AND IFNULL(IsRented, 0) = 0;
                        """);
                }

                if (await TableExistsAsync(connection, "RentalContracts"))
                {
                    await EnsureColumnAsync(connection, "RentalContracts", "ContractNumber", "TEXT NOT NULL DEFAULT ''");
                    await EnsureColumnAsync(connection, "RentalContracts", "CustomerId", "INTEGER NULL");
                    await EnsureColumnAsync(connection, "RentalContracts", "StartDate", $"TEXT NOT NULL DEFAULT '{DefaultDateText}'");
                    await EnsureColumnAsync(connection, "RentalContracts", "EndDate", $"TEXT NOT NULL DEFAULT '{DefaultDateText}'");
                    await EnsureColumnAsync(connection, "RentalContracts", "ActualReturnDate", "TEXT NULL");
                    await EnsureColumnAsync(connection, "RentalContracts", "TotalAmount", "TEXT NOT NULL DEFAULT 0");
                    await EnsureColumnAsync(connection, "RentalContracts", "DepositAmount", "TEXT NOT NULL DEFAULT 0");
                    await EnsureColumnAsync(connection, "RentalContracts", "Status", $"INTEGER NOT NULL DEFAULT {(int)RentalStatus.Active}");

                    await ExecuteNonQueryAsync(connection, """
                        UPDATE RentalContracts
                        SET ContractNumber = 'CTR-' || printf('%05d', Id)
                        WHERE IFNULL(ContractNumber, '') = '';
                        """);

                    await ExecuteNonQueryAsync(connection, """
                        UPDATE RentalContracts
                        SET StartDate = CreatedAt
                        WHERE IFNULL(StartDate, '') = '0001-01-01T00:00:00';
                        """);

                    await ExecuteNonQueryAsync(connection, """
                        UPDATE RentalContracts
                        SET EndDate = CreatedAt
                        WHERE IFNULL(EndDate, '') = '0001-01-01T00:00:00';
                        """);

                    await ExecuteNonQueryAsync(connection, """
                        UPDATE RentalContracts
                        SET TotalAmount = DailyRate
                        WHERE CAST(IFNULL(TotalAmount, 0) AS REAL) = 0
                          AND CAST(IFNULL(DailyRate, 0) AS REAL) > 0;
                        """);
                }

                if (await TableExistsAsync(connection, "RentalDetails"))
                {
                    await EnsureColumnAsync(connection, "RentalDetails", "EquipmentId", "INTEGER NULL");
                    await EnsureColumnAsync(connection, "RentalDetails", "EquipmentName", "TEXT NOT NULL DEFAULT ''");
                }

                if (await TableExistsAsync(connection, "Payments"))
                {
                    await EnsureColumnAsync(connection, "Payments", "RentalContractId", "INTEGER NULL");
                    await EnsureColumnAsync(connection, "Payments", "ContractNumber", "TEXT NULL");
                    await EnsureColumnAsync(connection, "Payments", "CustomerName", "TEXT NULL");
                }

                if (await TableExistsAsync(connection, "Maintenances"))
                {
                    await EnsureColumnAsync(connection, "Maintenances", "EquipmentId", "INTEGER NULL");
                    await EnsureColumnAsync(connection, "Maintenances", "EquipmentName", "TEXT NOT NULL DEFAULT ''");
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private async Task SeedDefaultAdminAsync()
        {
            var activeNonAdminUsers = await _db.Users
                .Where(u => u.Username != DefaultAdminUsername && u.IsActive)
                .ToListAsync();

            foreach (var user in activeNonAdminUsers)
            {
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
            }

            var existingAdmin = await _db.Users.FirstOrDefaultAsync(u => u.Username == DefaultAdminUsername);
            if (existingAdmin != null)
            {
                var changed = activeNonAdminUsers.Count > 0;

                if (existingAdmin.PhoneNumber != DefaultAdminPhoneNumber)
                {
                    existingAdmin.PhoneNumber = DefaultAdminPhoneNumber;
                    changed = true;
                }

                if (!_passwordHasher.VerifyPassword(DefaultAdminPassword, existingAdmin.PasswordHash))
                {
                    existingAdmin.PasswordHash = _passwordHasher.HashPassword(DefaultAdminPassword);
                    changed = true;
                }

                if (!existingAdmin.IsActive)
                {
                    existingAdmin.IsActive = true;
                    changed = true;
                }

                if (changed)
                {
                    existingAdmin.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }

                return;
            }

            var now = DateTime.UtcNow;
            _db.Users.Add(new User
            {
                Username = DefaultAdminUsername,
                Email = "admin@equipment.local",
                PhoneNumber = DefaultAdminPhoneNumber,
                PasswordHash = _passwordHasher.HashPassword(DefaultAdminPassword),
                FullName = "مدير النظام",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            await _db.SaveChangesAsync();
        }

        private static async Task<bool> TableExistsAsync(DbConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '{tableName}';";
            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        }

        private static async Task EnsureColumnAsync(DbConnection connection, string tableName, string columnName, string definition)
        {
            if (await ColumnExistsAsync(connection, tableName, columnName))
            {
                return;
            }

            using var command = connection.CreateCommand();
            command.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition};";
            await command.ExecuteNonQueryAsync();
        }

        private static async Task<bool> ColumnExistsAsync(DbConnection connection, string tableName, string columnName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info('{tableName}');";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static async Task ExecuteNonQueryAsync(DbConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }
    }
}
