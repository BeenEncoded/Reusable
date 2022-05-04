
namespace reusable.compression
{
    public interface IArchive
    {
        public delegate bool add_predicate_t(in string canonical_pathname);

        /// <summary>
        /// Adds an item from the filesystem to the archive.
        /// </summary>
        /// <param name="canonical_pathname">The fully qualified pathname to the filesystem entry.</param>
        /// <returns>True if the item was successfully added to the archive and no errors occured.</returns>
        public bool add_item(in string canonical_pathname);

        /// <summary>
        /// Adds an item from the filesystem only if the predicate returns true after operating on it.
        /// </summary>
        /// <param name="canonical_pathname">The fully qualified pathname to the filesystem entry.</param>
        /// <param name="predicate">A predicate that returns true only if the path should be copied.</param>
        /// <returns>True only if no errors occured.</returns>
        public bool add_item_if(in string canonical_pathname, in add_predicate_t predicate);

    }
}