using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Domain.Entities
{
    public abstract class OptionalNamedEntity : Entity
    {
        public Name? Name
        {
            get;
            set
            {
                if (value == field)
                    return;

                field = value;
                OnNameChanged();
            }
        }

        public event EventHandler? NameChanged;

        protected virtual void OnNameChanged()
            => NameChanged?.Invoke(this, EventArgs.Empty);

        public Description? Description
        {
            get;
            set
            {
                if (value == field)
                    return;

                field = value;
                OnDescriptionChanged();
            }
        }

        public event EventHandler? DescriptionChanged;

        protected virtual void OnDescriptionChanged()
            => DescriptionChanged?.Invoke(this, EventArgs.Empty);
    }
}
