﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NBrightCore.FtpUtil
{
    class DefaultDateTimeProvider:IDateTimeProvider
    {
        DateTime IDateTimeProvider.GetCurrentDateTime() {
            return DateTime.Now;
        }
    }
}
