#!/usr/bin/env python3

from __future__ import annotations

import argparse
import json
import re
import subprocess
from dataclasses import dataclass
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CLASS_SKILLS = ROOT / "class_skills.md"
SPELL_EVIDENCE = ROOT / "Source" / "NexusForever.Tests" / "SpellEvidenceTests.cs"
REPO = "HourGlss/XyNexusForeverF"


@dataclass(frozen=True)
class SkillRow:
    checked: bool
    class_name: str
    skill_name: str
    rank: str | None


@dataclass(frozen=True)
class IssueRef:
    number: int
    state: str
    title: str
    url: str
    body: str


def run_gh(*args: str) -> str:
    proc = subprocess.run(
        ["gh", *args],
        cwd=ROOT,
        check=True,
        text=True,
        capture_output=True,
    )
    return proc.stdout


def parse_skill_rows() -> list[SkillRow]:
    rows: list[SkillRow] = []
    pattern = re.compile(r"- \[(?P<checked>[ x])\] (?P<class>[^ ]+) (?P<skill>.+?)(?: - (?P<rank>[012]))?$")

    for raw_line in CLASS_SKILLS.read_text().splitlines():
        match = pattern.fullmatch(raw_line)
        if match is None:
            continue

        rows.append(
            SkillRow(
                checked=match.group("checked") == "x",
                class_name=match.group("class"),
                skill_name=match.group("skill"),
                rank=match.group("rank"),
            )
        )

    return rows


def parse_issue_refs() -> list[IssueRef]:
    data = json.loads(
        run_gh(
            "issue",
            "list",
            "--repo",
            REPO,
            "--state",
            "all",
            "--limit",
            "500",
            "--json",
            "number,title,state,url,body",
        )
    )

    return [
        IssueRef(
            number=entry["number"],
            state=entry["state"],
            title=entry["title"],
            url=entry["url"],
            body=entry.get("body") or "",
        )
        for entry in data
    ]


def parse_spell_evidence_rows() -> dict[tuple[str, str], list[str]]:
    text = SPELL_EVIDENCE.read_text()
    rows: dict[tuple[str, str], list[str]] = {}

    for block_name in ("ClassScoreOneSkillEffectTypeData", "ClassScoreTwoSkillEffectTypeData"):
        match = re.search(block_name + r' = """\n(.*?)\n""";', text, re.S)
        if match is None:
            raise ValueError(f"Unable to find evidence block {block_name}")

        for raw_row in match.group(1).splitlines():
            class_name, skill_name, effect_csv = raw_row.split("|")
            rows[(class_name, skill_name)] = [value.strip() for value in effect_csv.split(",") if value.strip()]

    return rows


def normalize(value: str) -> str:
    value = value.lower().replace("–", "-").replace("—", "-")
    value = re.sub(r"\[.*?\]", " ", value)
    value = re.sub(r"[^a-z0-9:]+", " ", value)
    return " ".join(value.split())


def parse_issue_body_field(body: str, field_name: str) -> str | None:
    match = re.search(rf"### {re.escape(field_name)}\n\n(.+?)(?:\n\n### |\Z)", body, re.S)
    if match is None:
        return None
    return match.group(1).strip()


def issue_matches_skill(issue: IssueRef, row: SkillRow) -> bool:
    class_token = normalize(row.class_name)
    skill_token = normalize(row.skill_name)
    title = normalize(issue.title)

    if class_token in title and skill_token in title:
        return True

    body_class = parse_issue_body_field(issue.body, "Class Name")
    body_skill = parse_issue_body_field(issue.body, "Spell Name")
    if body_class == row.class_name and body_skill == row.skill_name:
        return True

    if row.class_name == "Engineer" and row.skill_name == "Mortar Strike":
        return class_token in title and "moltar strike" in title

    return False


def build_issue_map(rows: list[SkillRow], issues: list[IssueRef]) -> list[tuple[SkillRow, list[IssueRef]]]:
    result: list[tuple[SkillRow, list[IssueRef]]] = []

    for row in rows:
        matches = [issue for issue in issues if issue_matches_skill(issue, row)]
        matches.sort(key=lambda issue: issue.number)
        result.append((row, matches))

    return result


def render_map(issue_map: list[tuple[SkillRow, list[IssueRef]]], evidence: dict[tuple[str, str], list[str]]) -> str:
    lines: list[str] = []
    lines.append("# Class Skills Issue Map")
    lines.append("")
    lines.append("Generated from `class_skills.md`, `SpellEvidenceTests.cs`, and live GitHub issues.")
    lines.append("")
    lines.append("- `tracker` is the status currently written in `class_skills.md`.")
    lines.append("- `issue anchor` lists the matching GitHub issue(s) for that skill.")
    lines.append("- `closed anchor` means at least one matching issue is closed.")
    lines.append("- `open-only anchor` means the skill is tracked on GitHub but not yet backed by a closed issue.")
    lines.append("")

    current_class: str | None = None
    for row, matches in issue_map:
        if row.class_name != current_class:
            current_class = row.class_name
            lines.append(f"## {current_class}")
            lines.append("")

        tracker = "[x]" if row.checked else row.rank or "?"
        if matches:
            refs = ", ".join(f"#{issue.number} ({issue.state.lower()})" for issue in matches)
            closed_anchor = "yes" if any(issue.state == "CLOSED" for issue in matches) else "no"
        else:
            refs = "missing"
            closed_anchor = "no"

        effects = evidence.get((row.class_name, row.skill_name))
        effect_note = f" | effects: {', '.join(effects)}" if effects else ""
        lines.append(
            f"- `{row.class_name} {row.skill_name}` | tracker: `{tracker}` | issue anchor: {refs} | closed anchor: `{closed_anchor}`{effect_note}"
        )

        if row.checked and matches and not any(issue.state == "CLOSED" for issue in matches):
            lines.append("  - note: tracker is `[x]`, but the GitHub anchor is still open-only.")

    lines.append("")
    return "\n".join(lines)


def main() -> None:
    parser = argparse.ArgumentParser(description="Audit class skill tracker coverage against GitHub issues.")
    parser.add_argument("--write-map", action="store_true", help="Write class_skills_issue_map.md")
    args = parser.parse_args()

    rows = parse_skill_rows()
    issues = parse_issue_refs()
    evidence = parse_spell_evidence_rows()
    issue_map = build_issue_map(rows, issues)

    covered = sum(1 for _, matches in issue_map if matches)
    closed = sum(1 for _, matches in issue_map if any(issue.state == "CLOSED" for issue in matches))
    missing = len(issue_map) - covered
    open_only_x = sum(1 for row, matches in issue_map if row.checked and matches and not any(issue.state == "CLOSED" for issue in matches))

    print(f"skills: {len(issue_map)}")
    print(f"covered: {covered}")
    print(f"closed anchor: {closed}")
    print(f"missing: {missing}")
    print(f"[x] with open-only anchor: {open_only_x}")

    if args.write_map:
        (ROOT / "class_skills_issue_map.md").write_text(render_map(issue_map, evidence) + "\n")


if __name__ == "__main__":
    main()
