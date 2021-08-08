using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;


namespace Aktarim
{
    class Program
    {
        static StringBuilder sonuc = new StringBuilder();
        static StringBuilder text = new StringBuilder();
        private static Regex tarihPattern = new Regex("[0-9]{2}.[0-9]{2}.[0-9]{4}");

        static void Main(string[] args)
        {
            var path = Environment.CurrentDirectory;
            var birimler = new Dictionary<int, string>
            {
                {1,"Mühendislik Fakültesi"},
                {2,"FEN-EDEBİYAT FAKÜLTESİ"},
                {3,"ULA MYO"}
            };
            DirectoryInfo info = new DirectoryInfo(path + "\\Kararlar");
            using var sw = new StreamWriter(path + "\\sql.txt", false);
            using var swTarihsiz = new StreamWriter(path + "\\tarihsiz.txt", false);
            var rakamRegex = new Regex("([0-9])\\w+");
            string creatorId = null;
            foreach (DirectoryInfo subinfo in info.GetDirectories())
            {
                Console.WriteLine($"{subinfo.Name }");
                foreach (FileInfo file in subinfo.GetFiles())
                {
                    Console.WriteLine("    " + file.Name);
                    var kararTipi = kararTipiGetir(file.Name);
                    var tarih = ExtractDateFromPDF(file.DirectoryName + "\\" + file.Name);
                    var result = rakamRegex.Match(file.Name).Value;
                    var sayi = result.Split('_')[1];
                    var birimId = birimler.First(i => i.Value.ToLower().Equals(subinfo.Name.ToLower())).Key;
                    int isDeleted = 0;
                    int dosyaVarmi = 0;
                    string kararTarih = string.Empty;
                    if (tarih == null)
                    {
                        kararTarih = $"{result.Split('_')[0]}.01.01";
                        swTarihsiz.WriteLine($"Karar tarihi yok-----Klasör:{subinfo.Name}-----Dosya:{file.Name}");
                    }
                    else
                    {
                        kararTarih = $"{tarih.Value.Date.Year}.{tarih.Value.Date.Month}.{tarih.Value.Date.Day}";
                    }
                    var creator = string.IsNullOrWhiteSpace(creatorId) ? "NULL" : creatorId;
                    sw.WriteLine($"insert into MskuKararlar values('{kararTipi}',{birimId},'{sayi}','{kararTarih}',NULL,NULL,GETDATE(),{creator},NULL,NULL,0,NULL,NULL,'E')");
                }
            }

            sw.Close();
            Console.WriteLine("********Sql oluşturuldu********");
            Console.WriteLine("--------sql.txt dosyasını kontrol ediniz--------");
            Console.WriteLine("--------tarihi okunamayan dosyalar için tarihsiz.txt dosyasını kontrol ediniz--------");
            Console.ReadKey();
        }

        static string kararTipiGetir(string tip)
        {
            var karartipi = Regex.Replace(tip, @"\d", "").Split('.')[0].Trim('_').Trim(new[] { '_', '_' });
            return BasHarfleriBuyukYap(karartipi.Split('_'));
        }

        static string BasHarfleriBuyukYap(string[] kararTip)
        {
            sonuc.Clear();
            foreach (var tip in kararTip)
            {
                sonuc.Append(char.ToUpper(tip[0]));
                sonuc.Append(tip.Substring(1));
                sonuc.Append(" ");
            }
            return sonuc.ToString().Trim();
        }



        public static DateTime? ExtractDateFromPDF(string filePath)
        {
            PdfReader pdfReader = new PdfReader(filePath);

            PdfDocument pdfDoc = new PdfDocument(pdfReader);
            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
            string pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(1), strategy);
            var tarih = tarihPattern.Match(pageContent).Value;
            var tarihMi = DateTime.TryParse(tarih, out var newDate);
            pdfDoc.Close();
            pdfReader.Close();
            return tarihMi ? newDate : (DateTime?)null;
        }

        //static DateTime? GetTarih(string pdfPath)
        //{
        //using PdfReader reader = new PdfReader(pdfPath);
        //text.Clear();
        //text.Append(PdfTextExtractor.GetTextFromPage(reader, 1));
        //var tarih = tarihPattern.Match(text.ToString()).Value;
        //var tarihMi = DateTime.TryParse(tarih, out var newDate);
        //return tarihMi ? newDate : (DateTime?)null;
        //return (DateTime?)null;
        //}
    }
}
