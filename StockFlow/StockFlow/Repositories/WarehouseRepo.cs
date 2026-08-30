using StockFlow.Data;
using StockFlow.IRepository;
using StockFlow.Models;

namespace StockFlow.Repositories
{
    public class WarehouseRepo : GenericRepository<Warehouse>
    {
        private readonly AppDbContext _context;

        public WarehouseRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
