using System.Xml;
using System;
using System.Security.Cryptography.Xml;

// pomocná třída, která podle ID hledá elementy nejen v dokumentu XML,
// ale i v připraveném elementu pro vložení časového razítka
public class CustomIdSignedXml : SignedXml
{
    XmlElement obj = null;

    public CustomIdSignedXml(XmlDocument doc)
        : base(doc)
    {
        return;
    }

    public CustomIdSignedXml(XmlDocument doc, XmlElement obj)
        : base(doc)
    {
        this.obj = obj;
    }

    public override XmlElement GetIdElement(XmlDocument doc, string id)
    {
        XmlElement res = base.GetIdElement(doc, id);

        // pokud byl element nalezen původní metodou, vrať ho
        if (res != null)
            return res;
        // nehledalo se náhodou KeyInfo
        else if (String.Compare(id, this.KeyInfo.Id, StringComparison.OrdinalIgnoreCase) == 0)
            return this.KeyInfo.GetXml();
        else
        {
            // pokus o nalezení elementu v předaném objektu pro umístění časového razítka
            XmlNode node = this.obj.SelectSingleNode("//*[@Id = '" + id + "']");
            if (node != null)
                return (XmlElement)node;
            else
                return null;
        }
    }
}
