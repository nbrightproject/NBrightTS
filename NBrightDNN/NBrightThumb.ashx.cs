﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using NBrightCorev2.common;
using NBrightCorev2.images;

namespace NBrightDNNv2
{
    /// <summary>
    /// Summary description for NBrightThumb1
    /// </summary>
    public class NBrightThumb : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            var w = Utils.RequestQueryStringParam(context, "w");
            var h = Utils.RequestQueryStringParam(context, "h");
            var src = Utils.RequestQueryStringParam(context, "src");
            var imgtype = Utils.RequestQueryStringParam(context, "imgtype");

            if (h == "") h = "0";
            if (w == "") w = "0";

            if (Utils.IsNumeric(w) && Utils.IsNumeric(h))
            {
                src = HttpContext.Current.Server.MapPath(src);

                var strCacheKey = context.Request.Url.Host.ToLower() + "*" + src + "*" + Utils.GetCurrentCulture() + "*img:" + w + "*" + h + "*";
                var newImage = (Bitmap) Utils.GetCache(strCacheKey);

                if (newImage == null)
                {
                    newImage = ImgUtils.CreateThumbnail(src, Convert.ToInt32(w), Convert.ToInt32(h));
                    Utils.SetCache(strCacheKey, newImage);
                }

                if ((newImage != null))
                {
                    ImageCodecInfo useEncoder;

                    // due to issues on some servers not outputing the png format correctly from the thumbnailer.
                    // this thumbnailer will always output jpg, unless specifically told to do a png format.
                    useEncoder = ImgUtils.GetEncoder(ImageFormat.Jpeg);
                    if (imgtype.ToLower() == "png")  useEncoder = ImgUtils.GetEncoder(ImageFormat.Png);                        

                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 85L);

                    try
                    {
                        newImage.Save(context.Response.OutputStream, useEncoder, encoderParameters);
                    }
                    catch (Exception exc)
                    {
                        var outArray = Utils.StrToByteArray(exc.ToString());
                        context.Response.OutputStream.Write(outArray, 0, outArray.Count());
                    }
                }
            }
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}