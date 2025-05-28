using System;
using System.Collections.Generic;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.Interfaces
{
    public interface IPurchaseBL
    {
        // Основные операции с заказами
        Purchase GetPurchaseById(int id);
        List<Purchase> GetAllPurchases();
        List<Purchase> GetPurchasesByCustomer(int customerId);
        List<Purchase> GetPurchasesByStatus(string status);
        List<Purchase> SearchPurchases(string searchTerm);
        
        // CRUD операции
        bool CreatePurchase(Purchase purchase);
        bool UpdatePurchase(Purchase purchase);
        bool DeletePurchase(int id);
        
        // Дополнительные операции
        int GetPurchasesCount();
        List<Purchase> GetPagedPurchases(int page, int pageSize);
        List<Purchase> GetPurchasesByDateRange(DateTime startDate, DateTime endDate);
        
        // Операции со статусом заказа
        bool UpdatePurchaseStatus(int purchaseId, string newStatus);
        bool CancelPurchase(int purchaseId, string reason);
        
        // Операции с элементами заказа
        List<PurchaseItem> GetPurchaseItems(int purchaseId);
        bool AddPurchaseItem(PurchaseItem item);
        bool RemovePurchaseItem(int itemId);
        
        // Аналитика
        decimal GetTotalRevenue();
        decimal GetRevenueByDateRange(DateTime startDate, DateTime endDate);
        List<Purchase> GetRecentPurchases(int count);
    }
} 