using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UdemyUnitTestEx.Web.Models;

namespace UdemyUnitTestEx.Web.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly UdemyUnitTestDBContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(UdemyUnitTestDBContext context)
        {
            _context = context;

            _dbSet = _context.Set<TEntity>();
        }


        public async Task Create(TEntity entity)
        {
            // Task oldugu icin asenkron olmasi gerekir.
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> GetById(int Id)
        {
            return await _dbSet.FindAsync(Id);
        }

        public void Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified; 

            //_dbSet.Update(entity);

            _context.SaveChanges();
        }
    }
}
