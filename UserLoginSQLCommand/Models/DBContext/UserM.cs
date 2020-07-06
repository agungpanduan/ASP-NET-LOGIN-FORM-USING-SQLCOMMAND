using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserLoginSQLCommand.Models.DBContext
{
    public class UserM
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Nullable<bool> EmailVerification { get; set; }
        public Nullable<System.Guid> ActivetionCode { get; set; }
        public string OTP { get; set; }
    }
}

// Copyright © 2020 Agung Anggara. All rights reserved.
// Purpose: To search all columns of all tables for a given search string
// Written by: Agung Anggara
// Site: https://www.agungpanduan.com
// Updated and tested by Tim Gaunt
// https://medium.com/@agungpanduan/asp-net-create-login-form-using-sql-command-7a59297edb3e
// Tested on: SQL Server 2012 & 2016, Visual Studio 2010