# Ukázková aplikace pro podepisování a přidání časového razítka do dokumentů ISDOC

## Požadavky

Ukázková aplikace je napsaná v jazyce C# pro prostředí .NET. Součástí je i projekt pro Visual
Studio 2013, který dovoluje snadné spouštění aplikace. Visual Studio je možné získat ve verzi Exprss zdarma
na adrese http://www.visualstudio.com/en-us/products/visual-studio-express-vs.aspx

### Vygenerování testovacích certifikátů

Aplikace pro podepisování používá testovací certifikáty. Ty je možné vygenerovat pomocí příkazu `makecert`.
Příkaz je dostupný po spuštění **Visual Studio Command Prompt**.

````
makecert -r -pe -n "CN=Jan Novák" -ss My
makecert -r -pe -n "CN=Jana Procházková" -ss My
````

Certifikáty si můžete vygenerovat výše uvedeným postupem znovu, nebo je můžete importovat. Stačí
otevřít soubory `novak.pfx` a `prochazkova.pfx` uložené v adresáři `data`. Prostředí Windows automaticky
způstí průvodce importem certifikátu.

### Správce certifikátů

Při práci s digitálními podpisy v prostředí Windows je užitečné používat správce certifikátů. Ten lze spustit pomocí **Start -> Spustit -> 
certmgr.msc**

Osobní certifikáty je možné spravovat na záložce **Personal -> Cetificates** (**Osobní -> Certifikáty**).



