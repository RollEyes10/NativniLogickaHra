# Nativní Logická Hra

Krátký popis

- Jednoduchá hra Šibenice napsaná v .NET MAUI (.NET 10).
- Hra umí načítat slova z AI providerů (Gemini, ChatGPT, Claude) a také používat lokální záložní slovo.

Požadavky

- .NET 10 SDK
- Visual Studio (např. 2026) s podporou .NET MAUI
- Internetové připojení pro volání AI

Jak spustit

1. Otevři řešení v Visual Studiu.
2. Zvol cílovou platformu (Android/iOS/Windows) a spusť.

Konfigurace AI providerů

- Otevři stránku `ConnectionAI` v aplikaci (Menu: Správa API klíčů).
- Vyber provider (`Gemini`, `ChatGPT` nebo `Claude`) v `Picker`u a vlož svůj API klíč do pole.
- Stiskni `Uložit / Aktualizovat`.

Uložené klíče

- Klíče jsou ukládány do `SecureStorage` pod názvy: `Gemini`, `ChatGPT`, `Claude`.
- Pro Gemini se navíc ukládá režim do klíče `Gemini_Mode` (hodnoty `FREE` nebo `PAID`).

Poznámka k AIGameFactory

- Soubor `AIGameFactory.cs` v projektu nyní používá jiné názvy klíčů (`api_Gemini`, `api_ChatGPT`, `api_Claude`). Pokud chceš, aby `AIGameFactory` používal stejné klíče jako UI (`Gemini` / `ChatGPT` / `Claude`), uprav kód v `AIGameFactory.cs` nebo změň způsob ukládání klíčů v `ConnectionAI`.

Kdy se volá AI

- AI se volá při startu nové hry (`Game.StartNewGameAsync`) — tedy jednou na každou novou hru.
- Pokud hra končí výhrou/prohrou, volá se `StartNewGameAsync()` znovu a tím i další request.
- Pro Gemini existuje lokální rate-limiter: 5 dotazů / minutu (nastavený v `AiWordService`). ChatGPT a Claude nemají v projektu centrální limiter.

Logování

- Volání a odpovědi od AI (syrové odpovědi) jsou zapisovány do logu `ai_calls.log` v adresáři aplikace (`FileSystem.AppDataDirectory`).
- Pokud AI neodpoví nebo klíč chybí, v UI se zobrazí odpovídající zpráva a použije se záložní slovo.

Lokalní debug tipy

- Zkontroluj `ai_calls.log` pro surové HTTP odpovědi a chyby.
- Ujisti se, že máš správný API klíč uložený pod správným názvem.

Dálší vylepšení (možnosti)

- Přidat zobrazování logu přímo v UI pro snadné ladění.
- Přidat timeouty / retry politiku pro ChatGPT a Claude.
- Sjednotit názvy SecureStorage klíčů mezi UI a `AIGameFactory`.

Autor

Projekt v repozitáři: https://github.com/RollEyes10/NativniLogickaHra

