﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nanophone.Core
{
    public interface IManageServiceInstances
    {
        Task RegisterServiceAsync(string serviceName, string version, Uri uri, Uri healthCheckUri = null, IEnumerable<string> tags = null);
        Task DeregisterServiceAsync(string serviceId);
    }
}
