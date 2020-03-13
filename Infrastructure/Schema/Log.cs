using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Database.ContraDB;

namespace Infrastructure.Schema
{
    public class Log
    {
        public long ID { get; set; }

        public DateTime Time { get; set; }

        public IPAddress Client { get; set; }

        public string Question { get; set; }

        public string QuestionType { get; set; }

        public string Action { get; set; }

        public List<string> Answers { get; set; }

        public string ClientHostname { get; set; }

        public PhysicalAddress ClientMAC { get; set; }

        public string ClientVendor { get; set; }

        public Log() { }

        public Log(log log)
        {
            Time           = log.time;
            Client         = log.client;
            Question       = log.question;
            QuestionType   = log.question_type;
            Action         = log.action;
            Answers        = log.answers?.ToList() ?? new List<string>();
            ClientHostname = log.client_hostname;
            ClientMAC      = log.client_mac;
            ClientVendor   = log.client_vendor;
        }

        public override string ToString()
        {
            return $"{{{Client} | {ID} -> [{QuestionType}] {Question}}}";
        }
    }
}