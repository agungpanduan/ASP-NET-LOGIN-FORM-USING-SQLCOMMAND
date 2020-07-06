using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UserLoginSQLCommand.Models
{
    public class ForgetPassword
    {
        [Display(Name = "User Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "User Email Id Required")]
        public string EmailId { get; set; }
    }
}
// Copyright © 2020 Agung Anggara. All rights reserved.
// Purpose: To search all columns of all tables for a given search string
// Written by: Agung Anggara
// Site: https://www.agungpanduan.com
// Updated and tested by Tim Gaunt
// https://medium.com/@agungpanduan/asp-net-create-login-form-using-sql-command-7a59297edb3e
// Tested on: SQL Server 2012 & 2016, Visual Studio 2010