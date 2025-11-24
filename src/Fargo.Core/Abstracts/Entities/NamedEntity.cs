namespace Fargo.Domain.Abstracts.Entities
{
    public abstract class NamedEntity : Entity
    {
        public required string Name { get; set => SetField(ref field, value, OnNameChanged); } = string.Empty;
        public string Description { get; set => SetField(ref field, value, OnDescriptionChanged); } = string.Empty;

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