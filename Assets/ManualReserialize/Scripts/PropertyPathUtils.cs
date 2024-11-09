#if UNITY_EDITOR

namespace ManualReserialization
{
    public static class PropertyPathUtils
    {
        public static bool DoesPropertyModificationMatchAnyPath(string propertyPath, string initialPath, string[] reserializePaths)
        {
            var hasInitialPath = initialPath.Length > 0 ? 1 : 0;
            foreach (var reserializePath in reserializePaths)
            {
                if (DoesPropertyModificationMatchPath(propertyPath, initialPath, reserializePath, hasInitialPath))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DoesPropertyModificationMatchPath(string propertyPath, string initialPath, string reserializePath, int hasInitialPath)
        {
            if (string.CompareOrdinal(propertyPath, 0, initialPath, 0, initialPath.Length) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(propertyPath, initialPath.Length + hasInitialPath, reserializePath, 0, reserializePath.Length) != 0)
            {
                return false;
            }

            return true;
        }

    }
}
#endif
