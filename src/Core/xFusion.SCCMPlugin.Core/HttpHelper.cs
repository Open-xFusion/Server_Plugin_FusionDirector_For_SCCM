//**************************************************************************  
//Copyright (C) 2019 xFusion Digital Technologies Co., Ltd. All rights reserved.
//This program is free software; you can redistribute it and/or modify
//it under the terms of the MIT license.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//MIT license for more detail.
//*************************************************************************  
﻿using CommonUtil;
using xFusion.SCCMPlugin.Core.Exceptions;
using xFusion.SCCMPlugin.Core.Model.Request;
using xFusion.SCCMPlugin.Core.Model.Response;
using xFusion.SCCMPlugin.Core.Workers;
using xFusion.SCCMPlugin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace xFusion.SCCMPlugin.Core
{
    public class HttpHelper
    {

        /// <summary>
        /// 单例
        /// </summary>
        public static HttpHelper Instance
        {
            get { return SingletonProvider<HttpHelper>.Instance; }
        }
        /// <summary>
        /// 定义需要使用的系统枚举。
        /// </summary>
        [Flags]
        private enum MySecurityProtocolType
        {
            //
            // Summary:
            //     Specifies the Secure Socket Layer (SSL) 3.0 security protocol.
            Ssl3 = 48,
            //
            // Summary:
            //     Specifies the Transport Layer Security (TLS) 1.0 security protocol.
            Tls = 192,
            //
            // Summary:
            //     Specifies the Transport Layer Security (TLS) 1.1 security protocol.
            Tls11 = 768,
            //
            // Summary:
            //     Specifies the Transport Layer Security (TLS) 1.2 security protocol.
            Tls12 = 3072

        }

        /// <summary>
        /// 处理请求FD业务API
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public CommonResponse CommonHttpHandle(CommonRequest request)
        {
            var res = new CommonResponse("0");
            res.Ips = request.Ips;
            try
            {
                if (!HttpWhiteListHepler.Instance.Validate(request.Endpoint, request.HttpMethod))
                {
                    res.Code = ErrorCode.REQUESTNOTSUPPORTED_ERROR;
                    return res;
                }
                var fusionDirector = FusionDirectorWorker.Instance.FindByIP(request.Ips[0]);
                if (fusionDirector == null)
                {
                    res.Code = ErrorCode.NET_FD_NOFOUND;
                    return res;
                }
                // IsCrtExist();
                // TrustCertificate();
                SetCertificateValidation();
                HttpResponseMessage response = new HttpResponseMessage();
                
                using (HttpClient httpClient = new HttpClient())
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(fusionDirector.LoginAccount + ":" + fusionDirector.LoginPwd);
                    var authStr = "Basic " + Convert.ToBase64String(bytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", authStr);
                    var remoteUrl = "https://" + fusionDirector.HostIP + ":" + fusionDirector.Port + "/redfish/v1" + request.Endpoint;
                    switch (request.HttpMethod)
                    {
                        case "GET":
                            response = httpClient.GetAsync(remoteUrl).Result;
                            break;
                        case "POST":
                            var content = new StringContent(request.Data.ToString(), Encoding.UTF8, "application/json");
                            response = httpClient.PostAsync(remoteUrl, content).Result;
                            break;
                        case "DELETE":
                            response = httpClient.DeleteAsync(remoteUrl).Result;
                            break;
                        case "PATCH":
                            var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), remoteUrl)
                            {
                                Content = new StringContent(request.Data.ToString(), Encoding.UTF8, "application/json")
                            };
                            response = httpClient.SendAsync(requestMessage).Result;
                            break;
                        default:
                            res.Code = ErrorCode.REQUESTNOTSUPPORTED_ERROR;
                            return res;
                    }
                    var statusCode = ((int)response.StatusCode).ToString();
                    var task = response.Headers.FirstOrDefault(x => x.Key == "Task").Value;
                    if (task != null)
                    {
                        res.Headers = new
                        {
                            Task = task
                        };
                    }
                    res.Description = response.Headers.Date.Value.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    var resultStr = response.Content.ReadAsStringAsync().Result;
                    LogUtil.HWLogger.API.Info("URL:" + remoteUrl + "HttpMethod:" + request.HttpMethod  + " Api Result:" + resultStr);
                    JsonSerializerSettings setting = new JsonSerializerSettings();
                    JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                    {
                        //日期类型默认格式化处理
                        setting.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                        //setting.DateFormatString = "yyyy-MM-ddTHH:mm:ssz";
                        return setting;
                    });
                    res.Data.Add(new Item()
                    {
                        Ip = fusionDirector.HostIP,
                        Code = statusCode,
                        Data = JsonConvert.DeserializeObject(resultStr, setting)
                    });
                    if (!statusCode.StartsWith("2"))
                    {
                        throw new Exception(statusCode);
                    }
                }

            }
            catch (CryptographicException ex)
            {
                LogUtil.HWLogger.UI.Error("CommonHttpHandle failed: cert error:", ex);
                res.Code = ErrorCode.CERT_ERROR;
                res.Description = ex.Message;
            }
            catch (AggregateException ex)
            {
                LogUtil.HWLogger.API.Error("CommonHttpHandle Error", ex);
                throw HandleException(ex);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.API.Error("CommonHttpHandle Error", ex);
                res.Code = "-1";
                res.Description = ex.Message;
            }
            return res;

        }


        /// <summary>
        /// 测试链接FD
        /// </summary>
        /// <returns></returns>
        public bool TestLinkFD(FusionDirectorModel fusionDirector)
        {
            try
            {
                //IsCrtExist();
                // TrustCertificate();
                SetCertificateValidation();
                HttpResponseMessage response = new HttpResponseMessage();
                using (HttpClient httpClient = new HttpClient())
                {
                    var remoteUrl = "https://" + fusionDirector.HostIP + ":" + fusionDirector.Port + "/redfish/v1/SessionService/Sessions";
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(fusionDirector.LoginAccount + ":" + fusionDirector.LoginPwd);
                    var authStr = "Basic " + Convert.ToBase64String(bytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", authStr);
                    response = httpClient.GetAsync(remoteUrl).Result;
                    var statusCode = ((int)response.StatusCode).ToString();
                    if (statusCode.StartsWith("2"))
                    {
                        return true;
                    }
                    else if (statusCode == "430" || statusCode == "400")
                    {
                        throw new BaseException(ErrorCode.SYS_USER_LOGING, "FD用户名或者密码错误");
                    }
                    else if (statusCode == "445")
                    {
                        throw new BaseException(ErrorCode.SYS_USER_LOCK, "FD用户被加锁");
                    }
                    else if (statusCode == "431")
                    {
                        throw new BaseException(ErrorCode.SYS_USER_LOCK, "FD用户类型错误");
                    }
                    else if (statusCode == "429")
                    {
                        throw new BaseException(ErrorCode.HW_LOGIN_AUTH, "系统错误");
                    }
                    else if (statusCode == "432")
                    {
                        throw new BaseException(ErrorCode.HW_LOGIN_AUTH, "会话创建达到上限");
                    }
                    else
                    {
                        throw new BaseException(ErrorCode.SYS_USER_LOGING, "FD用户名或者密码错误");
                    }
                }
            }
            catch (AggregateException ex)
            {
                LogUtil.HWLogger.API.Error("TestLinkFD Error", ex);
                throw HandleException(ex);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.API.Error("TestLinkFD Error", ex);
                throw ex;
            }

        }

        public void SetCertificateValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                return sslPolicyErrors == SslPolicyErrors.None || sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch;
            };
            //只支持tls1.2及以上协议
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)MySecurityProtocolType.Tls12;
        }

        /// <summary>
        /// 解析复合Exception里的innerException,返回innerException
        /// </summary>
        /// <param name="ae">复合Exception</param>
        /// <returns>inner Exception 列表</returns>
        private List<Exception> GetFlattenAggregateException(AggregateException ae)
        {
            // Initialize a collection to contain the flattened exceptions.
            List<Exception> flattenedExceptions = new List<Exception>();

            // Create a list to remember all aggregates to be flattened, this will be accessed like a FIFO queue
            List<AggregateException> exceptionsToFlatten = new List<AggregateException>();
            exceptionsToFlatten.Add(ae);
            int nDequeueIndex = 0;

            // Continue removing and recursively flattening exceptions, until there are no more.
            while (exceptionsToFlatten.Count > nDequeueIndex)
            {
                // dequeue one from exceptionsToFlatten
                IList<Exception> currentInnerExceptions = exceptionsToFlatten[nDequeueIndex++].InnerExceptions;

                for (int i = 0; i < currentInnerExceptions.Count; i++)
                {
                    Exception currentInnerException = currentInnerExceptions[i];

                    if (currentInnerException == null)
                    {
                        continue;
                    }
                    AggregateException currentInnerAsAggregate = currentInnerException as AggregateException;

                    // If this exception is an aggregate, keep it around for later.  Otherwise,
                    // simply add it to the list of flattened exceptions to be returned.
                    if (currentInnerAsAggregate != null)
                    {
                        exceptionsToFlatten.Add(currentInnerAsAggregate);
                    }
                    else
                    {
                        flattenedExceptions.Add(currentInnerException);
                        flattenedExceptions.AddRange(GetFlattenException(currentInnerException));

                    }
                }
            }
            return flattenedExceptions;
        }
        /// <summary>
        /// 解析Exception里的innerException,返回innerException
        /// </summary>
        /// <param name="se">Exception</param>
        /// <returns>inner Exception 列表</returns>
        private List<Exception> GetFlattenException(Exception se)
        {
            List<Exception> exs = new List<Exception>();

            Exception currentInnerException = se.InnerException;
            if (currentInnerException == null)
                return new List<Exception>();
            else
            {
                exs.Add(currentInnerException);
                exs.AddRange(GetFlattenException(currentInnerException));
            }
            return exs;
        }
        /// <summary>
        /// 解析复合Exception里的innerException,返回解析过的内部Exception，方便前台判断。
        /// </summary>
        /// <param name="ae">复合Exception</param>
        /// <returns> 解析过的内部Exception，方便前台判断。</returns>
        public Exception HandleException(AggregateException ae)
        {
            StringBuilder sb = new StringBuilder();
            LogUtil.HWLogger.API.Error(ae);
            List<Exception> flattenedExceptions = GetFlattenAggregateException(ae);
            foreach (var ex in flattenedExceptions)
            {
                if (ex is WebException)
                {
                    WebException we = (WebException)ex;
                    LogUtil.HWLogger.API.Error(we.Response);
                    if (we.Response != null)
                    {
                        HttpWebResponse response = (HttpWebResponse)we.Response;
                        StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                        string backstr = sr.ReadToEnd();
                        sr.Close();
                        response.Close();
                        LogUtil.HWLogger.API.Error(backstr);
                    }
                }
                sb.AppendLine(ex.Message);
            }
            LogUtil.HWLogger.API.Error(sb.ToString());
            int errCnt = flattenedExceptions.Count;
            for (int i = errCnt - 1; i >= 0; i--)
            {
                var ex = flattenedExceptions[i];
                if (ex is SocketException)//是否socket连接错误
                {
                    var sex = ex as SocketException;
                    if (sex.NativeErrorCode == 10061)//是否socket拒绝连接
                    {
                        throw new BaseException(ErrorCode.NET_SOCKET_REFUSED, ex.Message);
                    }
                    else
                    {
                        throw new BaseException(ErrorCode.NET_SOCKET_UNKNOWN, ex.Message);
                    }
                }
                else if (ex is AuthenticationException)
                {
                    throw new BaseException(ErrorCode.CERT_ERROR, ex.Message);
                }
                else if (ex is CryptographicException)
                {
                    throw new BaseException(ErrorCode.CERT_ERROR, ex.Message);
                }
                else if (ex is WebException)
                {
                    throw new BaseException(ErrorCode.SYS_UNKNOWN_ERR, ex.Message);
                }
            }
            throw new BaseException(ErrorCode.SYS_UNKNOWN_ERR, sb.ToString());
        }


        public bool IsCrtExist()
        {
            X509Store store1 = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store1.Open(OpenFlags.MaxAllowed);
            X509Certificate2Collection certs1 = store1.Certificates.Find(X509FindType.FindBySubjectName, "Huawei Equipment CA", false);
            if (certs1.Count == 0 || certs1[0].NotAfter < DateTime.Now)
            {
                throw new BaseException(ErrorCode.CERT_ROOT_NOTFOUND, "请先上传根证书");
            }
            X509Store store2 = new X509Store(StoreName.CertificateAuthority, StoreLocation.LocalMachine);
            store2.Open(OpenFlags.MaxAllowed);
            X509Certificate2Collection certs2 = store2.Certificates.Find(X509FindType.FindBySubjectName, "Huawei IT Product CA", false);
            if (certs2.Count == 0 || certs2[0].NotAfter < DateTime.Now)
            {
                throw new BaseException(ErrorCode.CERT_ROOT_NOTFOUND, "请先上传中间证书");
            }
            return true;

        }
    }
}
