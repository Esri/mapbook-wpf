using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OfflineMapBook.ViewModels
{
    class DownloadViewModel : BaseViewModel
    {
        //         try
        //            {
        //                DownloadRemoteImageFile(thumbnailUri.ToString(), localFilename);
        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.WriteLine(ex.StackTrace.ToString());
        //            }
        //var map = mmpk.Maps[0];
        //var mapName = map.Item.Title;

        //var itemData = map.Item as Item;


        //var mapThumbnailFile = @"C:\Users\mara8799\Documents\visual studio 2015\Projects\OfflineMapBook\OfflineMapBook\Images\mapthumbnail.png";
        //            //DownloadRemoteImageFile(mapThumbnailUri.ToString(), mapThumbnailFile);

        private static void DownloadRemoteImageFile(string uri, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {

                // if the remote file was found, download oit
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
            }
        }

    }
}
