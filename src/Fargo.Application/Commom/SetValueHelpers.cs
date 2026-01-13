namespace Fargo.Application.Commom
{
    public static class SetValueHelpers
    {
        public static void SetIfNotNull<T>(ref T field, T? value)
            where T : struct
        {
            if (value == null)
            {
                return;
            }

            field = value;
        }
    }
}
