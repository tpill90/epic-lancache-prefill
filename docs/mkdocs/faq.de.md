# Häufig gestellte Fragen

## Kann ich {{prefill_name}} auf dem Lancache Server ausführen?

Ja, kannst du! Du musst einfach nur **{{prefill_name}}** auf dem Server installieren und es regulär ausführen.

Wenn alles wie erwartet funktioniert, solltest du eine Nachricht sehen, dass ein Server unter der IP `127.0.0.1` gefunden wurde.

<img src="../images/svg/AutoDns-Server.svg" alt="Prefill running on Lancache Server">

Du kannst auch den Docker Container nutzen, in dem Falle sollte der Server stattdessen unter der IP `172.17.0.1` gefunden werden.

<img src="../images/svg/AutoDns-Docker.svg" alt="Prefill running on Lancache Server in Docker">

**{{prefill_name}}** auf dem Lancache Server selbst zu installieren bring einige Vorteile gegenüber der Installation auf einem Client Gerät mit sich, primär die Geschwindigkeit mit der Apps heruntergeladen werden können.
Dadurch dass keine Daten über das Netzwerk transportiert werden, sollte der `prefill` Befehl nur von der Festplatten- und CPU Leistung abhängig sein.
Wenn man beispielsweise eine **SK Hynix Gold P31 2TB NVME SSD** nutzt und `prefill --force` APPs aus dem Cache herunterlädt, sind solche Geschwindigkeiten zu erwarten:

<img src="../images/svg/AutoDns-ServerPerf.svg" alt="Prefill running on Lancache Server in Docker">

---

## Kann {{prefill_name}} automatisch nach Zeitplan ausgeführt werden?

