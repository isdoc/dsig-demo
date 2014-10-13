using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;


class Program
{
    static void Main(string[] args)
    {

        // vytvoření pomocné třídy pro práci s podpisy
        Utils utils = new Utils();

        // načtení dokumentu XML k podpisu
        Console.WriteLine("Načítám dokument priklad.isdoc...");
        XmlDocument doc = new XmlDocument();
        doc.PreserveWhitespace = true;               // bílé znaky musíme zachovat, jinak se špatně spočte hash        
        doc.Load("../../../data/priklad.isdoc");

        Console.WriteLine("Podepisuji dokument...");

        // načtení certifikátu z úložiště
        // certifikát je identifikován svým subjektem
        // pro reálné certifikáty vypadá např. "SERIALNUMBER=P111870, CN=Ing. Jiří Kosek, OU=1, O=Ing. Jiří Kosek [IČ 71612998], C=CZ"
        X509Certificate2 cert = utils.GetCertificate("CN=Jan Novák");
        if (cert == null) Console.WriteLine("Nepodařilo se načíst certifikát.");

        // podepsání dokumentu
        XmlDocument signedDoc = utils.Sign(doc, cert);

        // uložení podepsaného dokumentu
        Console.WriteLine("Ukládám podepsaný dokument do priklad-podepsany.isdoc...");
        signedDoc.Save("../../../data/priklad-podepsany.isdoc");

        Console.WriteLine("Podepisuji dokument...");
        
        // načtení certifikátu z úložiště
        X509Certificate2 cert2 = utils.GetCertificate("CN=Jana Procházková");
        if (cert2 == null) Console.WriteLine("Nepodařilo se načíst certifikát.");

        // připojení druhého podpisu k dokumentu
        XmlDocument doubleSignedDoc = utils.Sign(signedDoc, cert2);

        Console.WriteLine("Ukládám podepsaný dokument do priklad-podepsany2.isdoc...");
        doubleSignedDoc.Save("../../../data/priklad-podepsany2.isdoc");

        /*
        XmlDocument doc2 = new XmlDocument();
        doc2.PreserveWhitespace = true;           // bílé znaky musíme zachovat, jinak se špatně spočte hash
        doc2.Load("../../../data/priklad-podepsany.isdoc");
        System.Console.WriteLine(utils.Verify(doc2));
         */

        System.Console.ReadLine();

    }
}
