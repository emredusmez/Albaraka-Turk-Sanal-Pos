using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AlBarakaTurkPos
{
   public static class ParaBirimi
    {
        public static string TL = "YT";
        public static string EURO = "EU";
        public static string DOLAR = "US";
    }
    public static class IptalIslemTipi
    {
        public static string Satis = "sale";
        public static string Provizyon = "auth";
        public static string Finanssallastirma= "capt";
        public static string PuanKullanimi = "pointUsage";
        public static string VftIslemi = "vftTransaction";
        public static string IadeIslemi = "return";
    }
   
    public class SanalPos
    {

        public class JokerVadaKampanya
        {
            public  string KampanyaKodu { get; set; }
            public  string KampanyaAciklama { get; set; }

        }
        string VeriYolla(string xml)
        {
            string webpageContent = string.Empty;

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes("xmldata=<?xml version=\"1.0\" encoding=\"ISO-8859-9\"?>" + xml);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://epostest.albarakaturk.com.tr/EPosWebService/XML");
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = byteArray.Length;

                using (Stream webpageStream = webRequest.GetRequestStream())
                {
                    webpageStream.Write(byteArray, 0, byteArray.Length);
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.GetEncoding("windows-1254")))
                    {
                       
                        webpageContent = reader.ReadToEnd();
                        return webpageContent;
                    }
                }
            }
            catch (Exception ex)
            {

                webpageContent = ex.Message;
                return webpageContent;
            }
        }
       public SanalPos(string mid,string tid)
        {
            Mid = mid;
            Tid = tid;

        }
        string Mid, Tid;
        /// <summary>
        /// Taksitli veya tek çekim
        /// </summary>
        /// <param name="Tutar">Ödeme tutarı </param>
        /// <param name="KartNo">Çekim yapılacak kredi kartı numarası</param>
        /// <param name="ParaBirimi">Çekim yapılacak para birimi (YT,US,EU)</param>
        /// <param name="CvcNo">Kredi kartı arkasındaki 3 haneli güvenlik numarası</param>
        /// <param name="SonKullanmaTarihi">Kredi kartı son kullanma tarihi (4 haneli olarak girilecek.)</param>
        /// <param name="TaksitSayisi">Yapılacak taksit sayısı(Taksit olmayacak ise değer 0 girilmeli)</param>
        /// <param name="JokerKodu">Bonus v.s  gibi joker kullanımı yapılacak ise joker kodu</param>
        /// <param name="SiparisId">Benzersiz sipariş Id (Bu Id numarası benzersiz olarak üretilmelidir.)(24 haneli olması gerekmekte)</param>
        /// <param name="Mesaj">İşlem hakkında üye işyerinin gönderdiği açıklama bilgisidir. İlgili alan max.24 karakter alfa- nümerik olup Tükçe karakter desteklenmemektedir.</param>
        public Yanitlar.Cekim Satis(string Tutar,string KartNo,string ParaBirimi,string CvcNo,string SonKullanmaTarihi,int TaksitSayisi,string JokerKodu, string SiparisId,string Mesaj)
        {
           
                if (Mid.Length != 10)
                {
                    throw new Exception("Mid değeri 10 haneli sayıdan oluşmalı.");
                }
           
              
                if (Tid.Length != 8)
                {
                    throw new Exception("Tid değeri 8 haneli sayıdan oluşmalı.");
                }
           
           
            if (SiparisId.Length!=24)
            {
                throw new Exception("Sipariş Id 24 haneli rakam ve harf karışımından oluşan benzersiz bir değer olmalı");
            }

            
            if (KartNo.Length>19 || KartNo.Length<16)
            {
                throw new Exception("Kart numarası  maksimum 19 haneli sayıdan oluşmalı");
            }
            if (CvcNo.Length!=3)
            {
                throw new Exception("Cvc no 3 haneli sayıdan oluşmalı.");
            }
            if (SonKullanmaTarihi.Length!=4)
            {
                throw new Exception("Son kullanma tarihi 4 haneli sayıdan oluşmalı.");
            }
            if (Mesaj.Length>24)
            {
                throw new Exception("Mesaj maksimum 24 haneli olmalı");
            }
           
            
            StringBuilder sb = new StringBuilder();
            sb.Append("<posnetRequest>");
            sb.Append("<mid>"+Mid+"</mid>");
            sb.Append("<tid>"+Tid+"</tid>");
            sb.Append("<sale>");
            sb.Append("<amount>"+Tutar+"</amount>");
            sb.Append("<ccno>"+KartNo+"</ccno>");
            sb.Append("<currencyCode>"+ParaBirimi+"</currencyCode>");
            sb.Append("<cvc>"+CvcNo+"</cvc>");
            sb.Append("<expDate>"+SonKullanmaTarihi+"</expDate>");
            if (JokerKodu!="")
            {
                sb.Append("<installment>"+JokerKodu+"</installment>");
            }
            if (TaksitSayisi!=0)
            {
                sb.Append("<koiCode>"+TaksitSayisi.ToString()+"</koiCode>");
            }
            
            sb.Append("<orderID>"+SiparisId+"</orderID>");
            sb.Append("<specialMessage>"+Mesaj+"</specialMessage>");
            sb.Append("</sale>");
            sb.Append("</posnetRequest>");
            string yanit = VeriYolla(sb.ToString());
            XmlTextReader okuyucu = new XmlTextReader(new System.IO.StringReader(yanit));
            Yanitlar.Cekim Yanit = new Yanitlar.Cekim();
            while (okuyucu.Read())
            {
                if (okuyucu.Name == "approved")
                {
                    if (okuyucu.ReadString()=="001")
                    {
                        Yanit.Durum = true;
                    }
                    else
                    {
                        Yanit.Durum = false;
                    }
                   
                }
                else if (okuyucu.Name== "hostlogkey")
                {
                    Yanit.AlbarakaOnayKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name== "authCode")
                {
                    Yanit.ProvizyonKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name== "inst1")
                {
                    Yanit.TaksitSayisi = okuyucu.ReadString();
                }
                else if (okuyucu.Name== "amnt1")
                {
                    Yanit.TaksitTutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name== "point")
                {
                    Yanit.Puan = okuyucu.ReadString();
                }
                else if (okuyucu.Name== "pointAmount")
                {
                    Yanit.Puantutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name== "totalPoint")
                {
                    Yanit.ToplamPuan = okuyucu.ReadString();
                }
                else if (okuyucu.Name== "totalPointAmount")
                {
                    Yanit.ToplamPuanTutari = okuyucu.ReadString();
                }
            }
            return Yanit;
        }

        /// <summary>
        /// Taksitli veya tek çekim
        /// </summary>
        /// <param name="Tutar">Ödeme tutarı </param>
        /// <param name="KartNo">Çekim yapılacak kredi kartı numarası</param>
        /// <param name="ParaBirimi">Çekim yapılacak para birimi (YT,US,EU)</param>
        /// <param name="CvcNo">Kredi kartı arkasındaki 3 haneli güvenlik numarası</param>
        /// <param name="SonKullanmaTarihi">Kredi kartı son kullanma tarihi (4 haneli olarak girilecek.)</param>
        /// <param name="TaksitSayisi">Yapılacak taksit sayısı(Taksit olmayacak ise değer 0 girilmeli)</param>
        /// <param name="JokerKodu">Bonus v.s  gibi joker kullanımı yapılacak ise joker kodu</param>
        /// <param name="SiparisId">Benzersiz sipariş Id (Bu Id numarası benzersiz olarak üretilmelidir.)(24 haneli olması gerekmekte)</param>
        /// <param name="Mesaj">İşlem hakkında üye işyerinin gönderdiği açıklama bilgisidir. İlgili alan max.24 karakter alfa- nümerik olup Tükçe karakter desteklenmemektedir.</param>
        public Yanitlar.Cekim Provizyon(string Tutar, string KartNo, string ParaBirimi, string CvcNo, string SonKullanmaTarihi, int TaksitSayisi, string JokerKodu, string SiparisId, string Mesaj)
        {

            if (Mid.Length != 10)
            {
                throw new Exception("Mid değeri 10 haneli sayıdan oluşmalı.");
            }


            if (Tid.Length != 8)
            {
                throw new Exception("Tid değeri 8 haneli sayıdan oluşmalı.");
            }


            if (SiparisId.Length != 24)
            {
                throw new Exception("Sipariş Id 24 haneli rakam ve harf karışımından oluşan benzersiz bir değer olmalı");
            }

            
            if (KartNo.Length > 19 || KartNo.Length < 16)
            {
                throw new Exception("Kart numarası  maksimum 19 haneli sayıdan oluşmalı");
            }
            if (CvcNo.Length != 3)
            {
                throw new Exception("Cvc no 3 haneli sayıdan oluşmalı.");
            }
            if (SonKullanmaTarihi.Length != 4)
            {
                throw new Exception("Son kullanma tarihi 4 haneli sayıdan oluşmalı.");
            }
            if (Mesaj.Length > 24)
            {
                throw new Exception("Mesaj maksimum 24 haneli olmalı");
            }

            
            StringBuilder sb = new StringBuilder();
            sb.Append("<posnetRequest>");
            sb.Append("<mid>" + Mid + "</mid>");
            sb.Append("<tid>" + Tid + "</tid>");
            sb.Append("<auth>");
            sb.Append("<amount>" + Tutar + "</amount>");
            sb.Append("<ccno>" + KartNo + "</ccno>");
            sb.Append("<currencyCode>" + ParaBirimi + "</currencyCode>");
            sb.Append("<cvc>" + CvcNo + "</cvc>");
            sb.Append("<expDate>" + SonKullanmaTarihi + "</expDate>");
            if (JokerKodu != "")
            {
                sb.Append("<installment>" + JokerKodu + "</installment>");
            }
            if (TaksitSayisi != 0)
            {
                sb.Append("<koiCode>" + TaksitSayisi.ToString() + "</koiCode>");
            }

            sb.Append("<orderID>" + SiparisId + "</orderID>");
            sb.Append("<specialMessage>" + Mesaj + "</specialMessage>");
            sb.Append("</auth>");
            sb.Append("</posnetRequest>");
            string yanit = VeriYolla(sb.ToString());
            XmlTextReader okuyucu = new XmlTextReader(new System.IO.StringReader(yanit));
            Yanitlar.Cekim Yanit = new Yanitlar.Cekim();
            while (okuyucu.Read())
            {
                if (okuyucu.Name == "approved")
                {
                    if (okuyucu.ReadString() == "001")
                    {
                        Yanit.Durum = true;
                    }
                    else
                    {
                        Yanit.Durum = false;
                    }

                }
                else if (okuyucu.Name == "hostlogkey")
                {
                    Yanit.AlbarakaOnayKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "authCode")
                {
                    Yanit.ProvizyonKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "inst1")
                {
                    Yanit.TaksitSayisi = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "amnt1")
                {
                    Yanit.TaksitTutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "point")
                {
                    Yanit.Puan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "pointAmount")
                {
                    Yanit.Puantutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPoint")
                {
                    Yanit.ToplamPuan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPointAmount")
                {
                    Yanit.ToplamPuanTutari = okuyucu.ReadString();
                }
                if (okuyucu.Name== "respText")
                {
                    Yanit.Yanitmesaji = okuyucu.ReadString();
                }
            }
            return Yanit;
        }

        public Yanitlar.Cekim Finanssallastirma(string Tutar, string AlbarakaOnayKodu,string ParaBirimi)
        {

            
           
            StringBuilder sb = new StringBuilder();
            sb.Append("<posnetRequest>");
            sb.Append("<mid>" + Mid + "</mid>");
            sb.Append("<tid>" + Tid + "</tid>");
            sb.Append("<capt>");
            sb.Append("<amount>" + Tutar + "</amount>");
            sb.Append("<currencyCode>" + ParaBirimi + "</currencyCode>");
            sb.Append("<hostLogKey>" + AlbarakaOnayKodu + "</hostLogKey>");
            sb.Append("</capt>");
            sb.Append("</posnetRequest>");
            string yanit = VeriYolla(sb.ToString());
            XmlTextReader okuyucu = new XmlTextReader(new System.IO.StringReader(yanit));
            Yanitlar.Cekim Yanit = new Yanitlar.Cekim();
            while (okuyucu.Read())
            {
                if (okuyucu.Name == "approved")
                {
                    if (okuyucu.ReadString() == "001")
                    {
                        Yanit.Durum = true;
                    }
                    else
                    {
                        Yanit.Durum = false;
                    }

                }
                else if (okuyucu.Name == "hostlogkey")
                {
                    Yanit.AlbarakaOnayKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "authCode")
                {
                    Yanit.ProvizyonKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "inst1")
                {
                    Yanit.TaksitSayisi = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "amnt1")
                {
                    Yanit.TaksitTutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "point")
                {
                    Yanit.Puan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "pointAmount")
                {
                    Yanit.Puantutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPoint")
                {
                    Yanit.ToplamPuan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPointAmount")
                {
                    Yanit.ToplamPuanTutari = okuyucu.ReadString();
                }
                if (okuyucu.Name == "respText")
                {
                    Yanit.Yanitmesaji = okuyucu.ReadString();
                }
            }
            return Yanit;
        }


        public Yanitlar.Cekim Iptal( string AlbarakaOnayKodu, string SiparisId,string IslemTipi)
        {


           
            StringBuilder sb = new StringBuilder();
            sb.Append("<posnetRequest>");
            sb.Append("<mid>" + Mid + "</mid>");
            sb.Append("<tid>" + Tid + "</tid>");
            sb.Append("<reverse>");
            sb.Append("<transaction>" + IslemTipi + "</transaction>");
            sb.Append("<hostLogKey>" + AlbarakaOnayKodu + "</hostLogKey>");
           // sb.Append("<authCode>" + AlbarakaOnayKodu + "</authCode>");
            sb.Append("</reverse>");
            sb.Append("</posnetRequest>");
            string yanit = VeriYolla(sb.ToString());
            XmlTextReader okuyucu = new XmlTextReader(new System.IO.StringReader(yanit));
            Yanitlar.Cekim Yanit = new Yanitlar.Cekim();
            while (okuyucu.Read())
            {
                if (okuyucu.Name == "approved")
                {
                    if (okuyucu.ReadString() == "001")
                    {
                        Yanit.Durum = true;
                    }
                    else
                    {
                        Yanit.Durum = false;
                    }

                }
                else if (okuyucu.Name == "hostlogkey")
                {
                    Yanit.AlbarakaOnayKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "authCode")
                {
                    Yanit.ProvizyonKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "inst1")
                {
                    Yanit.TaksitSayisi = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "amnt1")
                {
                    Yanit.TaksitTutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "point")
                {
                    Yanit.Puan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "pointAmount")
                {
                    Yanit.Puantutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPoint")
                {
                    Yanit.ToplamPuan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPointAmount")
                {
                    Yanit.ToplamPuanTutari = okuyucu.ReadString();
                }
                if (okuyucu.Name == "respText")
                {
                    Yanit.Yanitmesaji = okuyucu.ReadString();
                }
            }
            return Yanit;
        }

        public Yanitlar.Cekim Iade(string AlbarakaOnayKodu, string SiparisId,string Tutar, string ParaBirimi)
        {


           
            StringBuilder sb = new StringBuilder();
            sb.Append("<posnetRequest>");
            sb.Append("<mid>" + Mid + "</mid>");
            sb.Append("<tid>" + Tid + "</tid>");
            sb.Append("<return>");
            sb.Append("<amount>" + Tutar + "</amount>");
            sb.Append("<hostLogKey>" + AlbarakaOnayKodu + "</hostLogKey>");
            sb.Append("<currencyCode>" + ParaBirimi+ "</currencyCode>");
            sb.Append("</return>");
            sb.Append("</posnetRequest>");
            string yanit = VeriYolla(sb.ToString());
            XmlTextReader okuyucu = new XmlTextReader(new System.IO.StringReader(yanit));
            Yanitlar.Cekim Yanit = new Yanitlar.Cekim();
            while (okuyucu.Read())
            {
                if (okuyucu.Name == "approved")
                {
                    if (okuyucu.ReadString() == "001")
                    {
                        Yanit.Durum = true;
                    }
                    else
                    {
                        Yanit.Durum = false;
                    }

                }
                else if (okuyucu.Name == "hostlogkey")
                {
                    Yanit.AlbarakaOnayKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "authCode")
                {
                    Yanit.ProvizyonKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "inst1")
                {
                    Yanit.TaksitSayisi = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "amnt1")
                {
                    Yanit.TaksitTutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "point")
                {
                    Yanit.Puan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "pointAmount")
                {
                    Yanit.Puantutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPoint")
                {
                    Yanit.ToplamPuan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPointAmount")
                {
                    Yanit.ToplamPuanTutari = okuyucu.ReadString();
                }
                if (okuyucu.Name == "respText")
                {
                    Yanit.Yanitmesaji = okuyucu.ReadString();
                }
            }
            return Yanit;
        }

        public Yanitlar.Cekim WorldPuanSorgula(string KartNo, string SonKullanmaTarihi)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<posnetRequest>");
            sb.Append("<mid>" + Mid + "</mid>");
            sb.Append("<tid>" + Tid + "</tid>");
            sb.Append("<pointInquiry>");
            sb.Append("<ccno>" + KartNo + "</ccno>");
           
            sb.Append("<expDate>" + SonKullanmaTarihi + "</expDate>");
            sb.Append("</pointInquiry>");
            sb.Append("</posnetRequest>");
            string yanit = VeriYolla(sb.ToString());
            XmlTextReader okuyucu = new XmlTextReader(new System.IO.StringReader(yanit));
            Yanitlar.Cekim Yanit = new Yanitlar.Cekim();
            while (okuyucu.Read())
            {
                if (okuyucu.Name == "approved")
                {
                    if (okuyucu.ReadString() == "001")
                    {
                        Yanit.Durum = true;
                    }
                    else
                    {
                        Yanit.Durum = false;
                    }

                }
                else if (okuyucu.Name == "hostlogkey")
                {
                    Yanit.AlbarakaOnayKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "authCode")
                {
                    Yanit.ProvizyonKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "inst1")
                {
                    Yanit.TaksitSayisi = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "amnt1")
                {
                    Yanit.TaksitTutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "point")
                {
                    Yanit.Puan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "pointAmount")
                {
                    Yanit.Puantutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPoint")
                {
                    Yanit.ToplamPuan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPointAmount")
                {
                    Yanit.ToplamPuanTutari = okuyucu.ReadString();
                }
                if (okuyucu.Name == "respText")
                {
                    Yanit.Yanitmesaji = okuyucu.ReadString();
                }
            }
            return Yanit;
        }

        public Yanitlar.Cekim WorldPuanKullan(string KartNo, string SonKullanmaTarihi,string Tutar,string SiparisId,string ParaBirimi)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<posnetRequest>");
            sb.Append("<mid>" + Mid + "</mid>");
            sb.Append("<tid>" + Tid + "</tid>");
            sb.Append("<pointUsage>");
            sb.Append("<amount>" + Tutar + "</amount>");
            sb.Append("<ccno>" + KartNo + "</ccno>");
            sb.Append("<currencyCode>" + ParaBirimi + "</currencyCode>");
           
            sb.Append("<expDate>" + SonKullanmaTarihi + "</expDate>");
            sb.Append("<orderID>" + SiparisId + "</orderID>");
            sb.Append("</pointUsage>");
            sb.Append("</posnetRequest>");
            string yanit = VeriYolla(sb.ToString());
            XmlTextReader okuyucu = new XmlTextReader(new System.IO.StringReader(yanit));
            Yanitlar.Cekim Yanit = new Yanitlar.Cekim();
            while (okuyucu.Read())
            {
                if (okuyucu.Name == "approved")
                {
                    if (okuyucu.ReadString() == "001")
                    {
                        Yanit.Durum = true;
                    }
                    else
                    {
                        Yanit.Durum = false;
                    }

                }
                else if (okuyucu.Name == "hostlogkey")
                {
                    Yanit.AlbarakaOnayKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "authCode")
                {
                    Yanit.ProvizyonKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "inst1")
                {
                    Yanit.TaksitSayisi = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "amnt1")
                {
                    Yanit.TaksitTutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "point")
                {
                    Yanit.Puan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "pointAmount")
                {
                    Yanit.Puantutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPoint")
                {
                    Yanit.ToplamPuan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPointAmount")
                {
                    Yanit.ToplamPuanTutari = okuyucu.ReadString();
                }
                if (okuyucu.Name == "respText")
                {
                    Yanit.Yanitmesaji = okuyucu.ReadString();
                }
            }
            return Yanit;
        }
        public Yanitlar.Cekim WorldPuanKullanimIade( string Tutar,  string ParaBirimi,string AlbarakaOnayKodu)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<posnetRequest>");
            sb.Append("<mid>" + Mid + "</mid>");
            sb.Append("<tid>" + Tid + "</tid>");
            sb.Append("<pointReturn>");
            sb.Append("<amount>" + Tutar + "</amount>");
          
            sb.Append("<currencyCode>" + ParaBirimi + "</currencyCode>");

            sb.Append("<hostLogKey>" + AlbarakaOnayKodu + "</hostLogKey>");
           
            sb.Append("</pointReturn>");
            sb.Append("</posnetRequest>");
            string yanit = VeriYolla(sb.ToString());
            XmlTextReader okuyucu = new XmlTextReader(new System.IO.StringReader(yanit));
            Yanitlar.Cekim Yanit = new Yanitlar.Cekim();
            while (okuyucu.Read())
            {
                if (okuyucu.Name == "approved")
                {
                    if (okuyucu.ReadString() == "001")
                    {
                        Yanit.Durum = true;
                    }
                    else
                    {
                        Yanit.Durum = false;
                    }

                }
                else if (okuyucu.Name == "hostlogkey")
                {
                    Yanit.AlbarakaOnayKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "authCode")
                {
                    Yanit.ProvizyonKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "inst1")
                {
                    Yanit.TaksitSayisi = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "amnt1")
                {
                    Yanit.TaksitTutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "point")
                {
                    Yanit.Puan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "pointAmount")
                {
                    Yanit.Puantutari = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPoint")
                {
                    Yanit.ToplamPuan = okuyucu.ReadString();
                }
                else if (okuyucu.Name == "totalPointAmount")
                {
                    Yanit.ToplamPuanTutari = okuyucu.ReadString();
                }
                if (okuyucu.Name == "respText")
                {
                    Yanit.Yanitmesaji = okuyucu.ReadString();
                }
            }
            return Yanit;
        }
        public Yanitlar.Cekim JokerVadaSorgula(string KartNo)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<posnetRequest>");
            sb.Append("<mid>" + Mid + "</mid>");
            sb.Append("<tid>" + Tid + "</tid>");
            sb.Append("<koiCampaignQuery>");
            sb.Append("<ccno>" + KartNo + "</ccno>"); sb.Append("</koiCampaignQuery>");
            sb.Append("</posnetRequest>");
            string yanit = VeriYolla(sb.ToString());
            XmlTextReader okuyucu = new XmlTextReader(new System.IO.StringReader(yanit));
            Yanitlar.JokerVada Yanit = new Yanitlar.JokerVada();
            
            while (okuyucu.Read())
            {
                JokerVadaKampanya jokervada = new JokerVadaKampanya();

                if (okuyucu.Name == "approved")
                {
                    if (okuyucu.ReadString() == "001")
                    {
                        Yanit.Durum = true;
                    }
                    else
                    {
                        Yanit.Durum = false;
                    }

                }
                else if (okuyucu.Name == "code")
                {
                    jokervada.KampanyaKodu = okuyucu.ReadString();
                    
                }
                else if (okuyucu.Name == "message")
                {
                    jokervada.KampanyaAciklama = okuyucu.ReadString();
                    Yanit.Kampanya.Add(jokervada);
                }
                else if (okuyucu.Name== "respCode")
                {
                    Yanit.YanitKodu = okuyucu.ReadString();
                }
                else if (okuyucu.Name== "respText")
                {
                    Yanit.YanitMesaji = okuyucu.ReadString();
                }

            }
            return Yanit;
        }
    }
}
