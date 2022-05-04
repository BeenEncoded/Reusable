using System.IO;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

using reusable.utility;

namespace reusable.data
{
    /// <summary>
    /// This data structure is responsible for allowing the association of IDs
    /// to strings.  This can be useful for a number of reasons.
    /// Its primary functions include IO suppport and automatic ID management.
    /// </summary>
    public struct id_mapping<T> : RWObject
    {
        private Dictionary<T, uint> _map;

        #region SettersGetters
        public string filename => throw new NotImplementedException();

        private Dictionary<T, uint> map
        {
            get
            {
                if(_map == null) _map = new Dictionary<T, uint>();
                return _map;
            }
            set
            {
                _map = value;
            }
        }
        #endregion

        #region comparison operators
        public static bool operator==(id_mapping<T> lho, id_mapping<T> rho)
        {
            return lho.map.SequenceEqual(rho.map);
        }

        public static bool operator!=(id_mapping<T> lho, id_mapping<T> rho)
        {
            return !(lho == rho);
        }

        public override bool Equals(object o)
        {
            if(o is id_mapping<T> other)
            {
                return other == this;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("[");
            foreach(T key in map.Keys)
            {
                s.Append("<");
                s.Append(key);
                s.Append(", ");
                s.Append(map[key]);
                s.Append(">");
            }
            s.Append("]");
            return s.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        /// <summary>
        /// Gets the id associated with the key provided.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="id">The id.</param>
        /// <returns>true if the key existed within the dictionary.</returns>
        public bool getId(in T key, out uint id)
        {
            id = 0;
            if(map.ContainsKey(key))
            {
                id = map[key];
                return true;
            }
            return false;
        }

        public bool getKey(in uint id, out T key)
        {
            key = default;
            if(map.ContainsValue(id))
            {
                foreach(var x in map.Keys)
                {
                    if(map[x] == id)
                    {
                        key = x;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool contains(in uint id)
        {
            return map.ContainsValue(id);
        }

        public bool contains(in T key)
        {
            return map.ContainsKey(key);
        }

        /// <summary>
        /// Adds the key to the dictionary.  A new id is generated and 
        /// assigned to it.  This id is garunteed only to be unique to this
        /// instance of id_mapping.  If the key already exists in the map, then
        /// nothing happens.
        /// </summary>
        /// <param name="key"></param>
        public void add(in T key)
        {
            if(key != null)
            {
                try
                {
                    map.Add(key, newid());
                }
                catch(ArgumentException)
                {
                }
            }
        }

        /// <summary>
        /// This updates the map to the list.  The idea is that we only remove elements
        /// that don't exist in the list that is passed as the argument.  Recreating the
        /// map may result in new ids for the same element, which may be problematic
        /// if you're using a persistent mapping that has to stay the same for each element
        /// that exists past the lifetime of the program.  To that end, this function will add new
        /// elements while removing deleted ones, and it also will NOT touch the ones that
        /// have stayed the same.
        /// </summary>
        /// <param name="l">The list to update this map to.</param>
        public void update(in List<T> l)
        {
            //this should work.
            foreach(T t in l) add(t);

            Dictionary<T, uint>.KeyCollection keys = new Dictionary<T, uint>.KeyCollection(map);
            foreach(T k in keys)
            {
                if(!l.Contains(k))
                {
                    map.Remove(k);
                }
            }
        }

        /// <summary>
        /// Removed the specified key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if and only if the key was removed.</returns>
        public bool remove(in T key)
        {
            return map.Remove(key);
        }

        /// <summary>
        /// Removes all keys with the matching ID.  Every Key
        /// should have a unique ID, so there should never be more than one with
        /// the specified ID, but we find and remove ALL keys with the matching ID
        /// just in case.
        /// </summary>
        /// <param name="id">The ID that will will remove.</param>
        /// <returns>True if and only if an item was removed.</returns>
        public bool remove(in uint id)
        {
            bool success = false;
            if(map.ContainsValue(id))
            {
                List<T> keys = new List<T>();

                //find all the keys with a matching id
                foreach(var k in map.Keys)
                {
                    if(map[k] == id)
                    {
                        keys.Add(k);
                    }
                }
                success = keys.Count > 0;

                //remove those keys -- tracking success.
                foreach(var k in keys) success &= map.Remove(k);
            }
            return success;
        }

        public bool read(BinaryReader reader, Encoding encoding = null)
        {
            if(encoding == null)
            {
                encoding = constant.defaultEncoding;
            }

            map.Clear();
            bool overall_success = true;
            if(overall_success &= io.read(out int count, reader, encoding))
            {
                uint newval = 0;
                for(int x = 0; x < count; ++x)
                {
                    if(overall_success &= io.read(out T temp, reader, encoding))
                    {
                        if(overall_success &= io.read(out newval, reader, encoding))
                        {
                            map.Add(temp, newval);
                        }
                    }
                }
            }
            return overall_success;
        }

        public void write(BinaryWriter writer, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = constant.defaultEncoding;
            }

            io.write(map.Count, writer, encoding);
            foreach (var element in map)
            {
                io.write(element.Key, writer, encoding);
                io.write(element.Value, writer, encoding);
            }
        }
        
        /// <summary>
        /// Given the existing ids, generates a unique one.
        /// </summary>
        /// <returns></returns>
        private uint newid()
        {
            uint id = unchecked((uint)-1); //in this case overflow is desired.
            while (map.ContainsValue(++id));
            return id;
        }

    }
}