using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace Wave.Common
{
    public static class IsolatedStorageHelper
    {
        public static bool FileExists(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
                return false;

            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                return iso.FileExists(filePath);
        }

        public static void DeleteFile(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
                return;

            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                iso.DeleteFile(filePath);
        }

        public static byte[] ReadFile(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
                return null;

            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            using (IsolatedStorageFileStream fs = iso.OpenFile(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] res = new byte[fs.Length];

                if (fs.Read(res, 0, (int)fs.Length) == fs.Length)
                    return res;
            }

            return null;
        }

        public static void WriteFile(string filePath, byte[] data)
        {
            if (String.IsNullOrWhiteSpace(filePath) || (data == null))
                return;

            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.FileExists(filePath))
                    iso.DeleteFile(filePath);

                using (IsolatedStorageFileStream fs = iso.CreateFile(filePath))
                    fs.WriteBytes(data);
            }
        }
    }
}
