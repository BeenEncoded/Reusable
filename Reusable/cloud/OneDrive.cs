/**
*  OneDrive support has been dropped.  I am unable to finance an Azure instance for
*  their identity service.  Anyone who would like to do so is welcome, but will have to bear the cost.
*  
*  
*  Sincerely, 
*            Jonathan Whitlock
*            aka. BeenEncoded
*/

// using System.Security.Cryptography;
// using System.IO;
// using System.Net;
// using System.Threading.Tasks;
// using System;
// using System.Text;
// using System.Text.Json;
// using System.Diagnostics;
// using System.Linq;
// using System.Net.Http;
// using System.Collections.Generic;

// using reusable.utility;
// using System.Runtime.Serialization;

// namespace reusable.cloud
// {
//     public class OneDriveDirectoryEntry : DirectoryEntry
//     {
//         private bool _isdirectory = false;
//         private string _id;

//         public bool isdirectory { get => _isdirectory; set => _isdirectory = value; }
//         public string id { get => _id; set => _id = value; }

//         public OneDriveDirectoryEntry()
//         {
//         }

//         public OneDriveDirectoryEntry(in JToken json)
//         {
//             from_json(json);
//         }

//         public void from_json(in JToken json)
//         {
//             Debug.WriteLine(json);
//             _path = json["name"].ToObject<string>();
//             _id = json["id"].ToObject<string>();
//             _isdirectory = json.SelectToken("folder") != null;
//         }

//         public override string ToString()
//         {
//             return $"Is Directory: {_isdirectory}{Environment.NewLine}ID = \"{_id}\"{Environment.NewLine}Path = \"{_path}\"";
//         }
//     }

//     internal struct OneDriveData
//     {
//         public string username,
//             userid,
//             driveid;
//         public bool initialized;

//         public OneDriveData(in string json)
//         {
//             var root = JObject.Parse(json);
//             username = root["owner"]["user"]["displayName"].ToObject<string>();
//             userid = root["owner"]["user"]["id"].ToObject<string>();
//             driveid = root["id"].ToObject<string>();
//             initialized = true;
//         }
//     }

//     public class OneDrive : ICloud
//     {
//         //private static readonly string TENANT_ID = "6d55d3e9-bb38-4a9a-8f81-3e5bcd3d0c07";
//         //private static readonly string SECRET = @"Ew~jsexiAGW4PW4.g.3zpW75.uz.NDgRGl";
//         //private static readonly string REDIRECT_URL = @"https://login.microsoftonline.com/common/oauth2/nativeclient";
//         private static readonly string[] SCOPES = { @"https://graph.microsoft.com/.default" };
//         private static readonly string AUTHORITY = @"https://login.microsoftonline.com/consumers/",
//             CLIENT_ID = "240e5a67-a425-4e37-9b5e-b4ee60e2ea7d",
//             GRAPH_ENDPOINT = @"https://graph.microsoft.com/v1.0";

//         private OneDriveRestApi api = null;
//         private AuthenticationResult token = null;
//         private bool authenticated = false;
//         private OneDriveData driveinfo;
//         private string root = string.Empty;

//         /** <summary>Instantiates a OneDrive object using the specified root.
//          * In this case the root is NOT enforced by the graph API but by this object itself.
//          * This allows for more flexibility and for the possibility of changing the root without
//          * making an API call.</summary>
//          * <param name="root">The path within OneDrive that will be operated within.</param>
//          */
//         public OneDrive(in string root)
//         {
//             this.root = root;
//             api = new OneDriveRestApi(this);
//         }

//         ~OneDrive()
//         {
//             disconnect();
//         }

//         public async Task connect()
//         {
//             api.get_token();
//             common.wait_until(() => authenticated, 10000);
//             await testiteration();
//         }

//         public void disconnect()
//         {
//         }

//         public IEnumerable<DirectoryEntry> iterate(string root, bool recurse = false)
//         {
//             common.wait_until(() => authenticated, 10000);
//             if (!authenticated)
//             {
//                 throw new OneDriveAuthorizationException("Failed to authenticate with OneDrive!");
//             }
//             var task = api.get_directories(root);
//             task.Wait();
//             var entries = task.Result;
//             foreach (var entry in entries)
//             {
//                 yield return entry;
//                 if (entry.isdirectory && recurse)
//                 {
//                     foreach (var element in iterate(entry.path, true))
//                     {
//                         yield return element;
//                     }
//                 }
//             }
//         }

//         private async Task testiteration()
//         {
//             foreach (var entry in iterate(string.Empty, true))
//             {
//                 Debug.WriteLine(entry);
//             }
//         }

//         private class OneDriveRestApi
//         {
//             private OneDrive instance = null;
//             private HttpClient client = new HttpClient();

//             public OneDriveRestApi(in OneDrive d)
//             {
//                 instance = d;
//             }

