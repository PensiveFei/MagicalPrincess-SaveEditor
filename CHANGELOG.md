# Changelog

## v0.1.2 (2026-09-02)

- Rewrote the README in a friendlier, less corporate tone; bilingual layout
  with a game recommendation section written by the project owner.
- Chinese name changed to 「存档修改器」(repo and English name unchanged).
- Removed all inline HTML tags from the README — they broke GitHub rendering
  in v0.1.1 (from "How it works (technical)" on, the rest of the page
  rendered in code style). All clear now.
- Updated publishing drafts (NexusMods / Steam guide) and in-app About text.

## v0.1.1 (2026-09-02)

- New beginner tab ("常用修改"): labeled quick-edit fields in Chinese with
  current values pre-filled; blank = keep original value. One-click save.
- Bilingual README (English + 简体中文) with a game recommendation section.
- Internal: menu dump now written to a UTF-8 file in headless mode.

## v0.1.0 (2026-09-xx)

- Initial release.
- Save slot browser (31 slots) with per-slot metadata.
- JSON tree editor with type-checked editing.
- Quick edit: money / stress / black coins / action points / skill points / father favor.
- Game settings editor: resolution, fullscreen, quality, vsync, language, volumes, text speed.
- Automatic timestamped backups before every write.
- Headless self-test mode (--headless).
