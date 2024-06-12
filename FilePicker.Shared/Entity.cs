namespace FilePicker.Shared
{
    public abstract class Entity
    {
        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        protected Entity(Guid Id)
        {
            this.Id = Id;
        }

        public Guid Id { get; }
        public bool Status { get; set; }
    }
}
