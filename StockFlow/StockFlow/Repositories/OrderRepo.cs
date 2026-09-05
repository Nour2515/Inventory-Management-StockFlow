using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.IRepository;
using StockFlow.Models;

namespace StockFlow.Repositories
{
    public class OrderRepo : GenericRepository<Order>, IOrderRepo
    {
        private readonly AppDbContext _context;
        public OrderRepo(AppDbContext context) : base(context) {
        _context = context;
        }

        public override async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders.Include(o => o.OrderItems)
                    .ThenInclude(item => item.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == id);
        }
        public async Task<Order?> GetByIdWithItemsAsync(int id)
        {
            return await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(item => item.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == id);
        }

        public Task<List<Order>> GetByUserIdAsync(int id)
        {
            return _context.Orders.Where(u=>u.UserId==id).Include(o=>o.OrderItems).ThenInclude(item => item.Product).AsNoTracking().ToListAsync();

        }

        public Task<List<Order>> GetOrderWithItemsAsync()
        {
            return _context.Orders.Include(o=>o.OrderItems).ThenInclude(p=>p.Product).AsNoTracking().ToListAsync();
        }

    }
}   
