//**************************************************************************  
//Copyright (C) 2019 xFusion Digital Technologies Co., Ltd. All rights reserved.
//This program is free software; you can redistribute it and/or modify
//it under the terms of the MIT license.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//MIT license for more detail.
//*************************************************************************  
﻿using CommonUtil.ModelHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xFusion.SCCMPlugin.Models
{
    [Serializable]
    [DbTableName("FusionDirectors")]
    public class FusionDirectorModel : BaseModel
    {
        [JsonProperty(PropertyName = "hostIp")]
        [DbColumn("HOST_IP")]
        public String HostIP { get; set; }

        [JsonProperty(PropertyName = "port")]
        [DbColumn("PORT")]
        public int Port { get; set; }

        [JsonProperty(PropertyName = "aliasName")]
        [DbColumn("ALIAS_NAME")]
        public string AliasName { get; set; }

        [JsonProperty(PropertyName = "loginAccount")]
        [DbColumn("LOGIN_ACCOUNT")]
        public String LoginAccount { get; set; }

        [JsonProperty(PropertyName = "loginPwd")]
        [DbColumn("LOGIN_PWD")]
        public String LoginPwd { get; set; }

        [JsonIgnore]
        [DbColumn("CERT_PATH")]
        public String CertPath { get; set; }

        [JsonProperty(PropertyName = "latestStatus")]
        [DbColumn("LATEST_STATUS")]
        public String LatestStatus { get; set; }

        [JsonProperty(PropertyName = "reservedInt1")]
        [DbColumn("RESERVED_INT1")]
        public int ReservedInt1 { get; set; }

        [JsonProperty(PropertyName = "reservedInt2")]
        [DbColumn("RESERVED_INT2")]
        public int ReservedInt2 { get; set; }

        [JsonProperty(PropertyName = "reservedStr1")]
        [DbColumn("RESERVED_STR1")]
        public string ReservedStr1 { get; set; }

        [JsonProperty(PropertyName = "reservedStr2")]
        [DbColumn("RESERVED_STR2")]
        public string ReservedStr2 { get; set; }
        [JsonConverter(typeof(CustomJsonDateTimeConverter))]
        [JsonProperty(PropertyName = "lastModify")]
        [DbColumn("LAST_MODIFY_TIME")]
        public DateTime LastModifyTime { get; set; }
        [JsonConverter(typeof(CustomJsonDateTimeConverter))]
        [JsonProperty(PropertyName = "createTime")]
        [DbColumn("CREATE_TIME")]
        public DateTime CreateTime { get; set; }
    }
}
