namespace WebService.Domain.Entities.Base
{
    public abstract class BaseEntity : IEntity
    {
        public virtual long Id { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long? UpdatedUser { get; set; }
        public int RowStatus { get; set; } = 1;
    }
}
