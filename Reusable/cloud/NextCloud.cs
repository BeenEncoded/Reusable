using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;

namespace reusable.cloud
{

    public class NextCloud : ICloud
    {
        private static NextcloudApi.Api _api;
        private NextCloudSettings _settings;

        #region settersgetters
        protected NextCloudSettings settings
        {
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("reusable.cloud.NextCloud.settings : unable to set to null.");
                }
                this._settings = value;
            }
            get
            {
                return this._settings;
            }
        }

        #endregion settersgetters

        /// <summary>
        /// Creates an instance of the NextCloud object to make calls from.
        /// </summary>
        /// <param name="uname">The username to use.</param>
        /// <param name="passwd">The password to use.</param>
        /// <param name="program_directory">The programs current operating directory.  This is used to load the nextcloud configuration.</param>
        /// <returns>A new instance of the NextCloud object appropriatly instantiated with the arguments passed.</returns>
        public NextCloud(in ICloudSettings settings) : base()
        {
            this.settings = new NextCloudSettings().setGenericSettings(settings);
            if(_api == null)
            {
                _api = new NextcloudApi.Api((NextcloudApi.ISettings)this.settings);
            }
            else
            {
                _api.Settings = this.settings;
            }
        }

        ~NextCloud()
        {
            _api.Dispose();
            Debug.Print("destroying nextcloud instance");
        }

        public async Task connect()
        {
            Trace.TraceInformation("Connecting to the nextcloud service...");
            await _api.LoginAsync();
        }

        public void disconnect()
        {
        }

        public IEnumerable<DirectoryEntry> iterate(string root, bool recurse)
        {
            throw new NotImplementedException();
        }

        public bool write(in BinaryReader reader, in string dest_subpath)
        {
            throw new NotImplementedException();
        }

        protected class NextCloudSettings : NextcloudApi.Settings
        {
            public ICloudSettings generic_settings = null;

            public NextCloudSettings setGenericSettings(in ICloudSettings settings)
            {
                this.generic_settings = settings;
                this.Password = generic_settings.password;
                this.Username = generic_settings.username;
                this.ServerUri = new Uri(settings.server);
                this.ApplicationName = "Application";
                this.ClientId = "1234567890";

                //set these or you get an exception when you create the NextcloudApi.Api
                this.ApplicationName = "nextcloud";
                this.RedirectUri = new Uri("https://192.168.1.200/nextcloud/");

                return this;
            }
        }


    }

    namespace nextcloud_actions
    {
        


    }

}