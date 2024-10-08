﻿//using System;
//using System.Text;
//using System.IO;
//using System.Linq;

//public class EncodingDetector
//{
//    public static Encoding DetectTextEncoding(string filename, out string text)
//    {
//        // Lê o arquivo como bytes
//        var buffer = File.ReadAllBytes(filename);

//        // Testa UTF-8
//        var utf8 = Encoding.UTF8;
//        var utf8Text = utf8.GetString(buffer);
//        if (utf8Text.Contains("�") == false)
//        {
//            text = utf8Text;
//            return utf8;
//        }

//        // Testa ISO-8859-1
//        var iso = Encoding.GetEncoding("ISO-8859-1");
//        var isoText = iso.GetString(buffer);
//        if (isoText.Contains("ç") && isoText.Contains("ã"))
//        {
//            text = isoText;
//            return iso;
//        }

//        // Se nenhuma codificação for detectada, retorna UTF-8 como padrão
//        text = utf8Text;
//        return utf8;
//    }
//}

using System.IO;
using System.Text;

public class EncodingDetector
{
    public static Encoding DetectTextEncoding(string filename, out string text)
    {
        var encodings = new[]
        {
            Encoding.UTF8,
            //Encoding.GetEncoding(1252), // ANSI - Western European
            Encoding.ASCII,
            Encoding.Unicode,           // UTF-16 LE
            Encoding.BigEndianUnicode,  // UTF-16 BE
            Encoding.UTF32,
            Encoding.GetEncoding(28591) // ISO-8859-1
        };

        using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
            try
            {
                byte[] bom = new byte[4];
                fileStream.Read(bom, 0, 4);
                fileStream.Position = 0;

                if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                {
                    text = Encoding.UTF8.GetString(File.ReadAllBytes(filename));
                    return Encoding.UTF8;
                }

                foreach (var enc in encodings)
                {
                    fileStream.Position = 0;
                    using (var reader = new StreamReader(fileStream, enc, true, 1024, true))
                    {
                        try
                        {
                            text = reader.ReadToEnd();
                            if (text.Contains('§'))
                            {
                                return enc;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch
            {
            }

            // Se nenhuma codificação for detectada, use UTF-8 como fallback
            text = File.ReadAllText(filename, Encoding.UTF8);
            return Encoding.UTF8;
        }
    }
}