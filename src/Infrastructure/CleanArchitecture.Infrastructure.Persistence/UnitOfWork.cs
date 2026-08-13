using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Common;
using System.Collections;

namespace CleanArchitecture.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Hashtable _repositories;
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _repositories = new Hashtable();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
        _disposed = true;
    }

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T).Name;

        if (!_repositories.Contains(type))
        {
            var repositoryType = typeof(Repository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);
            _repositories.Add(type, repositoryInstance);
        }

        return (IRepository<T>)_repositories[type]!;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Mengambil semua domain events dari entitas yang saat ini di-track oleh EF Core.
    /// Dipanggil oleh Behaviour (seperti DomainEventPublishingBehaviour) setelah SaveChangesAsync sukses.
    /// Metode ini memisahkan (decoupling) domain events dari siklus penyimpanan EF Core, 
    /// sehingga event tidak diproses jika penyimpanan gagal.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> GetAndClearDomainEvents()
    {
        // 1. Cari semua entitas yang mewarisi BaseEntity dan memiliki minimal 1 domain event di antreannya.
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // 2. Kumpulkan semua event tersebut ke dalam satu list (flattening).
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // 3. Wajib: Bersihkan list event di tiap entitas setelah diambil agar tidak ter-publish berulang kali
        // jika DbContext yang sama (Scoped) dipakai kembali di operasi selanjutnya.
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        return domainEvents;
    }
}
