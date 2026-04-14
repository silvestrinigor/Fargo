using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public class BarcodeRepository(FargoDbContext context) : IBarcodeRepository
{
    private readonly DbSet<Barcode> barcodes = context.Barcodes;

    public void Add(Barcode barcode) => barcodes.Add(barcode);

    public void Remove(Barcode barcode) => barcodes.Remove(barcode);

    public async Task<Barcode?> GetByGuid(Guid barcodeGuid, CancellationToken cancellationToken = default)
        => await barcodes
            .Include(b => b.Article)
            .Where(b => b.Guid == barcodeGuid)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Barcode>> GetByArticleGuid(Guid articleGuid, CancellationToken cancellationToken = default)
        => await barcodes
            .AsNoTracking()
            .Where(b => b.ArticleGuid == articleGuid)
            .OrderBy(b => b.Format)
            .ToListAsync(cancellationToken);
}
