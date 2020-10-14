using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ShortDash.Server.Components
{
    public class NavMenuManager
    {
        private readonly List<NavMenuButton> navMenuButtons = new List<NavMenuButton>();
        public IEnumerable<NavMenuButton> MenuBarButtons => navMenuButtons;
        public string Subtitle { get; set; }

        public void AddMenuButton(string icon, Action callback)
        {
            navMenuButtons.Add(new NavMenuButton
            {
                Icon = icon,
                Callback = callback
            });
        }

        public void Reset()
        {
            navMenuButtons.Clear();
            Subtitle = "";
        }
    }
}
