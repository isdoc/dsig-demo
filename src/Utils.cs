using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Xsl;
using System;
using System.IO;
using System.Net;
using Security.Cryptography;
using Org.BouncyCastle.Tsp;

public class Utils
   
{
    public Utils()
    {
        // ručně musíme přidat podporu pro podpisy RSA-SHA-256, .NET framework ji standardně nepodporuje
        // pro starší verze .NET frameworku je navíc potřeba doinstalovat knihovny z adresy http://clrsecurity.codeplex.com/wikipage?title=Security.Cryptography.RSAPKCS1SHA256SignatureDescription&referringTitle=Security.Cryptography.dll
        RSAPKCS1SHA256SignatureDescription desc = new RSAPKCS1SHA256SignatureDescription();
        System.Security.Cryptography.CryptoConfig.AddAlgorithm(desc.GetType(), "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
    }

    // Třída podepíše certifikátem dokument XML
    // Pokud je již dokument podepsaný, přidá se další podpis
    public XmlDocument Sign(XmlDocument doc, X509Certificate2 cert)
    {
        // před podepisováním z dokumentu odstraníme komentáře (.NET s nimi má problémy pokud se kombinují s XPath transformacemi)
        XmlDocument strippedDoc = RemoveComments(doc);

        // definice mapování prefixů na jmenné prostory
        XmlNamespaceManager manager = new XmlNamespaceManager(strippedDoc.NameTable);
        manager.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");

        // zjištění kolik podpisů již v dokumentu je
        int signatures = strippedDoc.SelectNodes("//dsig:Signature", manager).Count;

        // objekt sloužící pro vytvoření podpisu
        SignedXml signedXml = new SignedXml(strippedDoc);

        // podepisovat budeme privátním klíčem z certifikátu
        signedXml.SigningKey = cert.PrivateKey;

        // podepisovat budeme pomocí RSA-SHA256
        signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        // reference na podepisovaný dokument ("" znamená celý dokument)
        Reference reference = new Reference();
        reference.Uri = "";

        // pro výpočet otisku se bude používat SHA-256
        reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";

        // digitální podpis bude přímo součástí dokumentu XML (tzv. "enveloped signature")
        XmlDsigEnvelopedSignatureTransform envTransform = new XmlDsigEnvelopedSignatureTransform();
        reference.AddTransform(envTransform);

        // navíc budeme používat XPath transoformaci, která dovoluje přidat několik podpisů najednou
        XmlDsigXPathTransform xpathTransform = new XmlDsigXPathTransform();

        // příprava definice XPath transformace jako struktura XML signature
        XmlDocument transformBody = new XmlDocument();

        // podoba XPath filtru se liší podle počtu podpisů
        if (signatures == 0)
            transformBody.LoadXml("<dsig:XPath xmlns:dsig='http://www.w3.org/2000/09/xmldsig#'>not(ancestor-or-self::dsig:Signature)</dsig:XPath>");
        else
            transformBody.LoadXml("<dsig:XPath xmlns:dsig='http://www.w3.org/2000/09/xmldsig#'>not(ancestor-or-self::dsig:Signature) or not(ancestor-or-self::dsig:Signature/preceding-sibling::dsig:Signature[" + signatures + "])</dsig:XPath>");

        // načtení definice XPath transformace do objektu
        xpathTransform.LoadInnerXml(transformBody.SelectNodes("/*[1]"));
        
        // přidání XPath transformace
        reference.AddTransform(xpathTransform);

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

        // k podpisu přidáme identifikátor, tak jak doporučuje standard ISDOC
        xmlSignature.SetAttribute("Id", "Signature-" + (signatures + 1));

        // XML dokument pro podepsaný výsledek
        XmlDocument result = new XmlDocument();

        // bílé znaky musíme zachovat, jinak se špatně spočte hash
        result.PreserveWhitespace = true;               

        // načtení původního dokumentu
        result.AppendChild(result.ImportNode(strippedDoc.DocumentElement, true));

        // připojení podpisu na konec dokumentu XML
        result.DocumentElement.AppendChild(result.ImportNode(xmlSignature, true));

        return result;
    }

    // Třída podepíše certifikátem dokument XML a přidá časové razítko
    // Pokud je již dokument podepsaný, přidá se další podpis
    public XmlDocument SignWithTimestamp(XmlDocument doc, X509Certificate2 cert, string tsURL, string tsUsername, string tsPassword)
    {
        // před podepisováním z dokumentu odstraníme komentáře (.NET s nimi má problémy pokud se kombinují s XPath transformacemi)
        XmlDocument strippedDoc = RemoveComments(doc);

        // definice mapování prefixů na jmenné prostory
        XmlNamespaceManager manager = new XmlNamespaceManager(strippedDoc.NameTable);
        manager.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");

        // zjištění kolik podpisů již v dokumentu je
        int signatures = strippedDoc.SelectNodes("//dsig:Signature", manager).Count;

        string signatureID = (signatures + 1).ToString();

        // vytvoření elementu Object pro časové razítko
        XmlElement objectElement = doc.CreateElement("Object", "http://www.w3.org/2000/09/xmldsig#");

        // spočítání otisku certifikátu
        SHA256 sha256 = new SHA256Managed();
        string certHash = Convert.ToBase64String(sha256.ComputeHash(cert.GetRawCertData()));

        objectElement.InnerXml = @"<xades:QualifyingProperties xmlns:xades='http://uri.etsi.org/01903/v1.3.2#' Target='#Signature-" + signatureID + @"' xmlns='http://www.w3.org/2000/09/xmldsig#'>
                <xades:SignedProperties Id='Signature-" + signatureID + @"-SignedProperties'>
                  <xades:SignedSignatureProperties>
                    <xades:SigningTime>" + XmlConvert.ToString(DateTime.Now.ToUniversalTime(), XmlDateTimeSerializationMode.RoundtripKind) + @"</xades:SigningTime>
                    <xades:SigningCertificate>
                      <xades:Cert>
                        <xades:CertDigest>
                          <DigestMethod Algorithm='http://www.w3.org/2001/04/xmlenc#sha256'></DigestMethod>
                          <DigestValue>" + certHash + @"</DigestValue>
                        </xades:CertDigest>
                        <xades:IssuerSerial>
                          <X509IssuerName>" + cert.IssuerName + @"</X509IssuerName>
                          <X509SerialNumber>" + cert.GetSerialNumberString() + @"</X509SerialNumber>
                        </xades:IssuerSerial>
                      </xades:Cert>
                    </xades:SigningCertificate>
                  </xades:SignedSignatureProperties>
                  <xades:SignedDataObjectProperties>
                    <xades:DataObjectFormat ObjectReference='#Signature-" + signatureID + @"-Document-Reference'>
                      <xades:MimeType>application/xml</xades:MimeType>
                    </xades:DataObjectFormat>
                  </xades:SignedDataObjectProperties>
                </xades:SignedProperties>
                <xades:UnsignedProperties>
                  <xades:UnsignedSignatureProperties>
                    <xades:SignatureTimeStamp>
                      <xades:EncapsulatedTimeStamp Encoding='http://uri.etsi.org/01903/v1.2.2#DER'></xades:EncapsulatedTimeStamp>
                    </xades:SignatureTimeStamp>
                  </xades:UnsignedSignatureProperties>
                </xades:UnsignedProperties>
               </xades:QualifyingProperties>";


        // objekt sloužící pro vytvoření podpisu
        CustomIdSignedXml signedXml = new CustomIdSignedXml(strippedDoc, objectElement);

        // podepisovat budeme privátním klíčem z certifikátu
        signedXml.SigningKey = cert.PrivateKey;

        // podepisovat budeme pomocí RSA-SHA256
        signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        // reference na podepisovaný dokument ("" znamená celý dokument)
        Reference reference = new Reference();
        reference.Uri = "";
        reference.Id = "Signature-" + signatureID + "-Document-Reference";

        // pro výpočet otisku se bude používat SHA-256
        reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";

        // digitální podpis bude přímo součástí dokumentu XML (tzv. "enveloped signature")
        XmlDsigEnvelopedSignatureTransform envTransform = new XmlDsigEnvelopedSignatureTransform();
        reference.AddTransform(envTransform);

        // navíc budeme používat XPath transoformaci, která dovoluje přidat několik podpisů najednou
        XmlDsigXPathTransform xpathTransform = new XmlDsigXPathTransform();

        // příprava definice XPath transformace jako struktura XML signature
        XmlDocument transformBody = new XmlDocument();

        // podoba XPath filtru se liší podle počtu podpisů
        if (signatures == 0)
            transformBody.LoadXml("<dsig:XPath xmlns:dsig='http://www.w3.org/2000/09/xmldsig#'>not(ancestor-or-self::dsig:Signature)</dsig:XPath>");
        else
            transformBody.LoadXml("<dsig:XPath xmlns:dsig='http://www.w3.org/2000/09/xmldsig#'>not(ancestor-or-self::dsig:Signature) or not(ancestor-or-self::dsig:Signature/preceding-sibling::dsig:Signature[" + signatures + "])</dsig:XPath>");

        // načtení definice XPath transformace do objektu
        xpathTransform.LoadInnerXml(transformBody.SelectNodes("/*[1]"));

        // přidání XPath transformace
        reference.AddTransform(xpathTransform);

        // přidání reference do podpisu
        signedXml.AddReference(reference);

        // reference na SignedProperties -- XAdES-BES vyžaduje podpis certifikátu
        Reference spReference = new Reference();
        spReference.Uri = "#Signature-" + signatureID + "-SignedProperties";

        // pro výpočet otisku se bude používat SHA-256
        spReference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";

        // přidání reference do podpisu
        signedXml.AddReference(spReference);

        // přidání certifikátu do podpisu
        KeyInfo keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(cert));
        signedXml.KeyInfo = keyInfo;

        // přidání objektu s časovým razítkem do podpisu
        DataObject dataObj = new DataObject();
        dataObj.LoadXml(objectElement);
        signedXml.AddObject(dataObj);

        // výpočet podpisu
        signedXml.ComputeSignature();

        // získání XML reprezentace podpisu
        XmlElement xmlSignature = signedXml.GetXml();

        // k podpisu přidáme identifikátor, tak jak doporučuje standard ISDOC
        xmlSignature.SetAttribute("Id", "Signature-" + signatureID);

        // XML dokument pro podepsaný výsledek
        XmlDocument result = new XmlDocument();

        // bílé znaky musíme zachovat, jinak se špatně spočte hash
        result.PreserveWhitespace = true;

        // načtení původního dokumentu
        result.AppendChild(result.ImportNode(strippedDoc.DocumentElement, true));

        // připojení podpisu na konec dokumentu XML
        result.DocumentElement.AppendChild(result.ImportNode(xmlSignature, true));

        // Spočítání otisku digitálního podpisu
        byte[] digest;
        digest = sha256.ComputeHash(signedXml.SignatureValue);

        // generátor požadavků na časové razítko
        TimeStampRequestGenerator reqGen = new TimeStampRequestGenerator();

        // vytvoření dat pro požadavek na timestamp server
        TimeStampRequest request = reqGen.Generate(TspAlgorithms.Sha256, digest);

        // získání surových dat pro poslání na timestamp server
        byte[] reqData = request.GetEncoded();

        // inicializace požadavku na timestamp server
        HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(tsURL);
        httpReq.Method = "POST";
        httpReq.ContentType = "application/timestamp-query";
        httpReq.ContentLength = reqData.Length;
        httpReq.Credentials = new NetworkCredential(tsUsername, tsPassword);

        // odeslání požadavku na timestamp server
        Stream reqStream = httpReq.GetRequestStream();
        reqStream.Write(reqData, 0, reqData.Length);
        reqStream.Close();

        // přečtení odpovědi
        HttpWebResponse httpResp = (HttpWebResponse)httpReq.GetResponse();
        Stream respStream = new BufferedStream(httpResp.GetResponseStream());
        TimeStampResponse response = new TimeStampResponse(respStream);
        respStream.Close();
        
        // Console.WriteLine("Status razítkování: " + response.Status);
        // Console.WriteLine("Čas razítka: " + response.TimeStampToken.TimeStampInfo.GenTime.ToLocalTime());
        string timestamp = Convert.ToBase64String(response.GetEncoded());

        // doplnění získaného časového razítka do dokumentu
        XmlNamespaceManager nsmng = new XmlNamespaceManager(result.NameTable);
        nsmng.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
        nsmng.AddNamespace("xades", "http://uri.etsi.org/01903/v1.3.2#");
        XmlElement etsElement = (XmlElement)result.SelectSingleNode("//*[@Id = 'Signature-" + signatureID +"']/ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:SignatureTimeStamp/xades:EncapsulatedTimeStamp", nsmng);
        etsElement.InnerText = timestamp;

        return result;
    }

