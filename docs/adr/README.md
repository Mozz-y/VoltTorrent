# Architecture Decision Records

This directory contains Architecture Decision Records (ADRs) for technical choices that shape VoltTorrent.

Use `0000-template.md` when adding a new decision.

## Initial ADR Candidates

- Desktop shell selection: Photino.NET.
- Custom Bencode parser implementation.
- JSON-first persistence versus SQLite from the beginning.
- SQLite data access approach: Dapper versus EF Core.
- Channels for piece/block scheduling.
- Pipelines for peer wire protocol parsing.
- Frontend state management and IPC message contract strategy.
