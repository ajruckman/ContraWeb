using System;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Components;

namespace Web.Services
{
    public class LogActionService
    {
        public LogActionService(NavigationManager navigationManager)
        {
            Console.WriteLine("Constructed");
            NavigationManager = navigationManager;
        }

        public NavigationManager NavigationManager { get; set; }
        
        public Log TargetLog { get; private set; }

        public void Invoke(Log targetLog)
        {
            Console.WriteLine("Invoked: " + targetLog.Question);
            TargetLog = targetLog;
            NavigationManager.NavigateTo("/rules");
        }

        public Log GetAndUnset()
        {
            Log log = TargetLog;
            TargetLog = null;
            return log;
        }
    }
}