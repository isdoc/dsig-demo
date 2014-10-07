using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Xml;
using System;

public class Utils
{
    // Třída podepíše certifikátem dokument XML
    // Pokud je již dokument podepsaný, přidá se další podpis
    public XmlDocument Sign(XmlDocument doc, X509Certificate2 cert)
    {
        // Objekt sloužící pro vytvoření podpisu
        SignedXml signedXml = new SignedXml(doc);

        // podepisovat budeme privátním klíčem z certifikátu
        signedXml.SigningKey = cert.PrivateKey;

        // podepisovat budeme pomocí RSA-SHA256
        // podporuje je nutno doinstalovat, viz: http://clrsecurity.codeplex.com/wikipage?title=Security.Cryptography.RSAPKCS1SHA256SignatureDescription&referringTitle=Security.Cryptography.dll
        signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        // reference na podepisovaný dokument ("" znamená celý dokument)
        Reference reference = new Reference();
        reference.Uri = "";

        // pro výpočet otisku se bude používat SHA-256
        reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";

        // digitální podpis bude přímo součástí dokumentu XML (tzv. "enveloped signature")
        XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
        reference.AddTransform(env);

        // přidání reference do podpisu
        signedXml.AddReference(reference);

        // přidání certifikátu do podpisu
        KeyInfo keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(cert));
        signedXml.KeyInfo = keyInfo;

        // výpočet podpisu
        signedXml.ComputeSignature();

        // získání XML reprezentace podpisu
        XmlElement xmlSignature = signedXml.GetXml();

        // XML dokument pro podepsaný výsledek
        XmlDocument result = new XmlDocument();

        // načtení původního dokumentu
        result.ImportNode(doc, true);

        // připojení podpisu na konec dokumentu XML
        result.DocumentElement.AppendChild(result.ImportNode(xmlSignature, true));

        return result;
    }

}
