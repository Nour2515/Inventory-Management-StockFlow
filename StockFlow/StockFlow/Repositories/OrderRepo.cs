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
