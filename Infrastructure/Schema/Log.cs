using System;
using System.Collections.Generic;
using System.Linq;
using Database.ContraCoreDB;

namespace Infrastructure.Schema
{
    public class Log
    {
        public long ID { get; set; } = 0;

        public DateTime Time { get; set; } = DateTime.MinValue;

        public string Client { get; set; } = "";

        public string Question { get; set; } = "";

        public string QuestionType { get; set; } = "";

        public string Action { get; set; } = "";

        public List<string> Answers { get; set; }

        public string ClientHostname { get; set; } = "";

        public string ClientMAC { get; set; } = "";

        public string ClientVendor { get; set; } = "";

        public dynamic ActionButton { get; set; }

        #pragma warning disable 8618
        public Log() { }
        #pragma warning restore 8618

        // public Log(log log)
        // {
        //     Time           = log.time;
        //     Client         = log.client.ToString();
        //     Question       = log.question;
        //     QuestionType   = log.question_type;
        //     Action         = log.action;
        //     Answers        = log.answers?.ToList() ?? new List<string>();
        //     ClientHostname = log.client_hostname;
        //     ClientMAC      = log.client_mac?.ToString();
        //     ClientVendor   = log.client_vendor;
        // }

        public override string ToString()
        {
            return $"{{{Client} | {ID} -> [{QuestionType}] {Question}}}";
        }
    }
}