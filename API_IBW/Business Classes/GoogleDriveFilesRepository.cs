using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Web;
using API_IBW.Models;
using System.Security.Cryptography.X509Certificates;

namespace API_IBW.Business_Classes
{
    public class GoogleDriveFilesRepository
    {
        public static string[] Scopes = { Google.Apis.Drive.v3.DriveService.Scope.Drive };
        public static Google.Apis.Drive.v3.DriveService GetService_v3()
        {
            string path = HttpContext.Current.Server.MapPath("~/App_Data/");
            UserCredential credential;
            using (var stream = new FileStream(path + "credentials.json", FileMode.Open, FileAccess.Read))
            {
                //String FolderPath = @"D:\";
                String FolderPath = HttpContext.Current.Server.MapPath("~");
                String FilePath = Path.Combine(FolderPath, "DriveServiceCredentials.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FilePath, true)).Result;
            }

            //Create Drive API service.
            Google.Apis.Drive.v3.DriveService service = new Google.Apis.Drive.v3.DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveRestAPI-v3",
            });

            //Service Account
            //string path = HttpContext.Current.Server.MapPath("~/App_Data/");
            //var keyFilePath = path + "Quickstart-7181e72973ec.p12";    // Downloaded from https://console.developers.google.com
            //var serviceAccountEmail = "fileuploadservice@quickstart-1572689873564.iam.gserviceaccount.com";  // found https://console.developers.google.com

            ////loading the Key file
            //var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            //var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
            //{
            //    Scopes = Scopes
            //}.FromCertificate(certificate));
            //Google.Apis.Drive.v3.DriveService service = new Google.Apis.Drive.v3.DriveService(new BaseClientService.Initializer()
            //{
            //    HttpClientInitializer = credential,
            //    ApplicationName = "Quickstart-3",

            //});
            //
            return service;
        }
        public static Google.Apis.Drive.v2.DriveService GetService_v2()
        {
            string path = HttpContext.Current.Server.MapPath("~/App_Data/");
            UserCredential credential;
            using (var stream = new FileStream(path + "credentials.json", FileMode.Open, FileAccess.Read))
            {
                String FolderPath = HttpContext.Current.Server.MapPath("~");
                //String FolderPath = @"D:\";
                String FilePath = Path.Combine(FolderPath, "DriveServiceCredentials.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FilePath, true)).Result;
            }

            //Create Drive API service.
            Google.Apis.Drive.v2.DriveService service = new Google.Apis.Drive.v2.DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveRestAPI-v2",
            });
            //Once consent is recieved, your token will be stored locally on the AppData directory, so that next time you wont be prompted for consent.   

            //Service Account
            //string path = HttpContext.Current.Server.MapPath("~/App_Data/");
            //var keyFilePath = path + "Quickstart-7181e72973ec.p12";     // Downloaded from https://console.developers.google.com
            //var serviceAccountEmail = "fileuploadservice@quickstart-1572689873564.iam.gserviceaccount.com";  // found https://console.developers.google.com

            ////loading the Key file
            //var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            //var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
            //{
            //    Scopes = Scopes
            //}.FromCertificate(certificate));
            //Google.Apis.Drive.v2.DriveService service = new Google.Apis.Drive.v2.DriveService(new BaseClientService.Initializer()
            //{
            //    HttpClientInitializer = credential,
            //    ApplicationName = "Quickstart",

