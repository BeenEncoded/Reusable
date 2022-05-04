using System;
using System.IO.Compression;
using System.Linq;

namespace reusable.compression
{
    /// <summary>
    /// using the command design pattern to encapsulate different actions we will perform on the archive.
    /// This will allow us to dynamically change the behavior of the algorithm based on what is needed, and
    /// abstract many of the common functions we perform on the ziparchive without needing to 
    /// worry about the ZipArchive's lifetime.
    /// </summary>
    public abstract class ArchiveAction
    {
        protected ArchiveAction()
        {
        }

        ~ArchiveAction()
        {
        }

        public abstract bool execute(in ZipArchive archive);
    }

    public sealed class AddFileIf : ArchiveAction
    {
        public delegate bool add_predicate_t(in string canonical_pathname, in ZipArchiveEntry entry);

        private add_predicate_t predicate;
        private string filename, zip_location;

        /// <summary>
        /// Creates an AddFileIf action that will add a file to a zip archive only
        /// if the defined predicate returns true given a path on the filesystem and
        /// a zipArchiveEntry.
        /// </summary>
        /// <param name="canonical_pathname">The full canonical pathname
        /// of the file on the filesystem.</param>
        /// <param name="zip_location">Location in the zip archive the
        /// file should be?  or whatever you want depending on what you're doing.</param>
        /// <param name="predicate">The predicate that returns true upon a pre-specified condition.</param>
        public AddFileIf(
            in string canonical_pathname, 
            in string zip_location,
            in add_predicate_t predicate) : base()
        {
            add_predicate_t do_nothing = (in string s, in ZipArchiveEntry z) => true;
            this.predicate = predicate ?? do_nothing;
            this.zip_location = zip_location ?? string.Empty;
            filename = canonical_pathname ?? string.Empty;
        }

        ~AddFileIf()
        {
        }

        /// <summary>
        /// Executes this action upon a zip archive.
        /// </summary>
        /// <param name="archive">The archive to execute this action upon.</param>
        /// <returns>True if the action was performed successfully without errors.</returns>
        public override bool execute(in ZipArchive archive)
        {
            if(predicate(in filename, archive.GetEntry(zip_location)))
            {
                // TODO: do this
            }
            throw new NotImplementedException("Not Implimented!");
        }

        
    }

    /// <summary>
    /// A collection of functions designed to operate within a zip archive
    /// that help us test for certain conditions.
    /// </summary>
    public static class ZTest
    {
        public static bool exists(in ZipArchive arch, in string zip_loc)
        {
            string loc = zip_loc ?? string.Empty;
            return arch.Entries.Any(entry => entry.FullName == loc);
        }

        public static bool is_file(in ZipArchive arch, in string zip_loc)
        {
            return false; //TODO: write the is_file for inside zips
            if(exists(arch, zip_loc))
            {
                
            }
        }


    }
}