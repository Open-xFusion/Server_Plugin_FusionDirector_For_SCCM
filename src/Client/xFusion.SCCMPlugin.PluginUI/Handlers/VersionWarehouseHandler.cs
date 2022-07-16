//**************************************************************************  
//Copyright (C) 2019 xFusion Digital Technologies Co., Ltd. All rights reserved.
//This program is free software; you can redistribute it and/or modify
//it under the terms of the MIT license.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//MIT license for more detail.
//*************************************************************************  
﻿using CefSharp;
using CommonUtil;
using xFusion.SCCMPlugin.Core.Exceptions;
using xFusion.SCCMPlugin.Core.Model.Request;
using xFusion.SCCMPlugin.Core.Model.Response;
using xFusion.SCCMPlugin.Core.Workers;
using xFusion.SCCMPlugin.FusionDirector.PluginUI.Attributes;
using xFusion.SCCMPlugin.FusionDirector.PluginUI.Entitys;
using xFusion.SCCMPlugin.FusionDirector.PluginUI.Helper;
using xFusion.SCCMPlugin.FusionDirector.PluginUI.Interface;
using xFusion.SCCMPlugin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xFusion.SCCMPlugin.FusionDirector.PluginUI.Handlers
{
    /// <summary>
    /// 升级仓库
    /// </summary>
    [CefURL("xfusion/upgradePackageWarehouse/index.html")]
    [Bound("NetBound")]
    public class VersionWarehouseHandler : BaseHandler, IWebHandler
    {
        static event EventHandler<AggregateExceptionArgs> AggregateExceptionCatched2;

        CancellationTokenSource TokenSource2 = new CancellationTokenSource();
        HttpWebRequest httpWebRequest;

        public override string Execute(string eventName, object eventData)
        {
            try
            {
                LogUtil.HWLogger.UI.Info("Executing the method of Server page...");
                object result = new ApiResult(ErrorCode.SYS_UNKNOWN_ERR, "unknown");
                switch (eventName)
                {
                    case "requestFusionDirectorAPI":
                        result = RequestFusionDirectorAPI(eventData);
                        break;
                    case "loadFDList":
                        result = LoadFDList(eventData);
                        break;
                    case "uploadPackage":
                        result = UploadPackage(eventData);
                        break;
                    case "uploadSignature":
                        result = UploadSignature(eventData);
                        break;
                    case "cancelPackageUpload":
                        result = CancelPackageUpload();
                        break;
                    case "cancelSignatureUpload":
                        result = CancelSignatureUpload();
                        break;
                    default: break;
                }
                return JsonConvert.SerializeObject(result);
            }
            catch (JsonSerializationException ex)
            {
                LogUtil.HWLogger.UI.Error("Call JsonConvert.SerializeObject failed.", ex);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Execute the method in Server page failed.", ex);
            }
            return "{code:" + ErrorCode.SYS_UNKNOWN_ERR + ",data:[]}";
        }


        /// <summary>
        /// 取消上传升级包
        /// </summary>
        /// <returns></returns>
        public CommonResponse CancelPackageUpload()
        {
            var ret = new CommonResponse("0");
            try
            {
                httpWebRequest.Abort();
            }
            catch (Exception ex)
            {

                LogUtil.HWLogger.UI.Error("Exe CancelPackageUpload failed: ", ex);
            }
            return ret;
        }

        /// <summary>
        /// 取消上传签名
        /// </summary>
        /// <returns></returns>
        public CommonResponse CancelSignatureUpload()
        {
            var ret = new CommonResponse("0");
            try
            {
                LogUtil.HWLogger.UI.Error("Exe CancelSignatureUpload");
                TokenSource2.Cancel();
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Exe CancelSignatureUpload failed: ", ex);
            }
            return ret;
        }

        ///上传升级包
        /// </summary>
        public CommonResponse UploadPackage(object eventData)
        {
            var ret = new CommonResponse("0");
            var jsData = JsonUtil.SerializeObject(eventData);
            var request = JsonUtil.DeserializeObject<CommonRequest>(jsData);
            ret.Ips = request.Ips;
            if (!File.Exists(request.Data.ToString()))
            {
                ret.Code = ErrorCode.FILE_NOTFOUND;
                LogUtil.HWLogger.UI.Error("Upload Package failed: File path error，error path " + request.Data.ToString());
                return ret;
            }
            var fusionDirector = FusionDirectorWorker.Instance.FindByIP(request.Ips[0]);
            if (fusionDirector == null)
            {
                ret.Code = ErrorCode.NET_FD_NOFOUND;
                LogUtil.HWLogger.UI.Error("Upload Package failed: not found Fd" + request.Ips[0].ToString());
                return ret;
            }
            Task.Factory.StartNew(() =>
            {
                try
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(fusionDirector.LoginAccount + ":" + fusionDirector.LoginPwd);
                    var authStr = "Basic " + Convert.ToBase64String(bytes);
                    var remoteUrl = "https://" + fusionDirector.HostIP + ":" + fusionDirector.Port + "/redfish/v1" + request.Endpoint;

                    FileStream fs = new FileStream(request.Data.ToString(), FileMode.Open, FileAccess.Read);
                    ret.Description = Math.Round((decimal)fs.Length / (1024 * 1024), 0).ToString();
                    byte[] boundaryBytes;

                    using (BinaryReader r = new BinaryReader(fs))
                    {
                        var fileName = request.Data.ToString().Substring(request.Data.ToString().LastIndexOf('\\') + 1);
                        //时间戳
                        string strBoundary = "--" + DateTime.Now.Ticks.ToString("x");
                        boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "\r\n");
                        //请求头部信息
                        StringBuilder sb = new StringBuilder();
                        sb.Append("--");
                        sb.Append(strBoundary);
                        sb.Append("\r\n");
                        sb.Append("Content-Disposition: form-data; name=\"");
                        sb.Append("tiFile");
                        sb.Append("\"; filename=\"" + fileName + "");
                        sb.Append("\";");
                        sb.Append("\r\n");
                        sb.Append("Content-Type: ");
                        sb.Append("application/octet-stream;");
                        sb.Append("\r\n");
                        sb.Append("\r\n");
                        string strPostHeader = sb.ToString();
                        byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);
                        //默认忽略证书
                        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                        //兼容所有ssl协议
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)(3072 | 768 | 192 | 48);
                        httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(remoteUrl));
                        httpWebRequest.Method = "POST";

                        httpWebRequest.AllowWriteStreamBuffering = false;
                        httpWebRequest.Timeout = 1000 * 60 * 60 * 3;
                        httpWebRequest.Headers.Add("Authorization", authStr);
                        httpWebRequest.ContentType = "multipart/form-data; boundary=" + strBoundary;
                        long length = fs.Length + postHeaderBytes.Length + boundaryBytes.Length;
                        long fileLength = fs.Length;
                        httpWebRequest.ContentLength = length;

                        //每次上传4k
                        int bufferLength = 4096;
                        byte[] buffer = new byte[bufferLength];
                        //已上传的字节数
                        long offset = 0;
                        //开始上传时间
                        DateTime startTime = DateTime.Now;
                        int size = r.Read(buffer, 0, bufferLength);
                        using (Stream postStream = httpWebRequest.GetRequestStream())
                        {     //发送请求头部消息
                            postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                            while (size > 0)
                            {
                                postStream.Write(buffer, 0, size);
                                offset += size;
                                var Progress = Convert.ToInt32(Math.Round((decimal)offset / length, 2) * 100);
                                FDBrowser.ExecuteScriptAsync("setUploadPackageProgress", Progress);
                                Application.DoEvents();
                                size = r.Read(buffer, 0, bufferLength);
                            }
                            //添加尾部的时间戳
                            postStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                        }
                    }

                    //获取服务器端的响应

                    WebResponse webRespon = httpWebRequest.GetResponse();
                    string content;
                    using (Stream requestStream = webRespon.GetResponseStream())
                    using (StreamReader sr = new StreamReader(requestStream, System.Text.Encoding.UTF8))
                    {
                        content = sr.ReadToEnd();
                    }
                    LogUtil.HWLogger.API.Info(content);
                    var response = httpWebRequest.GetResponse() as HttpWebResponse;
                    if (response == null) throw new ArgumentNullException(nameof(response));
                    var statusCode = ((int)response.StatusCode).ToString();
                    var task = response.Headers.GetValues("Task");
                    if (task != null)
                    {
                        ret.Headers = new
                        {
                            Task = task[0]
                        };
                    }
                    ret.Data.Add(new Item()
                    {
                        Ip = fusionDirector.HostIP,
                        Code = statusCode,
                        Data = JsonConvert.DeserializeObject(content)
                    });
                    if (!statusCode.StartsWith("2"))
                    {
                        throw new Exception(statusCode);
                    }
                }
                catch (BaseException ex)
                {
                    LogUtil.HWLogger.UI.Error("Upload Package failed: ", ex);
                    ret.Code = ex.Code;
                }
                catch (Exception ex)
                {
                    LogUtil.HWLogger.UI.Error("Upload Package failed: ", ex);
                    ret.Code = ErrorCode.SYS_UNKNOWN_ERR;
                }
                ret.Ips = request.Ips;
                FDBrowser.ExecuteScriptAsync("uploadPackageFileSuccess", JsonConvert.SerializeObject(ret));//执行升级包上传成功方法  uploadPackageFileSuccess是UI js升级包上传成功方法

            });
            return ret;

        }

        ///上传签名
        /// </summary>
        public CommonResponse UploadSignature(object eventData)
        {
            var ret = new CommonResponse("0");
            var jsData = JsonUtil.SerializeObject(eventData);
            var request = JsonUtil.DeserializeObject<CommonRequest>(jsData);
            ret.Ips = request.Ips;
            if (!File.Exists(request.Data.ToString()))
            {
                ret.Code = ErrorCode.FILE_NOTFOUND;
                LogUtil.HWLogger.UI.Error("Upload Signatur failed: File path error，error path " + request.Data.ToString());
                return ret;
            }
            var fusionDirector = FusionDirectorWorker.Instance.FindByIP(request.Ips[0]);
            if (fusionDirector == null)
            {
                ret.Code = ErrorCode.NET_FD_NOFOUND;
                LogUtil.HWLogger.UI.Error("Upload Signatur failed: not found Fd" + request.Ips[0].ToString());
                return ret;
            }
            AggregateExceptionCatched2 += new EventHandler<AggregateExceptionArgs>(Program_AggregateExceptionCatched2);
            TokenSource2 = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(fusionDirector.LoginAccount + ":" + fusionDirector.LoginPwd);
                    var authStr = "Basic " + Convert.ToBase64String(bytes);
                    var remoteUrl = "https://" + fusionDirector.HostIP + ":" + fusionDirector.Port + "/redfish/v1" + request.Endpoint;

                    //默认忽略证书
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    //兼容所有ssl协议
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)(3072 | 768 | 192 | 48);
                    var file = File.Open(request.Data.ToString(), FileMode.Open, FileAccess.Read);
                    ret.Description = Math.Round((decimal)file.Length / (1024 * 1024), 0).ToString();
                    var fileContent = new StreamContent(file)
                    {
                        Headers = { ContentLength = file.Length, ContentType = new MediaTypeHeaderValue("application/octet-stream") }
                    };
                    var formDataContent = new MultipartFormDataContent();
                    var fileName = file.Name.Substring(file.Name.LastIndexOf('\\') + 1);
                    formDataContent.Add(fileContent, "tiFile", fileName);
                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, remoteUrl);
                    var progressContent = new ProgressableStreamContent(formDataContent, 4096 * 100, (sent, total) =>
                    {
                        try
                        {
                            var Progress = Convert.ToInt32(Math.Round((decimal)sent / total, 2) * 100);
                            FDBrowser.ExecuteScriptAsync("setUploadSignaturProgress", Progress);//设置升级包上传进度  setUploadSignaturProgress是UI js方法名称 Progress是进度 0-100
                        }
                        catch (Exception ex)
                        {
                            AggregateExceptionArgs args = new AggregateExceptionArgs()
                            {
                                AggregateException = new AggregateException(ex)
                            };
                            //使用主线程委托代理，处理子线程 异常
                            //这种方式没有阻塞 主线程或其他线程
                            AggregateExceptionCatched2?.Invoke(null, args);
                            throw ex;
                        }

                    });
                    httpRequestMessage.Content = progressContent;
                    httpRequestMessage.Headers.Add("Authorization", authStr);
                    using (HttpClient httpClient = new HttpClient())
                    {
                        var httpResponse = httpClient.SendAsync(httpRequestMessage, TokenSource2.Token).Result;
                        var statusCode = ((int)httpResponse.StatusCode).ToString();
                        var resultStr = httpResponse.Content.ReadAsStringAsync().Result;
                        LogUtil.HWLogger.API.Info("URL:" + remoteUrl + " Api Result:" + resultStr);
                        ret.Data.Add(new Item()
                        {
                            Ip = fusionDirector.HostIP,
                            Code = statusCode,
                            Data = JsonConvert.DeserializeObject(resultStr)
                        });
                        if (!statusCode.StartsWith("2"))
                        {
                            throw new BaseException(statusCode, "");
                        }
                        fileContent.Dispose();
                    }
                    file.Dispose();
                }
                catch (BaseException ex)
                {
                    LogUtil.HWLogger.UI.Error("Upload Signatur failed: ", ex);
                    ret.Code = ex.Code;
                }
                catch (Exception ex)
                {
                    LogUtil.HWLogger.UI.Error("Upload Signatur failed: ", ex);
                    ret.Code = ErrorCode.SYS_UNKNOWN_ERR;
                    AggregateExceptionArgs args = new AggregateExceptionArgs()
                    {
                        AggregateException = new AggregateException(ex)
                    };
                    //使用主线程委托代理，处理子线程 异常
                    //这种方式没有阻塞 主线程或其他线程
                    AggregateExceptionCatched2?.Invoke(null, args);
                }
                ret.Ips = request.Ips;
                FDBrowser.ExecuteScriptAsync("uploadSignaturFileSuccess", JsonConvert.SerializeObject(ret));//执行升级包上传成功方法  uploadSignaturFileSuccess是UI js升级包上传成功方法
            });
            return ret;

        }

        void Program_AggregateExceptionCatched2(object sender, AggregateExceptionArgs e)
        {
            foreach (var item in e.AggregateException.InnerExceptions)
            {
                LogUtil.HWLogger.UI.ErrorFormat("异常类型：{0}{1}来自：{2}{3}异常内容：{4}",
                    item.GetType(), Environment.NewLine, item.Source,
                    Environment.NewLine, item.Message);
            }
        }
    }
}
