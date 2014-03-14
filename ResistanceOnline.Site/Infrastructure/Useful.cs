using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Humanizer;

namespace ResistanceOnline.Site.Infrastructure
{
    public static class Useful
    {
        public static string CommaQuibbling(IEnumerable<string> items)
        {
            var itemArray = items.ToArray();

            var commaSeparated = String.Join(", ", itemArray, 0, Math.Max(itemArray.Length - 1, 0));
            if (commaSeparated.Length > 0) commaSeparated += " and ";

            return commaSeparated + itemArray.LastOrDefault();
        }

        public static string RandomName()
        {
            List<string> names = new List<string>() {
                "Deuv","Evory","Smias","Saynt","Iundi","Llyld","Peyrr","Kalk","Pul","Ghap","Daur","Thuiq","Bam","Auntu","Garb","Brer","Chroib","Uchai","Croert","Nold","Chair","Tieck","Zeas","Teith","Aentha","Achd","Neq","Erodi","Endss","Rakt","Bayz","Kass","Naegh","Rakg","Ousta","Deind","Bleis","Kall","Ranh","Achph","Uquao","Eeste","Sern","Uwore","Loss","Thernn","Yut","Cheq","Onali","Iagea","Arotha","Emm","Oyera","Chass","Dad","Samk","Quer","Isgh","Brin","Danth","Thean","Weir","Tonll","Moerd","Dren","Schat","Inep","Risnd","Seen","Seum","Smof","Vorh","Enthr","Orm","Stroir","Keut","Dum","Oache","Tannd","Ildll","Iat","Adt","Etld","Iecha","Thaeph","Neiss","Aelda","Etld","Ycha","Ethero","Rould","Cert","Ealdo","Moock","Oare","School","Zen","Tonlt","Iunto","Sus","Tasv","Chald","Otaiu","Biat","Ceck","Yshye","Angn","Arode","Edane","Boend","Cerld","Dayq","Ooldi","Nuh","Tiac","Belnt","Creez","Ror","Shyz",
                "Coud","Mosl","Tiat","Ryns","Oemu","Lial","Zheyn","Zheyt","Iraky","Etana","Dyk","Syrd","Austy","Chrelt","Uquee","Undz","Hinp","Laef","Polph","Opere","Rooy","Haw","Aessi","Asm","Ardr","Awp","Ypole","Throock","Hoird","Vorm","Ehato","Oadi","Cruih","Aoso","Arw","Irynu","Eangy","Uvesa","Iaughi","Aldgh","Euske","Ocheu","Eaty","Lin","Oighto","Kelrd","Epola","Most","Iuska","Ightk","Awph","Tinr","Skelt","Sysh","Perm","Iangi","Irrd","Oiney","Giey","Uengi","Ymosu","Iti","Emnn","Nuiq","Iene","Sayss","Ird","Oacko","Igare","Loul","Ashz","Essq","Yeno","Elmsh","Vyd","Rois","Awl","Mosrt","Ealda","Reun","Idrai","Eleru","Louq","Eryni","Teb","Oldp","Osule","Bleid","Sneav","Aetu","Rank","Chieb","Epolo","Adb","Bloem","Ass","Iemo","Mosnt","Utoro","Mas","Endt","Neat","Vorw","Iunto","Dels","Ydeno","Theinn","Oacku","Risn","Yul","Unalu","Aentha","Tonm","Evesa","Aimy","Sheic","Kauw","Rilth","Rakc"
            };

            return names.OrderBy(x => Guid.NewGuid()).First();
        }
      
    }
}