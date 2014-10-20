using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Spira.Core
{
    [Flags]
    public enum TypeCacheFlags : byte
    {
        None = 0,
        ReadableFromStream = 2
    }

    public static class TypeCache<T>
    {
        public static readonly Type Type = typeof(T);
        public static readonly int UnsafeSize = GetSize();
        public static readonly TypeCacheFlags Flags = GetFlags();

        private static int GetSize()
        {
            DynamicMethod dynamicMethod = new DynamicMethod("SizeOf", typeof(int), Type.EmptyTypes);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Sizeof, Type);
            generator.Emit(OpCodes.Ret);

            return ((Func<int>)dynamicMethod.CreateDelegate(typeof(Func<int>)))();
        }

        private static TypeCacheFlags GetFlags()
        {
            TypeCacheFlags result = TypeCacheFlags.None;
            
            if (TypeCache<IReadableFromStream>.Type.IsAssignableFrom(Type))
                result |= TypeCacheFlags.ReadableFromStream;

            return result;
        }
    }
}