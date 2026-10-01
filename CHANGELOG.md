# Changelog

Alle nennenswerten Änderungen an DHLabel werden hier festgehalten.

## [1.5.9]

- Fix: Eine Datei als Kommandozeilenparameter beim ersten Programmstart (z. B.
  über "Öffnen mit") wurde nicht geladen - der übergebene Pfad wurde zwar
  entgegengenommen, aber nie an `LoadFile` weitergereicht.
- Fix: Wurde eine zweite Datei geöffnet, während DHLabel bereits lief, wurde
  sie durch einen Index-Fehler in der Einzelinstanz-Behandlung ignoriert.
- Fix: Die Checkbox "Öffnen mit..." registrierte DHLabel bisher nicht
  tatsächlich im "Öffnen mit"-Menü von Windows für PDF-Dateien - sie speicherte
  nur eine Einstellung, ohne die Registry zu schreiben.
- Fix: Business-Label-PDFs (Geschäftskundenportal) konnten nicht geöffnet
  werden ("Nicht genügend Arbeitsspeicher") - sowohl wegen eines nicht von
  GDI+ unterstützten Alphakanal-Bitmapformats beim PDF-Rendering als auch
  wegen eines beim Umstieg auf PDFiumSharp verloren gegangenen, formatabhängigen
  Zuschnitts. Für Business-Labels ist jetzt wieder ein eigener, kalibrierter
  Zuschnitt vorhanden.
- Robustheit: Ein zu groß gewähltes Zuschnitt-Rechteck führt nicht mehr zu
  einem Absturz, sondern wird auf die tatsächliche Bildgröße begrenzt.

<!--
  Naechster Eintrag: Abschnitt "## [x.y.z]" direkt darunter einfuegen,
  mit der Version aus Program\Properties\AssemblyInfo.cs. Wird von
  publish-release.ps1 ausgelesen und muss zum Release passen.
-->
