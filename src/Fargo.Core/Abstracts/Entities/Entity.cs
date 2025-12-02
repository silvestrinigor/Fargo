namespace Fargo.Domain.Abstracts.Entities
{
    public abstract class Entity : IEquatable<Entity>
    {
        public Guid Guid { get; init; } = Guid.NewGuid();
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public string? Name { get; set => SetField(ref field, value, OnNameChanged); }
        public string? Description { get; set => SetField(ref field, value, OnDescriptionChanged); }
        public virtual Guid? ParentGuid { get; internal set; }

        public override bool Equals(object? obj) =>
            obj is Entity other && Guid.Equals(other.Guid);

        public bool Equals(Entity? other) =>
            other is not null && (ReferenceEquals(this, other) || Guid.Equals(other.Guid));

        public override int GetHashCode() =>
            Guid.GetHashCode();

        public static bool operator ==(Entity? left, Entity? right) =>
            ReferenceEquals(left, right)
            || (left is not null && right is not null && left.Equals(right));

        public static bool operator !=(Entity? left, Entity? right) =>
            !(left == right);

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
