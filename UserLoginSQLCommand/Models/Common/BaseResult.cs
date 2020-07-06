using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserLoginSQLCommand.Models.Common
{
    public class BaseResult
    {
        public const string VALUE_SUCCESS = "SUCCESS";
        public const string VALUE_ERROR = "ERROR";

        public string Result { set; get; }
        public string ProcessId { set; get; }
        public string[] SuccMesgs { set; get; }
        public string[] ErrMesgs { set; get; }
        public object[] Params { set; get; }
        public object[] Params2 { set; get; }
        public string RedirectURL { set; get; }
    }
}
// Copyright © 2020 Agung Anggara. All rights reserved.
// Purpose: To search all columns of all tables for a given search string
// Written by: Agung Anggara
// Site: https://www.agungpanduan.com
// Updated and tested by Tim Gaunt
// https://medium.com/@agungpanduan/asp-net-create-login-form-using-sql-command-7a59297edb3e
// Tested on: SQL Server 2012 & 2016, Visual Studio 2010