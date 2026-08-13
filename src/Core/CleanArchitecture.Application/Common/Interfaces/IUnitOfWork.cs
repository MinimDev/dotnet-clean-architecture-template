using CleanArchitecture.Domain.Common;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Mengambil semua domain events dari entitas yang di-track dan mengosongkan listnya.
    /// Digunakan oleh Pipeline Behaviour untuk mem-publish event setelah transaksi sukses.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> GetAndClearDomainEvents();
}
