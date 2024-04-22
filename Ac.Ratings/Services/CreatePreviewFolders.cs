using System.IO;

namespace Ac.Ratings.Services {
    public class CreatePreviewFolders {
        private const string _acRootFolder = @"D:\\SteamLibrary\\steamapps\\common\\assettocorsa\\content\\cars";
        private const string _previewsFolder = @"C:\Users\ReneVa\source\repos\Ac.Ratings\Ac.Ratings\Resources\Previews\";
        private readonly InitializeData _data = new InitializeData();

        public CreatePreviewFolders() {
            
            GetPreviewsFromFolders(_acRootFolder);
        }

        private void GetPreviewsFromFolders(string rootFolder) {
            
            var carDirectories = Directory.GetDirectories(rootFolder);

            foreach (string carDir in carDirectories) {
                string fileName = Path.GetFileName(carDir);

                string skinsDir = Path.Combine(carDir, "skins");

                if (Directory.Exists(skinsDir)) {
                    string[] skinDirectories = Directory.GetDirectories(skinsDir);
                    string sourceFile = Path.Combine(skinsDir, skinDirectories[0], "preview.jpg");
                    if (File.Exists(sourceFile)) {
                        string targetFile = Path.Combine(_previewsFolder, $"{fileName}-preview.jpg");
                        File.Copy(sourceFile, targetFile, true);
                    }
                }
            }
        }
    }
}
