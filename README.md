# Ukázková aplikace pro podepisování a přidání časového razítka do dokumentů ISDOC

Ukázkovou aplikaci si můžete naklonovat pomocí prostředků verzovacího systému Git. Alternativně
si můžete všechny soubory stáhnout jako jeden archiv z adresy https://github.com/isdoc/dsig-demo/archive/master.zip

## Požadavky

Ukázková aplikace je napsaná v jazyce C# pro prostředí .NET. Součástí je i projekt pro Visual
Studio 2013, který dovoluje snadné spouštění aplikace. Visual Studio je možné získat ve verzi Express zdarma
na adrese http://www.visualstudio.com/en-us/products/visual-studio-express-vs.aspx

> Elektronické podpisy a PKI je poměrně komplexní obor. Před psaním jakékoliv aplikace
> výrazně doporučujeme seznámit se s celou oblastí, aby z neznalosti nebyla do vaší aplikace
> zanesena bezpečnostní chyba.

> Ukázkové aplikace pro zachování jednoduchosti a názornosti některé věci opomíjejí.
> Např. v tuto chvíli nekontrolují, zda byl certifikán od svého vydání zneplatněn pomocí CRL.

## Obsah ukázkové aplikace

* `src` - adresář se zdrojovým kódem
  * `Program.cs` - hlavní třída, která volá operace pro podepisování a kontrolu podpisů
  * `Utils.cs`- pomocná třída s definicií všech důležitých operací
  * `RSAPKCS1SHA256SignatureDescription.cs`- pomocná třída registrující RSA-SHA-256 pro použití v .NET Frameworku
* `data`- adresář obsahující ukázkové soubory

## Certifikáty

### Vygenerování testovacích certifikátů

Aplikace pro podepisování používá testovací certifikáty. Ty je možné vygenerovat pomocí příkazu `makecert`.
Příkaz je dostupný po spuštění **Visual Studio Command Prompt**.

````
makecert -a sha256 -len 2048 -r -pe -n "CN=Jan Novák" -ss My -sy 24
makecert -a sha256 -len 2048 -r -pe -n "CN=Jana Procházková" -ss My -sy 24
````

Certifikáty si můžete vygenerovat výše uvedeným postupem znovu, nebo je můžete importovat. Stačí
otevřít soubory `novak.pfx` a `prochazkova.pfx` uložené v adresáři `data`. Prostředí Windows automaticky
spustí průvodce importem certifikátu. Certifikáty jsou chráněny heslem `heslo`.

### Správce certifikátů

Při práci s digitálními podpisy v prostředí Windows je užitečné používat správce certifikátů. Ten lze spustit pomocí **Start -> Spustit -> 
certmgr.msc**

Osobní certifikáty je možné spravovat na záložce **Personal -> Cetificates** (**Osobní -> Certifikáty**).



