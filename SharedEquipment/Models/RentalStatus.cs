using System;
using System.Collections.Generic;
using System.Text;

namespace SharedEquipment.Models
{
    public enum RentalStatus
    {
        Active = 1,
        Completed = 2,
        Canceled = 3,
        Overdue = 4,
        Extended = 5
    }

    public enum PaymentMethod
    {
        Cash = 1,
        CreditCard = 2,
        BankTransfer = 3,
        DigitalWallet = 4
    }

    public enum EquipmentCondition
    {
        Excellent = 1,
        Good = 2,
        Fair = 3,
        NeedsMaintenance = 4,
        Damaged = 5,
    }

    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        Staff = 3,
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4
    }
}
