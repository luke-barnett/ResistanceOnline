using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistanceOnline.Core
{
    [DebuggerDisplay("{Name} {Character}")]
    public class Player
    {
        public enum Type
        {
            Human,
            TrustBot
        }

        public string Name { get; set; }
        public Character Character { get; set; }
        public Guid Guid { get; set; }
        public Type PlayerType { get; set; }


		public Player()
		{

		}

		public Player(ResistanceOnline.Database.Entities.Player player)
		{
			Name = player.Name;
			Character = (Core.Character)Enum.Parse(typeof(Core.Character), player.Character);
			PlayerType = (Type)Enum.Parse(typeof(Type), player.Type);
			Guid = player.Guid;
		}
    }
}
