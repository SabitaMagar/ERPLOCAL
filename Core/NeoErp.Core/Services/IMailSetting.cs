﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Core.Services
{
    public interface IMailSetting
    {
        SmtpClient CurrentMailSetting { get; set; }
    }
}
