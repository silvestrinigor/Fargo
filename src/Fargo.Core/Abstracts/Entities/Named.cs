namespace Fargo.Domain.Abstracts.Entities
{
    public abstract class Named : Entity
    {
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public string? Name { get; set => SetField(ref field, value, OnNameChanged); }
        public string? Description { get; set => SetField(ref field, value, OnDescriptionChanged); }

        public event EventHandler? NameChanged;
        public event EventHandler? DescriptionChanged;

        private void OnNameChanged()
        {
            NameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDescriptionChanged()
        {
            DescriptionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected static void SetField<T>(ref T field, T value, Action onChanged)
        {
            field = value;

            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            onChanged();
        }
    }
}