Ja, kann es! Zeitabhängige Jobs können unter Linux einfach über `systemd` servuces eingerichtet werden und laufen flexibel nach dem von dir konfigurierten Zeitplan. Lies dir [Einen Nightly Job einrichten](https://tpill90.github.io/{{repo_name}}/install-guides/Scheduled-Job/) für eine Anleitung dazu durch.

---

## Kann ich meinen Cache mit bereits installierten {{gaming_service_name}} APPs füllen?

Es ist leider nicht möglich einen Lancache direkt mit bereits installierten {{gaming_service_name}} APPs zu füllen. Die installierten APPs sind in einem anderen Format als dem, welches Lancache zwischenspeichert, da die Dateien nach dem Download dekomprimiert und entschlüsselt werden. Dieser Prozess ist nicht umkehrbar, daher ist der einzige Weg APPs richtig im Cache zu speichern, sie über **{{prefill_name}}** oder **{{gaming_service_name}}** erneut herunter zu laden.

---

## Wo speichert {{prefill_name}} downloads?

**{{prefill_name}}** speichert gar keine Downloads! Es lädt die Daten einfach nur so schnell wie möglich über Lancache herunter, ohne die Daten jemals auf der Festplatte zu speichern. Lancache ist das Programm, welches beim herunterladen von Dateien diese zwischenspeichert, egal ob durch **{{gaming_service_name}}** oder **{{prefill_name}}**.

---

## Wie pausiere ich laufende Downloads?

Du kannst downloads jederzeit pausieren, indem du den Prozess einfach mit `STRG + C` beendest. Dadurch geht nichts kaputt, **{{prefill_name}}** macht beim nächsten ausführen einfach da weiter, wo es aufgehört hat.

---

## Ist es möglich APPs die ich nicht besitze herunterzuladen?

Auch wenn es definitiv nützlich (und günstiger!) wäre, APPs die du dich nicht besitzt herunterzuladen, ist es leider nicht möglich. Um APPs von {{gaming_service_name}} herunterzuladen, ist es nötig sich mit seinem Nutzernamen und Passwort zu Authentifizieren. {{gaming_service_name}} weiß welche APPs du besitzt, wodurch **{{prefill_name}}** die Liste der APPs die du besitzt in `select-apps` anzeigen kann. Wenn **{{prefill_name}}** versucht eine APP (ob im Besitz oder nicht) herunterzuladen, überprüft {{gaming_service_name}} ob dein Konto dazu berechtigt ist und verweigert den Download einfach, wenn du die APP nicht besitzt.

---

## Wie kann ich die Bandbreite beim Herunterladen begrenzen?

Du möchtest vielleicht die Geschwindigkeit beim herunterladen von Spielen durch **{{prefill_name}}** begrenzen, um zu verhindern dass dadurch die komplette Internetleitung ausgelastet wird, worunter die Verbindung anderer Geräte leiden könnte. Dieses Problem ist bekannt als Bufferbloat, mehr Informationen zu diesem Problem können hier gefunden werden: [What is bufferbloat?](https://waveform.com/tools/bufferbloat)

**{{prefill_name}}** hat aktuell keine Funktion zur Limitierung der Bandbreite und wird eine solche Funktion durch die Art wie downloads implementierrt sind auch nie haben. Selbst wenn **{{prefill_name}}** seine Bandbreite begrenzen könnte, würden die gleichen Probleme vermutlich bei Downloads durch **{{gaming_service_name}}** auftreten.

Eine Möglichkeit die Brandbreite zu begrenzen wäre _Quality of Service (QOS)_ auf deinem Router einzurichten, wodurch die BAndbreite des Lancache servers begrenzt wird, oder andere Geräte priorisiert werden. Eine Anleitung für Anfänger kann hier gefunden werden: [Beginners guide to QOS](https://www.howtogeek.com/75660/the-beginners-guide-to-qos-on-your-router/)

Anleitungen für bestimmte Router können hier gefunden werden:

- [Asus](https://asus.com/support/FAQ/1013333/)
- [Netgear](https://kb.netgear.com/25613/How-do-I-enable-Dynamic-QoS-on-my-Nighthawk-router)
- [Linksys](https://linksys.com/support-article?articleNum=50216)
- [TP-Link](https://tp-link.com/us/support/faq/557/)

---

## Meine Logs haben komische Zeichen und sind dadurch schwer zu lesen. Kann ich die Zeichen entfernen?

Abhängig von der verwendeten Konsole und den vom System unterstützten Farben, könnten die Logs so aussehen:

```text
[6:20:46 PM] Starting [38;5;80mFortnite[0m
[6:20:46 PM] Downloading [38;5;170m42.91 GiB[0m
```

Einer der Gründe weshalb die Logs so aussehen, könnte sein dass deine Konsole falsche Angaben zur Kompatibilität dieser Zeichen macht, wodurch es Ausgaben erhält die nicht verarbeitet werden können. Um diese Zeichen zu entfernen, nutze einfach die Option `--no-ansi`, wodurch alle möglicherweise nicht unterstützten Zeichen entfernt werden.

---

## Kann ich mehrere {{gaming_service_name}} Konten gleichzeitig verwenden?

Die Verwendung von mehreren Konten wird aktuell leider nicht von {{prefill_name}} unterstützt, da es als Programm für einen Nutzer geschrieben wurde. Allerdings gibt es eine einfache Methode, das zu umgehen. Da {{prefill_name}} als komplett eigenständiges Programm entworfen wurde, also alle Konfigurationen im Installationsverzeichnis bleiben, kannst du einfach mehrere Instanzen unter verschiedenen Pfaden installieren und dadurch mehrere Konten verwenden.

Du installierst {{prefill_name}} also einmal pro Konto, welches du anmelden willst. Wenn du beispielsweise 2 Konten verwenden willst, könnte deine Ordnerstruktur ungefähr so aussehen:

<img src="../images/svg/Multi-User-File-Structure.svg" alt="Structure required for two {{gaming_service_name}} accounts">

Nachdem mehrere Instanzen installiert wurden, können sie wie üblich unabhängig von einander über die Konsole verwendet werden.
