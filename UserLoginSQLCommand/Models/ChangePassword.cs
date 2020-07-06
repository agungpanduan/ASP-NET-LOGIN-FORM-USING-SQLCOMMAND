using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UserLoginSQLCommand.Models
{
    public class ChangePassword
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "OTP is requierd")]
        public string OTP { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is requierd")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Need min 6 char")]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Confirm Password is requierd")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm Password should match with Password")]
        public string ConfirmPassword { get; set; }  
    }
}

// Copyright © 2020 Agung Anggara. All rights reserved.
// Purpose: To search all columns of all tables for a given search string
// Written by: Agung Anggara
// Site: https://www.agungpanduan.com
// Updated and tested by Tim Gaunt
// https://medium.com/@agungpanduan/asp-net-create-login-form-using-sql-command-7a59297edb3e
// Tested on: SQL Server 2012 & 2016, Visual Studio 2010