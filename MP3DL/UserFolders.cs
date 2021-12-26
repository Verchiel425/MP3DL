using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace MP3DL
{
    class UserFoldersClass
    {
        public List<string> MusicFolders { get; set; }
    }
    class UserFolders
    {
        private string name = "musicfolders.json";
        public List<string> MUSICFOLDERS = new();
        public void Save()
        {

            Debug.WriteLine("--Saving to config--");
            UserFoldersClass _client = new UserFoldersClass
            {
                MusicFolders = MUSICFOLDERS
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_client, options);
            System.IO.File.WriteAllText(name, json);
        }
        public List<string> GetMUSICFOLDERS()
        {
            if (System.IO.File.Exists(name))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(name);
                    UserFoldersClass _client = JsonSerializer.Deserialize<UserFoldersClass>(json);
                    List<string> MUSICFOLDERS = _client.MusicFolders;
                    return MUSICFOLDERS;
                }
                catch (Exception)
                {
                    System.IO.File.Delete(name);
                    return new List<string>();
                }
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
