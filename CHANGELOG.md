# Changelog

## v0.1.4 (2026-10-11)

- Compatibility guard: if the game ever renames its save files (a new version
  prefix instead of `v10_`), the tool now says so in plain language - it lists the
  unexpected file names and points at the issue tracker - instead of only echoing
  a raw English `FileNotFoundException`.
- Quick edit: fields that no longer exist in the save are now reported instead of
  being skipped silently, so a game-side rename can no longer look like a
  successful save (same for companions that cannot be matched by id).
- New `scripts/check-game-compat.ps1`: read-only self-check to run after every
  game update. It reads the AES key, the file prefix and the managed field list
  straight out of the C# sources, then decrypts index/config/device/all slots,
  verifies every managed field and re-checks the index sync mapping. Exits 0/1.
- `--headless` now prints a field self-check summary and warns about unexpected
  save files, so it doubles as a compatibility probe.
- The headless menu dump uses the same official Chinese terms as the UI tab
  (东亚硬币 / 可罗奈莉亚 / 父女感情 / 名声 …), and no longer lists the derived
  good/evil balance that v0.1.3 dropped from editing.
- Docs: fixed the code fences in README / PUBLISHING - the language tag sat on the
  line below the fence, so it rendered as a stray line of code and lost syntax
  highlighting; README wording aligned with the official Chinese terms.
- CI: actions/checkout v4 -> v7, setup-dotnet v4 -> v6, upload-artifact v4 -> v7,
  softprops/action-gh-release v2 -> v3; new Build workflow runs `dotnet build` on
  every push and PR so compile errors surface before a tag does.
- Verified against the live game (Ver. 1.5.1, 2026-09-28) before shipping: same
  `v10_*` file names, same AES key/IV, zero schema drift against a pre-1.5.1 save,
  all 53 managed fields present, index sync mapping still exact, and a full
  read/modify/write round trip on a current save.

## v0.1.3 (2026-09-02)

- Slot picker now sits directly on the beginner tab - no more tab hopping.
- Companion favor (伙伴好感) section for the 7 named characters.
- All field labels aligned with the game's official Chinese terms (thanks to
  the community CT table).
- Removed derived values that cannot be edited meaningfully (the four computed
  main attributes, good/evil balance).
- Save-directory UX: loud auto-locate feedback, persistent path label, manual
  dialog accepts pasted paths (AppData is a hidden folder).
- README: note that the release build is self-contained and needs no .NET.

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

## v0.1.0 (2026-09-02)

- Initial release.
- Save slot browser (31 slots) with per-slot metadata.
- JSON tree editor with type-checked editing.
- Quick edit: money / stress / black coins / action points / skill points / father favor.
- Game settings editor: resolution, fullscreen, quality, vsync, language, volumes, text speed.
- Automatic timestamped backups before every write.
- Headless self-test mode (--headless).
