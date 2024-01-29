namespace WebService.Domain.Entities.Base
{
    public interface IEntity<TKey>
        {
            public TKey Id { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public DateTime CreatedDate { get; set; }
            public long CreatedUser { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public long? UpdatedUser { get; set; }
            public int RowStatus { get; set; }

        }
        public interface IEntity : IEntity<long>
        {

        }
    
}
