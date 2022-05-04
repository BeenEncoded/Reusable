

namespace reusable.cloud
{
    /// <summary>
    /// Determines the method of copy to the destination.
    /// </summary>
    public enum WriteMethod : byte
    {
        /// <summary>
        /// Overwrite the destination, writing directly 
        /// over the existing data.
        /// </summary>
        overwrite = 0,

        /// <summary>
        /// Write a new copy, and erase the old one.
        /// </summary>
        copy_and_erase,

        /// <summary>
        /// write a new copy, and don't erase anything.
        /// </summary>
        copy_no_erase
    }

    /// <summary>
    /// Used to mark which type of compression should be used.
    /// </summary>
    public enum CompressionType : byte
    {
        /// <summary>
        /// no compression type will be used.  Totally uncompressed.
        /// </summary>
        no_compression = 0
    }

    public interface ICloudSettings
    {
        #region authentication
        public string password{get;}
        
        public string username{get;}

        #endregion authentication

        #region communication
        public string server{get;}
        #endregion communication

        #region behavior
        public WriteMethod write_method {get;}
        public bool use_rsync{get;}
        public CompressionType compression_type{get;}
        public string root_dir{get;}
        #endregion behavior

    }

    /// <summary>
    /// Generic cloud settings that should apply to every service.
    /// </summary>
    public class CloudSettings : ICloudSettings
    {
        protected string _password, _username, _root_dir, _server;
        protected WriteMethod _write_method;
        protected bool _use_rsync;
        protected CompressionType _compression_type;

        public CloudSettings()
        {
        }

        ~CloudSettings()
        {
        }

        /// <summary>
        /// sets the login settings.
        /// </summary>
        /// <param name="uname">The username to use.</param>
        /// <param name="passwd">the password to use.</param>
        /// <returns>CloudSettings</returns>
        public CloudSettings setLogin(in string uname, in string passwd)
        {
            password = passwd;
            username = uname;
            return this;
        }

        public CloudSettings setServer(in string uri)
        {
            this.server = uri;
            return this;
        }

        #region authentication
        public string password
        {
            get
            {
                return _password ?? string.Empty;
            }
            set
            {
                _password = value ?? string.Empty;
            }
        }
        
        public string username
        {
            get
            {
                return _username ?? string.Empty;
            }
            set
            {
                _username = value ?? string.Empty;
            }
        }

        #endregion authentication

        #region communication
        public string server
        {
            set
            {
                this._server = value ?? string.Empty;
            }
            get
            {
                return this._server ?? string.Empty;
            }
        }

        #endregion communication

        #region behavior
        public WriteMethod write_method => _write_method;
        public bool use_rsync => _use_rsync;
        public CompressionType compression_type => _compression_type;

        public string root_dir
        {
            set
            {
                _root_dir = value ?? string.Empty;
            }
            get
            {
                return _root_dir ?? string.Empty;
            }
        }
        #endregion behavior
    }


}