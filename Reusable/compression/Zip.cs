using System.IO.Compression;
using System;
using System.IO;

using reusable.fs;

namespace reusable.compression
{
    public class Zip : IArchive
    {
        private string _zip_filename;

        protected string zip_filename
        {
            get
            {
                return _zip_filename ?? string.Empty;
            }
        }

        public Zip(in string pathname)
        {
            if(!FTest.is_file(pathname))
                throw new ArgumentException($"{typeof(Zip).FullName}.Zip(in string pathname): Argument is not a file!");
            
            _zip_filename = Path.GetFullPath(pathname);
        }

        public bool add_item(in string canonical_pathname)
        {
            throw new System.NotImplementedException();
        }

        public bool add_item_if(in string canonical_pathname, in IArchive.add_predicate_t predicate)
        {
            throw new System.NotImplementedException();
        }
    }
}