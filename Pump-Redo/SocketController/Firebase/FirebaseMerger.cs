using System.Linq;

namespace Pump.FirebaseDatabase
{
    internal static class FirebaseMerger
    {
        public static void CopyValues<T>(T target, T source)
        {
            var t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null)
                    prop.SetValue(target, value, null);
            }
        }
    }
}