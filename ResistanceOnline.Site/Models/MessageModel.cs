using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistanceOnline.Site.Models
{
    public class MessageModel
    {
        public string Player { get; set; }
        public string Message { get; set; }

        public MessageModel(string player, string message)
        {
            Player = player;
            Message = message;
        }
    }
}
