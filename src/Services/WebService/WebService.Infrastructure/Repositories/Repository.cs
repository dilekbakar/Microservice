using System.Linq.Expressions;
using WebService.Application.Interfaces;
using WebService.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;


namespace WebService.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly DbContext dbContext;
        protected DbSet<TEntity> Entity => dbContext.Set<TEntity>();
        public Repository(DbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        #region Insert Methods

        public virtual async Task<int> AddAsync(TEntity entity)
        {
            await this.Entity.AddAsync(entity);
            return await dbContext.SaveChangesAsync();
        }

        public virtual int Add(TEntity entity)
        {
            this.Entity.Add(entity);
            return dbContext.SaveChanges();
        }

        public virtual async Task<int> AddAsync(IEnumerable<TEntity> entities)
        {
            if (entities != null && !entities.Any())
                return 0;

            await Entity.AddRangeAsync(entities);
            return await dbContext.SaveChangesAsync();
        }

        public virtual int Add(IEnumerable<TEntity> entities)
        {
            if (entities != null && !entities.Any())
                return 0;

            Entity.AddRange(Entity);
            return dbContext.SaveChanges();
        }

        #endregion

        #region Update Methods

        public virtual async Task<int> UpdateAsync(TEntity entity)
        {
            this.Entity.Attach(entity);
            dbContext.Entry(entity).State = EntityState.Modified;

            return await dbContext.SaveChangesAsync();
        }

        public virtual int Update(TEntity entity)
        {
            this.Entity.Attach(entity);
            dbContext.Entry(entity).State = EntityState.Modified;

            return dbContext.SaveChanges();
        }

        #endregion

        #region Delete Methods

        public virtual async Task<int> DeleteAsync(TEntity entity, bool isDeleted = false)
        {
            if (isDeleted)
            {
                if (dbContext.Entry(entity).State == EntityState.Detached)
                {
                    this.Entity.Attach(entity);
                }

                this.Entity.Remove(entity);

                return await dbContext.SaveChangesAsync();
            }

            else
            {
                entity.IsDeleted = true;
                return await UpdateAsync(entity);
            }
        }

        public virtual Task<int> DeleteAsync(long id, bool isDeleted = false)
        {
            var entity = this.Entity.Find(id);
            return DeleteAsync(entity, isDeleted);
        }

        public virtual int Delete(long id)
        {
            var entity = this.Entity.Find(id);
            return Delete(entity);
        }

        public virtual int Delete(TEntity entity)
        {
            if (dbContext.Entry(entity).State == EntityState.Detached)
            {
                this.Entity.Attach(entity);
            }

            this.Entity.Remove(entity);

            return dbContext.SaveChanges();
        }

        public virtual bool DeleteRange(Expression<Func<TEntity, bool>> predicate)
        {
            dbContext.RemoveRange(Entity.Where(predicate));
            return dbContext.SaveChanges() > 0;
        }

        public virtual async Task<bool> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate)
        {
            dbContext.RemoveRange(Entity.Where(predicate));
            return await dbContext.SaveChangesAsync() > 0;
        }

        #endregion

        #region AddOrUpdate Methods

        public virtual Task<int> AddOrUpdateAsync(TEntity entity)
        {
            // check the entity with the id already tracked
            if (!this.Entity.Local.Any(i => EqualityComparer<long>.Default.Equals(i.Id, entity.Id)))
                dbContext.Update(entity);

            return dbContext.SaveChangesAsync();
        }

        public virtual int AddOrUpdate(TEntity entity)
        {
            if (!this.Entity.Local.Any(i => EqualityComparer<long>.Default.Equals(i.Id, entity.Id)))
                dbContext.Update(entity);

            return dbContext.SaveChanges();
        }

        #endregion

        #region Get Methods

        public virtual IQueryable<TEntity> AsQueryable() => Entity.AsQueryable();
        public virtual IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate, bool noTracking = true, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = Entity.AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            query = ApplyIncludes(query, includes);

            if (noTracking)
                query = query.AsNoTracking();

            return query;
        }

        public virtual Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool noTracking = true, params Expression<Func<TEntity, object>>[] includes)
        {
            return Get(predicate, noTracking, includes).FirstOrDefaultAsync();
        }

        public virtual async Task<List<TEntity>> GetList(Expression<Func<TEntity, bool>> predicate, bool noTracking = true, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = Entity;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                query = query.Include(include);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (noTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public virtual async Task<List<TEntity>> GetAll(bool noTracking = true)
        {
            if (noTracking)
                return await Entity.AsNoTracking().Where(i => !i.IsDeleted).ToListAsync();

            return await Entity.ToListAsync();
        }


        public virtual async Task<TEntity> GetByIdAsync(long id, bool noTracking = true, params Expression<Func<TEntity, object>>[] includes)
        {
            TEntity found = await Entity.FindAsync(id);

            if (found == null)
                return null;

            if (noTracking)
                dbContext.Entry(found).State = EntityState.Detached;

            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                dbContext.Entry(found).Reference(include).Load();
            }

            return found;
        }

        public virtual async Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate, bool noTracking = true, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = Entity;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            query = ApplyIncludes(query, includes);

            if (noTracking)
                query = query.AsNoTracking();

            return await query.SingleOrDefaultAsync();

        }

        #endregion

        #region Bulk Methods

        public virtual Task BulkDeleteById(IEnumerable<long> ids)
        {
            if (ids != null && !ids.Any())
                return Task.CompletedTask;

            dbContext.RemoveRange(Entity.Where(i => ids.Contains(i.Id)));
            return dbContext.SaveChangesAsync();
        }

        public virtual Task BulkDelete(Expression<Func<TEntity, bool>> predicate)
        {
            dbContext.RemoveRange(Entity.Where(predicate));
            return dbContext.SaveChangesAsync();
        }

        public virtual Task BulkDelete(IEnumerable<TEntity> entities)
        {
            if (entities != null && !entities.Any())
                return Task.CompletedTask;

            Entity.RemoveRange(entities);
            return dbContext.SaveChangesAsync();
        }

        public virtual Task BulkUpdate(IEnumerable<TEntity> entities)
        {
            if (entities != null && !entities.Any())
                return Task.CompletedTask;

            foreach (var entityItem in entities)
            {
                Entity.Update(entityItem);
            }

            return dbContext.SaveChangesAsync();
        }

        public virtual async Task BulkAdd(IEnumerable<TEntity> entities)
        {
            if (entities != null && !entities.Any())
                await Task.CompletedTask;

            await Entity.AddRangeAsync(entities);

            await dbContext.SaveChangesAsync();
        }

        #endregion

        #region SaveChanges Methods

        public Task<int> SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return dbContext.SaveChanges();
        }

        #endregion

        private static IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, params Expression<Func<TEntity, object>>[] includes)
        {
            if (includes != null)
            {
                foreach (var includeItem in includes)
                {
                    query = query.Include(includeItem);
                }
            }

            return query;
        }
    }
}
