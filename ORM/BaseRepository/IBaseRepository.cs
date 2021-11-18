namespace ORM.Repository
{
    public interface IBaseRepository<T> where T : class, new()
    {
        public void Add(T model);
        public void Update(T newModel);
        public void Delete(int id);
        public T Get(int id);
    }
}
