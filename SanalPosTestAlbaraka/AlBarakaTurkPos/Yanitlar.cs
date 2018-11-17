using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlBarakaTurkPos
{
   public class Yanitlar
    {
        public class Cekim
        {
            public bool  Durum { get; set; }
            public string YanitKodu { get; set; }
            public string Yanitmesaji { get; set; }
            public string AlbarakaOnayKodu { get; set; }
            public string ProvizyonKodu { get; set; }
            public string TaksitSayisi { get; set; }
            public string TaksitTutari { get; set; }
            public string Puan { get; set; }
            public string Puantutari { get; set;}
            public string ToplamPuan { get; set; }
            public string ToplamPuanTutari { get; set; }
            public string IpAdres { get; set; }
        }
        public class JokerVada
        {
            public bool Durum { get; set; }
           public List<SanalPos.JokerVadaKampanya> Kampanya = new List<SanalPos.JokerVadaKampanya>();
            public string YanitKodu { get; set; }
            public string YanitMesaji { get; set; }
        }
    }
}
