using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Crutches.IO
{
    public static class FileSystemUtils
    {
        /// <summary>
        /// Check if file is ready to be read
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsFileLocked(this FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            }
            catch (IOException ex)
            {
                return true; // file doesn't exist or still being written to.
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;//file is not locked
        }

        public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo dir, Regex pattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (dir == null) throw new ArgumentNullException("dir");
            if (pattern == null) throw new ArgumentNullException("pattern");
            return dir.EnumerateFiles("*", searchOption).Where(x => pattern.IsMatch(x.Name));
        }

    }
}
