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
        
        public Log? TargetLog { get; private set; }

        public void Invoke(Log targetLog)
        {
            Console.Write("Invoked: " + targetLog.Question);
            TargetLog = targetLog;

            if (targetLog.Action.StartsWith("pass."))
            {
                Console.WriteLine(" -> blacklist");
                NavigationManager.NavigateTo("/blacklist");
            }
            else if (targetLog.Action.StartsWith("block."))
            {
                Console.WriteLine(" -> whitelist");
                NavigationManager.NavigateTo("/rules");
            }
        }

        public Log? GetAndUnset()
        {
            Log log = TargetLog!;
            TargetLog = null;
            return log;
        }
    }
}