    // vrátí certifikát identifikovaný pomocí Subject name z uživatelského úložiště certifikátů
    public X509Certificate2 GetCertificate(String subject)
    {
        // otevření uživatelského úložiště certifikátů
        X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.OpenExistingOnly);

        // načtení všech certifikátů z úložiště
        X509Certificate2Collection certificates = store.Certificates;

        // nalezení správného certifikátu
        foreach (X509Certificate2 c in certificates)
        {            
            if (c.Subject.Equals(subject))
            {
                return c;
            }
        }

        return null;
    }

    // Třída ověří všechny podpisy v dokumentu
    public bool Verify(XmlDocument doc)
    {
        // definice mapování prefixů na jmenné prostory
        XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
        manager.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");

        // příznak validnosti podpisu
        bool validates = false;

        // vybereme všechny podpisy v dokumentu
        XmlNodeList signatures = doc.SelectNodes("/*/dsig:Signature", manager);

        // pokud by v dokumentu nebylani jeden podpis, nemůže být podpis validní
        if (signatures.Count > 0) validates = true;

        // postupné ověření všech nalezených podpisů
        for (int i = 0; i < signatures.Count; i++)
        {
            // načtení XML reprezentace podpisu
            XmlElement signatureElement = (XmlElement)signatures.Item(i);

            // načtení dokumentu do objektu pro práci s podpisy
            SignedXml signedDoc = new SignedXml(doc);

            // nastavení elementu, ve kterém se má kontrolovat podpis
            signedDoc.LoadXml(signatureElement);

            // kontrola podpisu
            if (!signedDoc.CheckSignature()) validates = false;
        }
        return validates;
    }

    // Pomocná metoda pro odstranění komentářů z dokumetu
    // Ponechány jsou jen důležité části XML -- elementy, atributy a textové uzly
    public XmlDocument RemoveComments(XmlDocument doc)
    {
        // vytvoření kopie dokumentu
        XmlDocument result = new XmlDocument();
        result.PreserveWhitespace = true;
        result.AppendChild(result.ImportNode(doc.DocumentElement, true));

        // vybrání všech komentářů
        XmlNodeList comments = result.SelectNodes("//comment()");

        // odstranění komentářů
        foreach(XmlNode n in comments)
        {
            n.ParentNode.RemoveChild(n);
        }

        return result;
    }

}