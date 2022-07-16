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
using xFusion.SCCMPlugin.Core;
using xFusion.SCCMPlugin.Core.Exceptions;
using xFusion.SCCMPlugin.Core.Model.Request;
using xFusion.SCCMPlugin.Core.Model.Response;
using xFusion.SCCMPlugin.Models;
using xFusion.SCCMPlugin.FusionDirector.PluginUI.Attributes;
using xFusion.SCCMPlugin.FusionDirector.PluginUI.Entitys;
using xFusion.SCCMPlugin.FusionDirector.PluginUI.Helper;
using xFusion.SCCMPlugin.FusionDirector.PluginUI.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xFusion.SCCMPlugin.FusionDirector.PluginUI.Handlers
{
    /// <summary>
    /// 服务器管理
    /// </summary>
    [CefURL("xfusion/server/list.html")]
    [Bound("NetBound")]
    public class ServerHandler : BaseHandler, IWebHandler
    {

    }
}
