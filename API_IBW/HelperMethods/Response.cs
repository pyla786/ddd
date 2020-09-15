﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.HelperMethods
{
    public class Response<T>
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public T Data { get; set; }
    }
}