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
        XmlDocument doc = new XmlDocument();
        doc.PreserveWhitespace = true;           // bílé znaky musíme zachovat, jinak se špatně spočte hash
        doc.Load("../../../data/priklad.isdoc");

        X509Certificate2 cert = utils.GetCertificate("SERIALNUMBER=P111870, CN=Ing. Jiří Kosek, OU=1, O=Ing. Jiří Kosek [IČ 71612998], C=CZ");

        XmlDocument signedDoc = utils.Sign(doc, cert);
        
        System.Console.WriteLine(utils.Verify(signedDoc));

        signedDoc.Save("../../../data/priklad-podepsany.isdoc");

        XmlDocument doubleSignedDoc = utils.Sign(signedDoc, cert);
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
