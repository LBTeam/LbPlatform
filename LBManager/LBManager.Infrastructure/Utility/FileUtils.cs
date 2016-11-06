using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Utility
{
    public static class FileUtils
    {
        public static string ComputeContentMd5(Stream input, long partSize)
        {
            using (var md5 = MD5.Create())
            {
                int readSize = (int)partSize;
                long pos = input.Position;
                byte[] buffer = new byte[readSize];
                readSize = input.Read(buffer, 0, readSize);

                var data = md5.ComputeHash(buffer, 0, readSize);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        public static string ComputeFileMd5(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                return ComputeContentMd5(fileStream, fileStream.Length);
            }
        }
    }
}
