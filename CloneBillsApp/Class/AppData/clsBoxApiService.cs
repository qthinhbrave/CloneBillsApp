using Box.V2;
using Box.V2.Auth;
using Box.V2.Config;
using Box.V2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneBillsApp.Class.AppData
{
    public class clsBoxApiService
    {

        const string CLIENT_ID = "e72srbaqof4qibuek1rzpfzygor2xx5f";
        const string CLIENT_SECRET = "mqP1zgWRTCddfa73u4PDOCMEQjueCJl5";
        const string DEV_ACCESS_TOKEN = "67pJiyj3UPXh9dWnCQAf3w3RLXq46btP";  //log into developers.box.com and get this for your registered app; it will last for 60 minutes
        const string REFRESH_TOKEN = ""; //"THIS_IS_NOT_NECESSARY_FOR_A_DEV_TOKEN_BUT_MUST_BE_HERE";

        //set these to point to whatever file you want to upload; make sure it exists!
        //const string PATH_TO_FILE = @"D:\Source folder\Map_01.png";
        const string PATH_TO_FILE = @"D:\Source folder";
        static BoxClient client;

        public clsBoxApiService()
        {
            try
            {
                var config = new BoxConfig(CLIENT_ID, CLIENT_SECRET, new Uri("http://localhost"));
                var session = new OAuthSession(DEV_ACCESS_TOKEN, REFRESH_TOKEN, 3600, "bearer");
                client = new BoxClient(config, session);
                //var boxConfig = new BoxConfigBuilder(CLIENT_ID, CLIENT_SECRET).Build();
                //var boxCCG = new BoxCCGAuth(boxConfig);

                //AsyncContext.Run(() => MainAsync());
            }
            catch (Exception ex)
            {
                //Console.Error.WriteLine(ex);
            }
        }

        public async Task BOX_Upload(string file)
        {
            await Task.Run(async () =>
            {
                string subPath = Path.GetDirectoryName(file).Replace(PATH_TO_FILE, "");
                string fileName = Path.GetFileName(file);
                string toPath = String.Format("{0}{1}", clsCommon.APP_NAME, subPath);
                var boxFolderId = await CreateDirectoryIfNotExist(toPath);

                if (boxFolderId != null && boxFolderId != "0")
                {
                    using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read))
                    {
                        Console.WriteLine("Uploading file {0}...", file);

                        // Create request object with name and parent folder the file should be uploaded to
                        BoxFileRequest request = new BoxFileRequest()
                        {
                            Name = fileName,
                            Parent = new BoxRequestEntity() { Id = boxFolderId }
                        };
                        //BoxFile f = 
                        await client.FilesManager.UploadAsync(request, fs);
                        //Console.WriteLine("File Id {0}", f.Id);
                    }
                }
            });
        }

        private async Task<String> CreateDirectoryIfNotExist(string path)
        {
            var currFolderId = "0"; //the root folder is always "0"
            try
            {
                var folderNames = path.Split('\\');
                folderNames = folderNames.Where((f) => !String.IsNullOrEmpty(f)).ToArray();

                foreach (string folderName in folderNames)
                {
                    BoxCollection<BoxItem> folderItems = await client.FoldersManager.GetFolderItemsAsync(currFolderId, 100);
                    var foundFolder = folderItems.Entries.OfType<BoxItem>().FirstOrDefault((f) => f.Name == folderName && f.Type.Equals("folder", StringComparison.OrdinalIgnoreCase));
                    if (foundFolder != null)
                    {
                        currFolderId = foundFolder.Id;
                    }
                    else
                    {
                        // create new folder
                        BoxFolderRequest folderRequest = new BoxFolderRequest()
                        {
                            Name = folderName,
                        };
                        if (currFolderId != "0")
                        {
                            // set Parent
                            folderRequest.Parent = new BoxRequestEntity() { Id = currFolderId };
                        }
                        var newFolder = client.FoldersManager.CreateAsync(folderRequest, null).Result;
                        currFolderId = newFolder.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                currFolderId = "0";
            }
            return currFolderId;
        }

    }
}
