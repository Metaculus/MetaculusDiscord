# Specifikace zápočtového projektu z Programování a pokročilého programování v C# (NPRG035 a NPRG038)
### *Jan Provazník*

MetaculusDiscord je Discord bot, který zjednodušuje v Discordu interakci s API [Metaculus](https://www.metaculus.com) pro běžné uživatele a moderátory.

Funkce:
1. Vyhledání forecastingových otázek
2. Nastavení upozornění pro kanál či uživatele pro otázky, když se prudce změní nebo rozřeší, pomocí příkazů a emoji.
3. Nastavení upozornění pro kanál na kategorii, když se nějaká otázka z ní změní, rozřeší nebo přibyde, pomocí příkazů.
4. Rozesílání updatů otázek jednou za 6 hodin.
5. Rozesílání updatů kategorií jednou za 24 hodin.

Výchozím bodem je framework [Discord.NET](https://discordnet.dev/index.html) a data o upozornění jsou uložena v databázi PostgreSQL.