using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VrTeamTesting.Common.Models
{
    public class FileModel
    {
        public FileModel() { }

        public string ContentDisposition { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } = "API";
        public byte[] Contents { get; set; }

        public FileModel(IFormFile file)
        {
            Name = file.Name;
            FileName = file.FileName;
            ContentType = file.ContentType;
            ContentDisposition = file.ContentDisposition;
            var ms = new MemoryStream();
            file.CopyTo(ms);
            Contents = ms.ToArray();
            ms.Close();
        }
    }
}
