using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ShortDash.Server.Components
{
    public class NavMenuButton
    {
        public Action Callback { get; set; }
        public string Icon { get; set; }
    }
}
