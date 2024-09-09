using Google.Protobuf.Reflection;

namespace AElf
{
    public static class FileDescriptorExtensions
    {
        public static string GetIdentity(this FileDescriptor descriptor)
        {
            // TODO: CustomOptions are obsolete, use GetOptions() instead.
            if (descriptor.CustomOptions.TryGetString(500001, out var id)) return id;

            return "";
        }
    }
}