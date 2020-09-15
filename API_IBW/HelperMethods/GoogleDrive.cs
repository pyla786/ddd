//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Drive.v3;
//using Google.Apis.Services;
//using Google.Apis.Util.Store;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography.X509Certificates;
//using System.Threading;
//using System.Web;

//namespace API_IBW.HelperMethods
//{
//    public class GoogleDrive
//    {
//        public void Authorize(string fileFullPath)
//        {
//            //string[] scopes = new string[] { DriveService.Scope.Drive,
//            //                   DriveService.Scope.DriveFile,};
//            //var clientId = "143376680112-rs3mrhvrnjmf9ri32vcal9qchp5g7cot.apps.googleusercontent.com";      // From https://console.developers.google.com  
//            //var clientSecret = "pXR6LgzNbUmj77eVD60hTJ8Y";          // From https://console.developers.google.com  
//            //                                                             // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%  
//            //var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
//            //{
//            //    ClientId = clientId,
//            //    ClientSecret = clientSecret
//            //}, scopes,
//            //Environment.UserName, CancellationToken.None, new FileDataStore("MyAppsToken")).Result;



//            //Once consent is recieved, your token will be stored locally on the AppData directory, so that next time you wont be prompted for consent.   
//            string[] scopes = new string[] { DriveService.Scope.Drive }; // Full access

//            var keyFilePath = @"C:\Users\Anji\Downloads\Quickstart-7181e72973ec.p12";    // Downloaded from https://console.developers.google.com
//            var serviceAccountEmail = "fileuploadservice@quickstart-1572689873564.iam.gserviceaccount.com";  // found https://console.developers.google.com

//            //loading the Key file
//            var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);
//            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
//            {
//                Scopes = scopes
//            }.FromCertificate(certificate));
//            DriveService service = new DriveService(new BaseClientService.Initializer()
//            {
//                HttpClientInitializer = credential,
//                ApplicationName = "Quickstart",

//            });
//            service.HttpClient.Timeout = TimeSpan.FromMinutes(100);
//            //Long Operations like file uploads might timeout. 100 is just precautionary value, can be set to any reasonable value depending on what you use your service for  

//            //team drive root https://drive.google.com/drive/folders/0AJB8b7Dtu-LxUk9PVA   

//            var respocne = uploadFile(service, fileFullPath, "");
//            //InsertPermission(Business_Classes.GoogleDriveFilesRepository.GetService_v2(), "16UpN3w2KGZBYT1YUJB0TsCeoYqNLUqoW", "azimuth@ibwsurveyors.com", "anyone", "owner");
//            // Third parameter is empty it means it would upload to root directory, if you want to upload under a folder, pass folder's id here.
//            //MessageBox.Show("Process completed--- Response--" + respocne);
//        }
//        public Google.Apis.Drive.v3.Data.File uploadFile(DriveService _service, string _uploadFile, string _parent, string _descrp = "Uploaded with .NET!")
//        {
            
//            var Files = _service.Files.Get(_parent);
//            var Drives = _service.Drives;
//            if (System.IO.File.Exists(_uploadFile))
//            {
//                Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
//                body.Name = System.IO.Path.GetFileName(_uploadFile);
//                body.Description = _descrp;
//                body.MimeType = GetMimeType(_uploadFile);
//                body.Parents = new List<string>
//                    {
//                        "191KA_PI_-HiXtb8BjVW3B0v9plwKTDAd"
//                    };
//                //body.Parents = new List<string> { _parent };// UN comment if you want to upload to a folder(ID of parent folder need to be send as paramter in above method)
//                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
//                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
//                try
//                {
//                    FilesResource.CreateMediaUpload request = _service.Files.Create(body, stream, GetMimeType(_uploadFile));
//                    request.SupportsTeamDrives = true;
//                    // You can bind event handler with progress changed event and response recieved(completed event)
//                    //request.ProgressChanged += Request_ProgressChanged;
//                    //request.ResponseReceived += Request_ResponseReceived;
//                    request.Upload();

//                    //Google.Apis.Drive.v3.Data.File file = request.ResponseBody;
//                    //Google.Apis.Drive.v2.Data.Permission newPermission = new Google.Apis.Drive.v2.Data.Permission();
//                    //newPermission.Value = "azimuth@ibwsurveyors.com";
//                    //newPermission.Type = "user";
//                    //newPermission.Role = "writer";

//                    //Google.Apis.Drive.v2.PermissionsResource.InsertRequest insertRequest = Business_Classes.GoogleDriveFilesRepository.GetService_v2().Permissions.Insert(newPermission, file.Id);
//                    //insertRequest.SendNotificationEmails = false;
//                    //insertRequest.Execute();
//                    return request.ResponseBody;
//                }
//                catch (Exception e)
//                {
//                    //MessageBox.Show(e.Message, "Error Occured");
//                    return null;
//                }
//            }
//            else
//            {
//                //MessageBox.Show("The file does not exist.", "404");
//                return null;
//            }
//        }
//        public static Google.Apis.Drive.v2.Data.Permission InsertPermission(Google.Apis.Drive.v2.DriveService service, String fileId, String who, String type, String role)
//        {
//            Google.Apis.Drive.v2.Data.Permission newPermission = new Google.Apis.Drive.v2.Data.Permission();
//            newPermission.EmailAddress = who;
//            newPermission.Value = who;
//            newPermission.Type = type;
//            newPermission.Role = role;
//            newPermission.Domain = "";
//            try
//            {
//                return service.Permissions.Insert(newPermission, fileId).Execute();
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("An error occurred: " + e.Message);
//            }
//            return null;
//        }
//        //public static Google.Apis.Drive.v2.Data.Permission InsertPermission(Google.Apis.Drive.v2.DriveService service, String fileId, String who, String type, String role)
//        //{
//        //    Google.Apis.Drive.v2.Data.Permission newPermission = new Google.Apis.Drive.v2.Data.Permission();
//        //    newPermission.Value = "azimuth@ibwsurveyors.com";
//        //    newPermission.Type = "user";
//        //    newPermission.Role = "writer";



//        //    //Google.Apis.Drive.v2.Data.Permission newPermission = new Google.Apis.Drive.v2.Data.Permission();
//        //    //newPermission.Value = who;
//        //    //newPermission.Type = type;
//        //    //newPermission.Role = role;
//        //    try
//        //    {
//        //        Google.Apis.Drive.v2.PermissionsResource.InsertRequest insertRequest = service.Permissions.Insert(newPermission, fileId);
//        //        insertRequest.SendNotificationEmails = false;
//        //        insertRequest.Execute();
//        //        return newPermission;
//        //    }
//        //    catch (Exception e)
//        //    {
//        //        Console.WriteLine("An error occurred: " + e.Message);
//        //    }
//        //    return null;
//        //}
//        private static string GetMimeType(string fileName)
//        {
//            string mimeType = "application/unknown";
//            string ext = System.IO.Path.GetExtension(fileName).ToLower();
//            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
//            if (regKey != null && regKey.GetValue("Content Type") != null)
//                mimeType = regKey.GetValue("Content Type").ToString();
//            return mimeType;
//        }
//    }
//}