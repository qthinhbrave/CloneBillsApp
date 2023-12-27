using CloneBillsApp.Class.VacsMapApp;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloneBillsApp.Class.AppData
{
    public class clsGoogleApiSevice
    {
        static string crePath = Path.Combine(Application.StartupPath);
        static FileDataStore _fileDataStore = new FileDataStore(crePath, true);

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string LocalUserName { set; get; }
        public DriveService _service { get; set; }

        public clsGoogleApiSevice(string clientid, string clientsecret)
        {
            this.ClientId = clientid;
            this.ClientSecret = clientsecret;
            this.LocalUserName = clsCommon.APP_NAME;
        }
        /// <summary>
        /// Kết nối với tài khoản Google Drive, nếu kết nối rồi thì sẽ chạy hàm xong luôn
        /// </summary>
        public async Task LoginAsync()
        {
            var app = new ClientSecrets() { ClientId = this.ClientId, ClientSecret = this.ClientSecret };
            var scopes = new[] { DriveService.Scope.Drive,
                                 DriveService.Scope.DriveFile,
                                 DriveService.Scope.DriveAppdata,
                                 DriveService.Scope.DriveReadonly};

            //Kiểm tra thông tin đã xác thực chưa? Nếu chưa thì yêu cầu xác thực
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                app,
                scopes,
                this.LocalUserName,
                System.Threading.CancellationToken.None,
                _fileDataStore);

            //Tạo service
            _service = new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = clsCommon.APP_NAME
            });
        }

        /// <summary>
        /// Kiểm tra trang thái kết nối với tài khoản Google. True nếu đã kết nối thành công
        /// </summary>
        /// <returns></returns>
        public bool CheckLogin()
        {
            return CheckLogin(this.LocalUserName);
        }

        /// <summary>
        /// Kiểm tra trang thái kết nối với tài khoản Google. True nếu đã kết nối thành công
        /// </summary>
        /// <returns></returns>
        public static bool CheckLogin(string localusername)
        {
            string fileSave = string.Format("{0}\\{1}", _fileDataStore.FolderPath, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-" + localusername);
            return System.IO.File.Exists(fileSave);
        }

        /// <summary>
        /// Tạo thư mục trên drive
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public async Task<string> CreateFolderIfNotExistAync(string folderName)
        {
            #region Tìm kiếm có thì trả về
            var timKiemFolder_request = _service.Files.List();
            timKiemFolder_request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and trashed=false";
            var timKiemFolder_response = await timKiemFolder_request.ExecuteAsync();
            if (timKiemFolder_response.Files.Count >= 1)
                return timKiemFolder_response.Files[0].Id;
            #endregion

            #region Tạo thư mục
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var request = _service.Files.Create(fileMetadata);

            request.Fields = "id";
            var file = await request.ExecuteAsync();
            return file.Id;
            #endregion
        }

        /// <summary>
        /// Tải một tệp lên Drive
        /// </summary>
        /// <param name="filepath">Đường dẫn tệp trên máy</param>
        /// <param name="cancel_token"></param>
        /// <param name="folderid">Folder ID trên drive, null là upload lên thư mục gốc</param>
        /// <param name="callback_progressuploadchanged">Thông báo tiến trình upload</param>
        /// <param name="filename">Tên tệp trên drive, null là cùng với tên tệp trên máy</param>
        /// <param name="mimetype"></param>
        /// <param name="shareanyonewithlink">Chia sẻ link sau khi tải lên xong</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> UploadFileAsync(string filepath,
                                  CancellationToken cancel_token,
                                  string folderid = null,
                                  Action<double> callback_progressuploadchanged = null,
                                  string filename = null,
                                  string mimetype = "",
                                  bool shareanyonewithlink = false)
        {
            //Code vi du: https://developers.google.com/drive/v3/web/manage-uploads

            FileInfo fInfo = new FileInfo(filepath);
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = filename == null ? fInfo.Name : filename,
                Description = "",
                Parents = folderid != null ? new List<string> { folderid } : null
            };

            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                var requestUpload = _service.Files.Create(fileMetadata, fs, mimetype);

                if (callback_progressuploadchanged != null)
                    requestUpload.ProgressChanged += (u) => callback_progressuploadchanged((double)u.BytesSent / fs.Length);

                var upload_response = await requestUpload.UploadAsync(cancel_token);

                if (upload_response.Exception != null)
                {
                    //if(upload_response.Exception is Google.GoogleApiException)

                    throw upload_response.Exception;
                }

                if (requestUpload.ResponseBody == null)
                    throw new Exception("Upload fail");
                else
                {
                    if (shareanyonewithlink)
                    {
                        string linkShare = await ShareAnyoneWithLinkAsync(_service, requestUpload.ResponseBody.Id);
                        //return linkShare;
                    }

                    return requestUpload.ResponseBody.Id;
                }
            }
        }

        /// <summary>
        /// Chia sẻ một file với tất cả mọi người
        /// </summary>
        /// <param name="fileid"></param>
        /// <returns></returns>
        private async Task<string> ShareAnyoneWithLinkAsync(DriveService service, string fileid)
        {
            Permission permision = new Permission()
            {
                Type = "anyone",
                Role = "reader"
            };
            var requestShare = service.Permissions.Create(permision, fileid);
            await requestShare.ExecuteAsync();
            return $"https://drive.google.com/file/d/{fileid}/view?usp=sharing";
        }

        public String CreateDirectoryIfNotExist(String folderName)
        {
            try
            {
                string[] folderTree = folderName.Split('\\');
                String googleFolderIdParent = null;
                String name = null;
                foreach (string folder in folderTree)
                {
                    name = folder;
                    bool flagFound = false;
                    List<Google.Apis.Drive.v3.Data.File> list = getGoogleSubFolders(googleFolderIdParent);
                    foreach (Google.Apis.Drive.v3.Data.File file in list)
                    {
                        if (file.Name.Equals(name))
                        {
                            // Found folder [name]
                            flagFound = true;
                            googleFolderIdParent = file.Id;
                            break;
                        }
                    }
                    if (!flagFound)
                    {
                        // Not found => Create folder [name]
                        googleFolderIdParent = CreateDirectory(name, googleFolderIdParent);
                    }
                }

                return googleFolderIdParent;
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
            }

            return null;
        }

        private String CreateDirectory(String folderName, String googleFolderIdParent)
        {
            try
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder"
                };
                if (googleFolderIdParent != null)
                {
                    fileMetadata.Parents = new List<string>() { googleFolderIdParent };
                }

                var request = _service.Files.Create(fileMetadata);

                //request.Fields = "files(id, name)";
                var file = request.Execute();
                return file.Id;
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
            }

            return null;
        }

        private List<Google.Apis.Drive.v3.Data.File> getGoogleSubFolders(String googleFolderIdParent)
        {
            try
            {
                String query = null;
                if (googleFolderIdParent == null)
                {
                    query = " mimeType = 'application/vnd.google-apps.folder' " //
                            + " and 'root' in parents";
                }
                else
                {
                    query = " mimeType = 'application/vnd.google-apps.folder' " //
                            + " and '" + googleFolderIdParent + "' in parents";
                }

                FilesResource.ListRequest listRequest = _service.Files.List();
                listRequest.PageSize = 10;
                listRequest.Fields = "nextPageToken, files(id, name, createdTime)";
                listRequest.Spaces = "drive";
                listRequest.Q = query;

                // Fields will be assigned values: id, name, createdTime
                IList<Google.Apis.Drive.v3.Data.File> list = listRequest.Execute().Files;

                return list.ToList();
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
            }

            return null;
        }

        public bool ExistsFile(String folderid, String fileName)
        {
            try
            {
                var request = _service.Files.List();
                request.PageSize = 100;
                request.Q = $"parents in '{folderid}'";
                var results = request.Execute();

                foreach (var driveFile in results.Files)
                {
                    if (driveFile.Name == fileName)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLogger.Err(ex.Message);
            }

            return false;
        }
    }
}
