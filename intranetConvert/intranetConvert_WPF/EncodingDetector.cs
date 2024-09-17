using System;
using System.Text;
using System.IO;
using System.Linq;

public class EncodingDetector
{
    public static Encoding DetectTextEncoding(string filename, out string text)
    {
        // Lê o arquivo como bytes
        var buffer = File.ReadAllBytes(filename);

        // Testa UTF-8
        var utf8 = Encoding.UTF8;
        var utf8Text = utf8.GetString(buffer);
        if (utf8Text.Contains("�") == false)
        {
            text = utf8Text;
            return utf8;
        }

        // Testa ISO-8859-1
        var iso = Encoding.GetEncoding("ISO-8859-1");
        var isoText = iso.GetString(buffer);
        if (isoText.Contains("ç") && isoText.Contains("ã"))
        {
            text = isoText;
            return iso;
        }

        // Se nenhuma codificação for detectada, retorna UTF-8 como padrão
        text = utf8Text;
        return utf8;
    }
}