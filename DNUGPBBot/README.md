# README #

Demo-Projekt(e) zum Vortrag ["Microsoft Bot Framework (.NET Edition)"]() bei der dotnet Cologne am 10.05.2019.

Die .NET Variante des DNUGPBBot ist im Azure Bot Service verfügbar und kann unter [dnugpbbot.azurewebsites.net](http://dnugpbbot.azurewebsites.net) als Web Chat getestet werden.

## Projekt-Setup ##

### Allgemeine Infos ###

* Für dieses Projekt wird ein Developer Key für Bing Maps benötigt. Diesen kann man gemäß folgender Anleitung beschaffen: [Getting a Bing Maps Key](https://msdn.microsoft.com/de-de/library/ff428642.aspx).
* Für die Verwendung von [LUIS](https://luis.ai) wird je eine LUIS App ID und ein LUIS Authoring Key benötigt. Diese können bei der Veröffentlichung einer LUIS App ermittelt werden: [Publish your trained app](https://docs.microsoft.com/en-us/azure/cognitive-services/LUIS/publishapp).
* Für das Deployment eines Chatbots in den Azure Bot Service ist darauf zu achten, dass sowohl für den Bot Service, als auch für den App Service Plan der Tarif **"Free" (F0/F1)** gewählt werden. Sonst wird es sehr schnell sehr teuer!

### Projekt-Setup ###

* Was wird benötigt?
    * Visual Studio 2017 oder 2019 (Community Edition reicht aus) inkl. ASP.NET Core Workload (für die Entwicklung) und Azure Workload (für die Veröffentlichung des Bots im Azure Bot Service).
    * Bot Framework Emulator

* Die benötigten Schlüssel und IDs (siehe **Allgemeine Infos**) müssen in der Datei "ApiKeys.cs" im Verzeichnis "Resources" hinterlegt werden.
* Die benötigten NuGet-Pakete müssen wiederhergestellt werden.
* Der Chatbot kann über die Debug-Funktion von Visual Studio gestartet werden.
* Zum Testen kann der Bot Framework Emulator verwendet werden. Die Endpoint URL lautet: <http://localhost:3978/api/messages>.
* In verschiedenen Code-Dateien können unterschiedliche Funktionen durch ein-/auskommentieren an- bzw. ausgeschaltet werden:
    * **DNUGPBBot.cs (Zeile 83-85):** Wechsel zwischen einfachem SearchDialog und erweitertem Dialog-Flow (mit einfachem UI und erweitertem UI, z.B. AdaptiveCards).
    * **Dialogs/RootDialogExtendedUI.cs (Zeile 56-57):** Wechsel zwischen SearchDialog mit erweitertem UI und LUIS-basiertem SearchDialog.
