//**************************************************************************  
//Copyright (C) 2019 xFusion Digital Technologies Co., Ltd. All rights reserved.
//This program is free software; you can redistribute it and/or modify
//it under the terms of the MIT license.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//MIT license for more detail.
//*************************************************************************  
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using xFusion.SCCMPlugin.FusionDirector.PluginUI.Views;

namespace UnitTestSampleOnUI
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        //eSight配置
        private void button1_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("xfusion/FDConfig/index.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //服务器列表
        private void button2_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("xfusion/server/list.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //关于
        private void button3_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("xfusion/about/about.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //OS镜像管理
        private void button4_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("xfusion/osImage/index.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //OS部署管理
        private void button5_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("xfusion/osDeploy/index.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //任务管理
        private void button6_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("xfusion/task/index.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //配置管理
        private void button7_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("xfusion/configuration/index.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        //
        private void button8_Click(object sender, EventArgs e)
        {
            HostTabsViewFrm frm = new HostTabsViewFrm("xfusion/upgradePackageWarehouse/index.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

       
        private void button11_Click(object sender, EventArgs e)
        {

            HostTabsViewFrm frm = new HostTabsViewFrm("xfusion/deviceVersionStatus/index.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }

        private void button10_Click(object sender, EventArgs e)
        {

            HostTabsViewFrm frm = new HostTabsViewFrm("xfusion/upgradePlan/index.html");
            frm.WindowState = FormWindowState.Maximized;
            frm.Show();
        }
    }
}
