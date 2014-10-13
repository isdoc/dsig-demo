# Ukázková aplikace pro podepisování a pøidání èasového razítka do dokumentù ISDOC

## Požadavky

Ukázková aplikace je napsaná v jazyce C# pro prostøedí .NET. Souèástí je i projekt pro Visual
Studio 2013, který dovoluje snadné spouštìní aplikace. Visual Studio je možné získat ve verzi Exprss zdarma
na adrese http://www.visualstudio.com/en-us/products/visual-studio-express-vs.aspx

### Vygenerování testovacích certifikátù

Aplikace pro podepisování používá testovací certifikáty. Ty je možné vygenerovat pomocí pøíkazu `makecert`.
Pøíkaz je dostupný po spuštìní **Visual Studio Command Prompt**.

````
makecert -r -pe -n "CN=Jan Novák" -ss My
makecert -r -pe -n "CN=Jana Procházková" -ss My
````

Certifikáty si mùžete vygenerovat výše uvedeným postupem znovu, nebo je mùžete importovat. Staèí
otevøít soubory `novak.pfx` a `prochazkova.pfx` uložené v adresáøi `data`. Prostøedí Windows automaticky
zpùstí prùvodce importem certifikátu.

### Správce certifikátù

Pøi práci s digitálními podpisy v prostøedí Windows je užiteèné používat správce certifikátù. Ten lze spustit pomocí **Start -> Spustit -> 
certmgr.msc**

Osobní certifikáty je možné spravovat na záložce **Personal -> Cetificates** (Osobní -> Certifikáty).