            //});
            //
            return service;
        }

        public static List<GoogleDriveFiles> GetContainsInFolder(String folderId)
        {
            List<string> ChildList = new List<string>();
            Google.Apis.Drive.v2.DriveService ServiceV2 = GetService_v2();
            ChildrenResource.ListRequest ChildrenIDsRequest = ServiceV2.Children.List(folderId);
            do
            {
                ChildList children = ChildrenIDsRequest.Execute();

                if (children.Items != null && children.Items.Count > 0)
                {
                    foreach (var file in children.Items)
                    {
                        ChildList.Add(file.Id);
                    }
                }
                ChildrenIDsRequest.PageToken = children.NextPageToken;

            } while (!String.IsNullOrEmpty(ChildrenIDsRequest.PageToken));

            //Get All File List
            List<GoogleDriveFiles> AllFileList = GetDriveFiles();
            List<GoogleDriveFiles> Filter_FileList = new List<GoogleDriveFiles>();

            foreach (string Id in ChildList)
            {
                Filter_FileList.Add(AllFileList.Where(x => x.Id == Id).FirstOrDefault());
            }
            return Filter_FileList;
        }

        public static string CreateFolder(string FolderName, string parentId)
        {
            try
            {
                Google.Apis.Drive.v3.DriveService service = GetService_v3();

                var FileMetaData = new Google.Apis.Drive.v3.Data.File();
                FileMetaData.Name = FolderName;
                FileMetaData.MimeType = "application/vnd.google-apps.folder";
                if (parentId != null)
                {
                    FileMetaData.Parents = new List<string> { parentId };
                }
                Google.Apis.Drive.v3.FilesResource.CreateRequest request;

                request = service.Files.Create(FileMetaData);
                request.Fields = "id";
                var file = request.Execute();

                //Google.Apis.Drive.v3.Data.File file = request.ResponseBody;
                //Google.Apis.Drive.v2.Data.Permission newPermission = new Google.Apis.Drive.v2.Data.Permission();
                //newPermission.Value = "azimuth@ibwsurveyors.com";
                //newPermission.Type = "user";
                //newPermission.Role = "writer";

                //Google.Apis.Drive.v2.PermissionsResource.InsertRequest insertRequest = GetService_v2().Permissions.Insert(newPermission, file.Id);
                //insertRequest.SendNotificationEmails = false;
                //insertRequest.Execute();
                //Console.WriteLine("Folder ID: " + file.Id);
                return file.Id;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public static void FileUploadInFolder(string folderId, string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    Google.Apis.Drive.v3.DriveService service = GetService_v3();

                    //string path = Path.Combine(HttpContext.Current.Server.MapPath("~/GoogleDriveFiles"),
                    //Path.GetFileName(file.FileName));
                    //file.SaveAs(path);

                    var FileMetaData = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = Path.GetFileName(filePath),
                        MimeType = MimeMapping.GetMimeMapping(filePath),
                        Parents = new List<string>
                    {
                        folderId
                    }
                    };

                    Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;
                    using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                    {
                        request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
                        request.Fields = "id";
                        request.Upload();
                    }
                    var file1 = request.ResponseBody;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        //public static void FileUploadInFolder(string folderId, HttpPostedFileBase file)
        //{
        //    if (file != null && file.ContentLength > 0)
        //    {
        //        Google.Apis.Drive.v3.DriveService service = GetService_v3();

        //        string path = Path.Combine(HttpContext.Current.Server.MapPath("~/GoogleDriveFiles"),
        //        Path.GetFileName(file.FileName));
        //        file.SaveAs(path);

        //        var FileMetaData = new Google.Apis.Drive.v3.Data.File()
        //        {
        //            Name = Path.GetFileName(file.FileName),
        //            MimeType = MimeMapping.GetMimeMapping(path),
        //            Parents = new List<string>
        //            {
        //                folderId
        //            }
        //        };

        //        Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;
        //        using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
        //        {
        //            request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
        //            request.Fields = "id";
        //            request.Upload();
        //        }
        //        var file1 = request.ResponseBody;
        //    }
        //}
        public static List<GoogleDriveFiles> GetDriveFiles()
        {
            Google.Apis.Drive.v3.DriveService service = GetService_v3();

            // Define parameters of request.
            Google.Apis.Drive.v3.FilesResource.ListRequest FileListRequest = service.Files.List();
            FileListRequest.Fields = "nextPageToken, files(createdTime, id, name, size, version, trashed, parents)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            List<GoogleDriveFiles> FileList = new List<GoogleDriveFiles>();

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    GoogleDriveFiles File = new GoogleDriveFiles
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTime,
                        Parents = file.Parents
                    };
                    FileList.Add(File);
                }
            }
            return FileList;
        }
        public static void FileUpload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                Google.Apis.Drive.v3.DriveService service = GetService_v3();

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/GoogleDriveFiles"),
                Path.GetFileName(file.FileName));
                file.SaveAs(path);

                var FileMetaData = new Google.Apis.Drive.v3.Data.File();
                FileMetaData.Name = Path.GetFileName(file.FileName);
                FileMetaData.MimeType = MimeMapping.GetMimeMapping(path);

                Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;

                using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
                {
                    request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
                    request.Fields = "id";
                    request.Upload();
                }
            }
        }
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
        public static string GetSharableLink(string fileId)
        {
            string link = "";
            Google.Apis.Drive.v3.DriveService service = GetService_v3();
            // Define parameters of request.
            Google.Apis.Drive.v3.FilesResource.ListRequest FileListRequest = service.Files.List();
            FileListRequest.Fields = "files(id, webViewLink)";
            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            link = files.Where(x => x.Id == fileId).FirstOrDefault().WebViewLink;
            return link;
        }

        public static void DeleteFile(string fileId)
        {
            try
            {
                Google.Apis.Drive.v3.DriveService service = GetService_v3();
                Google.Apis.Drive.v3.FilesResource.DeleteRequest DeleteRequest = service.Files.Delete(fileId);
                DeleteRequest.Execute();
            }
            catch (Exception ex)
            {

            }

        }
        public static string FileUploadInFolderDirect(string folderId, HttpPostedFile file, string fileName, string mimeType)
        {
            try
            {
                Google.Apis.Drive.v3.DriveService service = GetService_v3();

                var FileMetaData = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = DateTime.Now.ToString("yyyyMMdd") + "-" + fileName + " - " + DateTime.Now.ToString("hh:mm tt"),
                    MimeType = mimeType,
                    Parents = new List<string>
                    {
                        folderId
                    }
                };


                string path = HttpContext.Current.Server.MapPath("~/QuotePurposeAttachments/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = Path.Combine(path, Path.GetFileName(file.FileName));


                file.SaveAs(filePath);

                Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;

                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                {
                    request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
                    request.Fields = "id";
                    request.Upload();
                }

                System.IO.File.Delete(filePath);


                var file1 = request.ResponseBody;
                return file1.Id;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static string FileUploadInFolder(string folderId, HttpPostedFile file, string fileName, string mimeType)
        {
            try
            {
                Google.Apis.Drive.v3.DriveService service = GetService_v3();

                var FileMetaData = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fileName,
                    MimeType = mimeType,
                    Parents = new List<string>
                    {
                        folderId
                    }
                };


                string path = HttpContext.Current.Server.MapPath("~/QuotePurposeAttachments/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = Path.Combine(path, Path.GetFileName(file.FileName));


                file.SaveAs(filePath);

                Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;

                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                {
                    request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
                    request.Fields = "id";
                    request.Upload();
                }

                System.IO.File.Delete(filePath);


                var file1 = request.ResponseBody;
                return file1.Id;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static void UpdateFolderName(string fileName, string fileId)
        {

            Google.Apis.Drive.v3.Data.File file = new Google.Apis.Drive.v3.Data.File() { Name = fileName };
            Google.Apis.Drive.v3.DriveService service = GetService_v3();
            var updateRequest = service.Files.Update(file, fileId);
            updateRequest.Fields = "name";
            file = updateRequest.Execute();
        }
        public static string MoveFiles(String fileId, String folderId)
        {
            Google.Apis.Drive.v3.DriveService service = GetService_v3();
            // Retrieve the existing parents to remove
            Google.Apis.Drive.v3.FilesResource.GetRequest getRequest = service.Files.Get(fileId);
            getRequest.Fields = "parents";
            Google.Apis.Drive.v3.Data.File file = getRequest.Execute();
            string previousParents = String.Join(",", file.Parents);
            // Move the file to the new folder
            Google.Apis.Drive.v3.FilesResource.UpdateRequest updateRequest = service.Files.Update(new Google.Apis.Drive.v3.Data.File(), fileId);
            updateRequest.Fields = "id, parents";
            updateRequest.AddParents = folderId;
            updateRequest.RemoveParents = previousParents;
            file = updateRequest.Execute();
            if (file != null)
            {
                return "Success";
            }
            else
            {
                return "Fail";
            }
        }

        public static string FileUploadInFolder(string folderId, string filePath, string fileName)
        {
            string fileId = string.Empty;
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    Google.Apis.Drive.v3.DriveService service = GetService_v3();

                    //string path = Path.Combine(HttpContext.Current.Server.MapPath("~/GoogleDriveFiles"),
                    //Path.GetFileName(file.FileName));
                    //file.SaveAs(path);

                    var FileMetaData = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePath) : fileName,
                        MimeType = MimeMapping.GetMimeMapping(filePath)
                    };

                    Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;
                    using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                    {
                        request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
                        request.Fields = "id";
                        request.Upload();
                    }
                    var file1 = request.ResponseBody;
                    fileId = file1.Id;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return fileId;
        }
        public static bool CheckFileExists(string fileId)
        {
            try
            {
                if (fileId != null)
                {
                    Google.Apis.Drive.v3.DriveService service = GetService_v3();
                    var file = service.Files.Get(fileId).Execute();
                    if (file.Trashed == true)
                    {
                        var data = file.Trashed;
                    }
                    if (file.Name != null || file.Name != "")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else {
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("404") == true)
                {
                    return false;
                }
                else
                {
                    throw ex;
                }
            }

        }
        //author:Vamsi 
        //date: 20/03/2020
        //To save invoice file to google drive
        public static string UploadInvoiceFiletoGoogleDrivefolder(string folderId, string fileName, byte[] invoicedoc)
        {
            try
            {
                Google.Apis.Drive.v3.DriveService service = GetService_v3();
                var FileMetaData = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fileName,
                    MimeType = null,
                    Parents = new List<string>
                    {
                        HelperMethods.Literals.GDriveInvoicegFolderId
                    }
                };
                string path = HttpContext.Current.Server.MapPath("~/QuotePurposeAttachments/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filePath = Path.Combine(path, Path.GetFileName(fileName + ".pdf"));
                System.IO.File.WriteAllBytes(filePath, invoicedoc);
                Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;

                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                {
                    request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
                    request.Fields = "id";
                    request.Upload();
                }
                System.IO.File.Delete(filePath);
                var file1 = request.ResponseBody;
                return file1.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}