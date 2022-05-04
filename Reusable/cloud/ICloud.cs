using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reusable.cloud
{
    /**
     * <summary>This class represents a directory entry within cloud storage.  It is extended
     * by subclasses for each provider (OneDrive, NextCloud, Etc. Etc.).</summary>
     */
    public class DirectoryEntry
    {
        protected string _path = string.Empty;

        public string path{get => _path ?? string.Empty; set => _path = value; }

        public DirectoryEntry()
        {
        }

    }

    /** <summary>Standard interface for all cloud storage APIs.</summary>
     */
    public interface ICloud
    {
        Task connect();
        void disconnect();
        IEnumerable<DirectoryEntry> iterate(string root, bool recurse);
        bool write(in BinaryReader reader, in string dest_subpath);
    }
}
