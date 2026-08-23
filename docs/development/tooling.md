# Development Tooling

This document describes repository-level tooling conventions that keep project configuration consistent across the solution.

The goal is not to make the project overly strict. The goal is to keep formatting, package versions, and common build settings predictable for anyone contributing to the codebase.

## Editor Configuration

The repository uses `.editorconfig` to keep code style and formatting consistent across the solution.

The editor configuration standardizes:

- whitespace, indentation, line endings, encoding, and trailing whitespace
- namespace and using placement
- brace, constructor, expression, and language-style preferences
- explicit var usage rules
- pattern-matching preferences
- accessibility modifiers and readonly field enforcement
- naming conventions for interfaces, types, members, fields, and constants
- analyzer severity for selected compiler and code-quality rules
- file-specific conventions such as XML indentation and LF line endings for shell scripts

Because the style rules are codified in `.editorconfig`, the project does not depend on each contributor remembering the same formatting preferences manually. Editors and IDEs can apply the rules automatically.

That makes the code look uniform across layers, keeps repeated patterns easier to scan, and keeps style disagreements out of the code itself.

Several rules are intentionally strict and promote style violations to errors rather than warnings. This reflects my own (the author's 'Ahemd Fawzi') current development preference and is still being experimented with.
In a team environment, these rules would typically be reviewed collectively, relaxed where appropriate, and treated as an agreed codebase convention.

## Centralized Package Management

The repository uses centralized package management through `Directory.Packages.props`, so dependency versions are defined once and reused across the solution.

This keeps package control predictable in a multi-assembly solution, where the same dependency may be used by different assemblies.

Centralized package management reduces:

- version drift between projects
- repeated version declarations
- accidental mismatches across layers

It also makes upgrades easier because package version changes are made in one place instead of across many project files.

## Centralized Build Props

The repository uses `Directory.Build.props` for build settings that should apply consistently across projects.

Current shared build properties include:

- target framework
- nullable reference type behavior
- implicit using behavior

Keeping these settings centralized prevents individual project files from drifting away from the solution baseline. Project files can stay focused on project-specific references and behavior, while common build assumptions remain visible at the repository root.
