using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using BeautyMoldova.Database;

namespace BeautyMoldova.Application
{
    /// <summary>
    /// Базовый класс для Business Logic слоя, содержащий основные CRUD операции
    /// </summary>
    public abstract class BaseApi : IDisposable
    {
        protected readonly ShopDataContext _context;

        protected BaseApi()
        {
            _context = new ShopDataContext();
        }

        protected BaseApi(ShopDataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Базовые CRUD операции

        /// <summary>
        /// Получить сущность по ID
        /// </summary>
        protected virtual T GetById<T>(int id) where T : class
        {
            return _context.Set<T>().Find(id);
        }

        /// <summary>
        /// Получить все сущности указанного типа
        /// </summary>
        protected virtual List<T> GetAll<T>() where T : class
        {
            return _context.Set<T>().ToList();
        }

        /// <summary>
        /// Получить все сущности с возможностью включения связанных данных
        /// </summary>
        protected virtual IQueryable<T> GetAllQueryable<T>() where T : class
        {
            return _context.Set<T>().AsQueryable();
        }

        /// <summary>
        /// Создать новую сущность
        /// </summary>
        protected virtual bool Create<T>(T entity) where T : class
        {
            try
            {
                _context.Set<T>().Add(entity);
                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Обновить существующую сущность
        /// </summary>
        protected virtual bool Update<T>(T entity) where T : class
        {
            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Удалить сущность по ID
        /// </summary>
        protected virtual bool Delete<T>(int id) where T : class
        {
            try
            {
                var entity = GetById<T>(id);
                if (entity != null)
                {
                    _context.Set<T>().Remove(entity);
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Удалить сущность
        /// </summary>
        protected virtual bool Delete<T>(T entity) where T : class
        {
            try
            {
                _context.Set<T>().Remove(entity);
                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получить количество сущностей
        /// </summary>
        protected virtual int Count<T>() where T : class
        {
            return _context.Set<T>().Count();
        }

        /// <summary>
        /// Получить сущности с пагинацией
        /// </summary>
        protected virtual List<T> GetPaged<T>(int page, int pageSize) where T : class
        {
            return _context.Set<T>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Проверить существование сущности по ID
        /// </summary>
        protected virtual bool Exists<T>(int id) where T : class
        {
            return GetById<T>(id) != null;
        }

        /// <summary>
        /// Сохранить изменения в контексте
        /// </summary>
        protected virtual bool SaveChanges()
        {
            try
            {
                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
} 