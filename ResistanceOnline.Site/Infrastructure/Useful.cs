using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Humanizer;

namespace ResistanceOnline.Site.Infrastructure
{
    public static class Useful
    {
        public static string CommaQuibbling(IEnumerable<string> items, string conjunction = "and")
        {
            var itemArray = items.ToArray();

            var commaSeparated = String.Join(", ", itemArray, 0, Math.Max(itemArray.Length - 1, 0));
            if (commaSeparated.Length > 0) commaSeparated += " " + conjunction + " ";

            return commaSeparated + itemArray.LastOrDefault();
        }

        public static string RandomName()
        {
            List<string> names = new List<string>() {
                "Deuv","Evory","Smias","Saynt","Iundi","Llyld","Peyrr","Kalk","Pul","Ghap","Daur","Thuiq","Bam","Auntu","Garb","Brer","Chroib","Uchai","Croert","Nold","Chair","Tieck","Zeas","Teith","Aentha","Achd","Neq","Erodi","Endss","Rakt","Bayz","Kass","Naegh","Rakg","Ousta","Deind","Bleis","Kall","Ranh","Achph","Uquao","Eeste","Sern","Uwore","Loss","Thernn","Yut","Cheq","Onali","Iagea","Arotha","Emm","Oyera","Chass","Dad","Samk","Quer","Isgh","Brin","Danth","Thean","Weir","Tonll","Moerd","Dren","Schat","Inep","Risnd","Seen","Seum","Smof","Vorh","Enthr","Orm","Stroir","Keut","Dum","Oache","Tannd","Ildll","Iat","Adt","Etld","Iecha","Thaeph","Neiss","Aelda","Etld","Ycha","Ethero","Rould","Cert","Ealdo","Moock","Oare","School","Zen","Tonlt","Iunto","Sus","Tasv","Chald","Otaiu","Biat","Ceck","Yshye","Angn","Arode","Edane","Boend","Cerld","Dayq","Ooldi","Nuh","Tiac","Belnt","Creez","Ror","Shyz",
                "Coud","Mosl","Tiat","Ryns","Oemu","Lial","Zheyn","Zheyt","Iraky","Etana","Dyk","Syrd","Austy","Chrelt","Uquee","Undz","Hinp","Laef","Polph","Opere","Rooy","Haw","Aessi","Asm","Ardr","Awp","Ypole","Throock","Hoird","Vorm","Ehato","Oadi","Cruih","Aoso","Arw","Irynu","Eangy","Uvesa","Iaughi","Aldgh","Euske","Ocheu","Eaty","Lin","Oighto","Kelrd","Epola","Most","Iuska","Ightk","Awph","Tinr","Skelt","Sysh","Perm","Iangi","Irrd","Oiney","Giey","Uengi","Ymosu","Iti","Emnn","Nuiq","Iene","Sayss","Ird","Oacko","Igare","Loul","Ashz","Essq","Yeno","Elmsh","Vyd","Rois","Awl","Mosrt","Ealda","Reun","Idrai","Eleru","Louq","Eryni","Teb","Oldp","Osule","Bleid","Sneav","Aetu","Rank","Chieb","Epolo","Adb","Bloem","Ass","Iemo","Mosnt","Utoro","Mas","Endt","Neat","Vorw","Iunto","Dels","Ydeno","Theinn","Oacku","Risn","Yul","Unalu","Aentha","Tonm","Evesa","Aimy","Sheic","Kauw","Rilth","Rakc",
                "Fan","Ebane","Otine","Dom","Fow","Yeq","Iemi","Aundo","Smoic","Saynn","Ukima","Siel","Yada","Shyt","Asamo","Cias","Nuirt","Urise","Treph","Urnz","Lec","Mornn","Samsh","Ilery","Zhauc","Breym","Atu","Thris","Suln","Rayl","Tanl","Imd","Ashye","Ynysa","Ches","Garnd","Shaif","Noirr","Yhati","Eani","Hinv","Iquai","Ughay","Eardo","Tann","Ypolu","Undd","Pernt","Hint","Danh","Odari","Treet","Angh","Whooz","Udaru","Neal","Uloro","Iqueu","Honr","Iml","Undch","Kiq","Lord","Unye","Irayo","Wart","Errd","Aldt","Reych","Mosq","Omora","Irisy","Ziass","Smoeck","Etasa","Rann","Iene","Ries","Darm","Untd","Eoru","Ykino","Ihina","Ihini","Lorch","Ydare","Awory","Dort","Yand","Enk","Belt","Tasld","Neym","Atory","Tiann","Diey","Ems","Driaw","Teess","Atori","Swad","Smeph","Yeny","Nalp","Ulyei","Ukaly","Elmr","Neuz","Adw","Eengi","Bant","Aiso","Shaun","Smin","Warh","Ceth","Yhine","Kaent","Brooq",
                "Obury","Riad","Edelo","Aughch","Lub","Lieh","Vaug","Udaru","Uskeli","Lorsh","Ocheo","Iraka","Undld","Quis","Nald","Ashyo","Dann","Emd","Troir","Seur","Snin","Rodll","Iora","Paild","Ormy","Olora","Kac","Yskely","Chrob","Sak","Uarde","Dars","Enn","Yernn","Leart","Erili","Ort","Gel","Odelo","Oquai","Yturi","Adelo","Blit","Zhail","Endn","Iskeli","Eleri","Iare","Tanb","Kous","Hatn","Asulo","Threum","Oasa","Drik","Radk","Neunt","Bleult","Ormd","Elmq","Iveri","Oilde","Skeld","Etona","Soh","Rer","Echai","Onss","Othera","Ildy","Dys","Beak","Osnd","Radth","Dart","Uecho","Darrd","Smiaw","Aldp","Uworo","Athv","Elmnd","Seynt","Kais","Baurd","Undd","Atone","Yvese","Nayd","Eskela","Cryrt","Angsh","Cheel","Iuske","Dynm","Drar","Dyst","Otiny","Therh","Rour","Ykelo","Acku","Ynysi","Engrr","Broot","Creuc","Taip","Osulo","Eraki","Inet","Yemo","Sler","Dreest","Paym","Lauss","Cleird","Emose","Dens","Evere",
                "Throuy","Deigh","Ped","Polg","Tans","Rhann","Oany","Aosy","Echst","Rilv","Soss","Hins","Say","Zom","Payst","Tursh","Eacho","Eatu","Irana","Hinch","Dynn","Itn","Goz","Ildq","Undy","Mosm","Elmg","Seagh","Ienthu","Ani","Morgh","Dis","Dynll","Undf","Tait","Oeme","Kimst","Blaush","Eango","Rakck","Aughl","Strud","Wars","Rid","Iolde","Ahate","Bym","Tooc","Polt","Uvere","Roum","Eera","Adray","Awnn","Zhys","Gack","Choer","Dold","Snooq","Garnt","Sersh","Nalss","Hinth","Lerl","Ruich","Yachi","Ens","Ausky","Louck","Teilt","Enss","Asd","Cet","Soust","Arnn","Ard","Isst","Nant","Polph","Bul","Achee","Iormo","Leet","Chaiph","Ysamo","Shack","Cloult","Imy","Einae","Turn","Ightp","Snuiv","Ytai","Yanga","Estld","Enyse","Poln","Tor","Tiar","Ihina","Uade","Yranu","Chroerr","Threyph","Opolo","Ychau","Ony","Chep","Morl","Rosh","Reeld","Dood","Snoth","Yashy","Chack","Idyny","Otaso","Toilt","Arl"
            };

            return names.Distinct().OrderBy(x => Guid.NewGuid()).First();
        }
      
    }
}