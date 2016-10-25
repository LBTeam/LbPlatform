using LBManager.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Utility
{
    public class UtilityTool
    {
        public static FileContentType MediaTypeToContentType(MediaType mediaType)
        {
            FileContentType contentType;
            switch (mediaType)
            {
                case MediaType.Image:
                    contentType = FileContentType.Image;
                    break;
                case MediaType.Video:
                    contentType = FileContentType.Video;
                    break;
                case MediaType.Text:
                    contentType = FileContentType.Text;
                    break;
                default:
                    contentType = FileContentType.Schedule;
                    break;
            }
            return contentType;
        }
    }
}