//             /** <summary>sends a request to the url with the <c>HttpClient</c> using
//              * the authentication token held by the instance of <c>OneDrive</c>.</summary>
//              * <returns>The json response.</returns>
//              */
//             private async Task<string> make_request(string url, HttpMethod method)
//             {
//                 HttpResponseMessage response;
//                 try
//                 {
//                     var request = new HttpRequestMessage(method, url);
//                     //Add the token in Authorization header
//                     request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", instance.token.AccessToken);
//                     response = await client.SendAsync(request);
//                     return await response.Content.ReadAsStringAsync();
//                 }
//                 catch (Exception ex)
//                 {
//                     return ex.ToString();
//                 }
//             }

//             /**
//              * <summary>Sends a GET request to the url using the onedrive auth token.</summary>
//              * <returns>The json response.</returns>
//              */
//             private async Task<string> get_request(string url)
//             {
//                 return await make_request(url, HttpMethod.Get);
//             }

//             /** <summary>This procedure acquires a token from Microsoft by allowing the
//              * user to authenticate.  This must be done at least once before making
//              * any API calls to the microsoft Graph API.</summary>
//              */
//             public async void get_token()
//             {
//             }

//             /** <summary>Acquires the user's one drive information.  In particular this gets the
//              * drive ID that is necessary to make Graph calls.  Called before any one drive API
//              * calls.</summary>
//              */
//             public async void get_drive_info()
//             {
//                 instance.driveinfo = new OneDriveData(await get_request($"{GRAPH_ENDPOINT}/drive"));
//                 Debug.WriteLine($"Drive Info Aqcuired: {instance.driveinfo.username}, id={instance.driveinfo.userid}, driveid={instance.driveinfo.driveid}");
//             }

//             /**
//              * <summary>Gets a list of directories under the specified root.</summary>
//              * <param name="root_directory">The directory under which to search.  Directory entries should 
//              * be named using non-canonical naming, but using complete paths.  Passing string.Empty will
//              * operate on the top-most directory.</param>
//              * <returns>The list of entries.</returns>
//              * <example>List entries = await get_directories("Documents/stuff/mycoolstuff");</example>
//              */
//             public async Task<List<OneDriveDirectoryEntry>> get_directories(string root_directory)
//             {
//                 common.wait_until(() => instance.driveinfo.initialized, 10000); // just wait for it to initialize plz...
//                 if (!instance.driveinfo.initialized)
//                 {
//                     throw new Exception("Drive info is not initialized...");
//                 }

//                 string QUERY = $"{GRAPH_ENDPOINT}/drives/{instance.driveinfo.driveid}";
//                 List<OneDriveDirectoryEntry> stuff = new List<OneDriveDirectoryEntry>();

//                 //build the query URL:
//                 StringBuilder sbuilder = new StringBuilder();
//                 sbuilder.Append($"{QUERY}/root:");
//                 if (instance.root.Length > 0)
//                 {
//                     sbuilder.Append($"/{instance.root}");
//                 }
//                 if (root_directory.Length > 0)
//                 {
//                     if (!root_directory.StartsWith(@"/"))
//                     {
//                         sbuilder.Append(@"/");
//                     }
//                     else
//                     {
//                         sbuilder.Append($"{root_directory}");
//                     }
//                 }
//                 sbuilder.Append(":");
//                 sbuilder.Append("/children");

//                 string jsonstuff = await get_request(sbuilder.ToString());
//                 Debug.WriteLine(jsonstuff);
//                 JsonDocument.
//                 var root = JObject.Parse(jsonstuff);
//                 if (root.SelectToken("error") != null)
//                 {
//                     throw new OneDriveMalformedRequestException(
//                         $"{root["code"]}{Environment.NewLine}{root["message"]}");
//                 }
//                 for (int x = 0; x < root["value"].Count(); ++x)
//                 {
//                     stuff.Add(new OneDriveDirectoryEntry(root["value"][x]));

//                     sbuilder.Clear();
//                     if (root_directory.Length > 0)
//                     {
//                         sbuilder.Append($"/{root_directory}");
//                     }
//                     sbuilder.Append($"/{stuff.Last().path}");

//                     stuff.Last().path = sbuilder.ToString();
//                     Debug.WriteLine($"I got a {(stuff.Last().isdirectory ? "folder" : "file")} " +
//                         $"\"{stuff.Last().path}\" and its id is {stuff.Last().id}!");
//                 }
//                 return stuff;
//             }


//         }
//     }

//     internal class OneDriveMalformedRequestException : Exception
//     {
//         public OneDriveMalformedRequestException()
//         {
//         }

//         public OneDriveMalformedRequestException(string message) : base(message)
//         {
//         }

//         public OneDriveMalformedRequestException(string message, Exception innerException) : base(message, innerException)
//         {
//         }

//         protected OneDriveMalformedRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
//         {
//         }
//     }

//     internal class OneDriveAuthorizationException : Exception
//     {
//         public OneDriveAuthorizationException()
//         {

//         }

//         public OneDriveAuthorizationException(string message) : base(message)
//         {
//         }

//         public OneDriveAuthorizationException(string message, Exception innerException) : base(message, innerException)
//         {
//         }

//         protected OneDriveAuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context)
//         {
//         }
//     }


// }
