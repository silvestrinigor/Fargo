using UnitsNet;

namespace Fargo.Domain.ValueObjects
{
    public class Cuboid
    {
        public Cuboid()
        {
        }

        public Cuboid(Length? x, Length? y, Length? z)
        {
            SetDimensions(x, y, z);
        }

        public Cuboid(Length? x, Length? y, Volume? v)
        {
            SetDimensions(x, y, v);
        }

        public Cuboid(Length? x, Volume? v, Length? z)
        {
            SetDimensions(x, v, z);
        }

        public Cuboid(Volume? v, Length? y, Length? z)
        {
            SetDimensions(v, y, z);
        } 

        public Length? X
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }

                if (IsXCalculableFromYZV)
                {
                    return V / Y / Z;
                }

                return null;
            }

            private set
            {
                if (field is null && IsXCalculableFromYZV)
                {
                    throw new InvalidOperationException("Cannot set length x when the value is calculable from other properties.");
                }

                if (value < Length.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(X), "Cannot be negative.");
                }

                field = value;
            }
        }

        private bool IsXCalculableFromYZV
            => Y is not null && Z is not null && V is not null;

        public Length? Y
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }

                if (IsYCalculableFromXZV)
                {
                    return V / X / Z;
                }

                return null;
            }

            private set
            {
                if (field is null && IsYCalculableFromXZV)
                {
                    throw new InvalidOperationException("Cannot set length y when the value is calculable from other properties.");
                }

                if (value < Length.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(Y), "Cannot be negative.");
                }

                field = value;
            }
        }

        private bool IsYCalculableFromXZV
            => X is not null && Z is not null && V is not null;

        public Length? Z
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }

                if (IsZCalculableFromXYV)
                {
                    return V / X / Y;
                }

                return null;
            }

            private set
            {
                if (field is null && IsZCalculableFromXYV)
                {
                    throw new InvalidOperationException("Cannot set length z when the value is calculable from other properties.");
                }

                if (value < Length.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(Z), "Cannot be negative.");
                }

                field = value;
            }
        }

        private bool IsZCalculableFromXYV
            => X is not null && Y is not null && V is not null;

        public Volume? V
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }

                if (IsVCalculableFromXYZ)
                {
                    return X * Y * Z;
                }

                return null;
            }

            private set
            {
                if (field is null && IsVCalculableFromXYZ)
                {
                    throw new InvalidOperationException("Cannot set volume when the value is calculable from other properties.");
                }

                if (value < Volume.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(V), "Cannot be negative.");
                }

                field = value;
            }
        }

        private bool IsVCalculableFromXYZ
            => X is not null && Y is not null && Z is not null;

        public void SetDimensions(Length? x, Length? y, Length? z)
        {
            if (x is not null && y is not null && z is not null)
            {
                V = null;
            }

            X = x;
            Y = y;
            Z = z;
        }

        public void SetDimensions(Length? x, Length? y, Volume? v)
        {
            if (x is not null && y is not null && v is not null)
            {
                Z = null;
            }

            V = v;
            X = x;
            Y = y;
        }

        public void SetDimensions(Length? x, Volume? v, Length? z)
        {
            if (x is not null && v is not null && z is not null)
            {
                Y = null;
            }

            V = v;
            X = x;
            Z = z;
        }

        public void SetDimensions(Volume? v, Length? y, Length? z)
        {
            if (v is not null && y is not null && z is not null)
            {
                X = null;
            }

            V = v;
            Y = y;
            Z = z;
        }
    }
}
