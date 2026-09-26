#!/usr/bin/env python3
"""Rebuild the HXIME Japanese and Korean dictionaries from the pinned upstream data.

This script replaces the private build tool that produced the shipped
``Dicts/*.dict.tsv.txt`` files.  It downloads the exact upstream revisions that
``Dicts/dictionary-manifest.json`` records, verifies every input by size and
SHA-256, converts the data and writes the four dictionary files.  The generated
files are compared against the hashes in the manifest, so the script doubles as
the reproducibility check for the published dictionaries.

Usage
-----
    python Dicts/tools/build_dictionaries.py            # rebuild into Dicts/
    python Dicts/tools/build_dictionaries.py --check    # verify only, write nothing
    python Dicts/tools/build_dictionaries.py --refresh-inputs

Inputs are cached under ``<repo>/Temp/dict-cache`` (git-ignored).  Only the
converted results and the licences are distributed with the package; the source
data is downloaded from the links in ``SOURCES.md``.

Requirements (see ``conversion_packages`` in the manifest)::

    pip install pykakasi==2.3.0 korean-romanizer==0.28.0

``jaconv 0.5.0``, ``Deprecated 1.3.1`` and ``wrapt 2.4.1`` are transitive
dependencies of those two packages.

Conversion rules (also documented in ``Dicts/SOURCES.md``)
---------------------------------------------------------
Japanese, from Google Mozc ``dictionary00..09.txt``:
  * columns are reading, left id, right id, cost, surface;
  * a row is kept when the reading consists only of hiragana (U+3041..U+3096),
    katakana (U+30A1..U+30FA) and the prolonged sound mark U+30FC, and when the
    Hepburn romanisation of that reading is non-empty and contains only
    ``a-z``, ``'`` and ``-``;
  * the code is that Hepburn romanisation (pykakasi 2.3.0);
  * rows are de-duplicated by (surface, code) keeping the lowest cost;
  * the weight is ``40000 - cost``;
  * the project samples from ``japanese_sample.tsv.txt`` are added with weight
    50000 (their own weights are ignored);
  * the full dictionary is sorted by code, then weight descending, then surface;
  * the compact dictionary is the 30000 highest weights (ties by code, then
    surface) and is sorted like the full dictionary.

Korean, from the NIKL basic dictionary ``krdict/001..011.xml``:
  * characters that XML 1.0 does not allow are removed before parsing;
  * per entry only the lemma, the ``발음`` (pronunciation) forms and the first
    pronunciation of every ``활용`` (inflection) form are read;
  * a pronunciation is usable when it contains only Hangul syllables once the
    length mark U+02D0 and spaces are removed;
  * codes are the romanised lemma plus every usable pronunciation (korean
    romanizer 0.28.0), lower-cased and stripped of spaces;
  * the headword is stripped and must consist of Hangul syllables separated by
    single spaces; affixes with a hyphen, standalone jamo and non-Korean
    headwords are dropped;
  * the weight is 초급 300, 중급 200, 고급 100 and 10 for ungraded entries;
  * pairs are de-duplicated by (word, code) keeping the highest weight and the
    project samples from ``korean_sample.tsv.txt`` are added with weight 1000;
  * the full dictionary is sorted by code, then weight descending, then word;
  * the compact dictionary is the 10000 highest weights among the graded pairs
    (ties by code, then word) and is sorted like the full dictionary.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import shutil
import sys
import urllib.request
from collections import Counter
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
DICT_DIR = REPO_ROOT / "Dicts"
MANIFEST_PATH = DICT_DIR / "dictionary-manifest.json"
DEFAULT_CACHE = REPO_ROOT / "Temp" / "dict-cache"

JAPANESE_SAMPLE = DICT_DIR / "japanese_sample.tsv.txt"
KOREAN_SAMPLE = DICT_DIR / "korean_sample.tsv.txt"

# pykakasi: hiragana, katakana and the prolonged sound mark are the only allowed
# reading characters.  Iteration marks (ゝゞヽヾ), 々 and every symbol are rejected.
JAPANESE_ALLOWED = (
    {chr(code) for code in range(0x3041, 0x3097)}      # hiragana
    | {chr(code) for code in range(0x30A1, 0x30FB)}    # katakana
    | {"\u30fc"}                                       # ー
)
JAPANESE_CODE = re.compile(r"^[a-z'\-]+$")
JAPANESE_COST_BASE = 40000
JAPANESE_SAMPLE_WEIGHT = 50000
JAPANESE_COMPACT_SIZE = 30000

KOREAN_WORD = re.compile(r"^[\uac00-\ud7a3]+(?: [\uac00-\ud7a3]+)*$")
KOREAN_LEVEL_WEIGHT = {"초급": 300, "중급": 200, "고급": 100, "": 10, "없음": 10}
KOREAN_SAMPLE_WEIGHT = 1000
KOREAN_GRADED_MIN_WEIGHT = 100
KOREAN_COMPACT_SIZE = 10000

HEADERS = {
    "japanese_mozc.dict.tsv.txt": [
        "# Japanese / Mozc, filtered full dictionary",
        "# UTF-8: word<TAB>romanized code<TAB>weight",
        "# Sources, licenses and conversion details: SOURCES.md; manifest: dictionary-manifest.json",
    ],
    "japanese_mozc_common.dict.tsv.txt": [
        "# Japanese / Mozc, top 30000 by inverse cost",
        "# UTF-8: word<TAB>romanized code<TAB>weight",
        "# Sources, licenses and conversion details: SOURCES.md; manifest: dictionary-manifest.json",
    ],
    "korean_nikl.dict.tsv.txt": [
        "# Korean / NIKL Basic Korean Dictionary, filtered full dictionary",
        "# UTF-8: word<TAB>romanized code<TAB>weight",
        "# Sources, licenses and conversion details: SOURCES.md; manifest: dictionary-manifest.json",
    ],
    "korean_nikl_common.dict.tsv.txt": [
        "# Korean / NIKL, top 10000 graded entries",
        "# UTF-8: word<TAB>romanized code<TAB>weight",
        "# Sources, licenses and conversion details: SOURCES.md; manifest: dictionary-manifest.json",
    ],
}


def log(message):
    print(message, flush=True)


def sha256_of(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for block in iter(lambda: handle.read(1 << 20), b""):
            digest.update(block)
    return digest.hexdigest()


# --------------------------------------------------------------------------- #
# inputs
# --------------------------------------------------------------------------- #

def load_manifest():
    with open(MANIFEST_PATH, encoding="utf-8") as handle:
        return json.load(handle)


def source_entries(manifest, prefix):
    return [entry for entry in manifest["sources"] if entry["file"].startswith(prefix)]


def fetch_inputs(manifest, cache_dir, refresh):
    cache_dir.mkdir(parents=True, exist_ok=True)
    for entry in manifest["sources"]:
        target = cache_dir / entry["file"]
        if target.exists() and not refresh:
            if (target.stat().st_size == entry["bytes"]
                    and sha256_of(target) == entry["sha256"]):
                continue
            log("  re-downloading %s (cached copy does not match the manifest)" % entry["file"])
        else:
            log("  downloading %s" % entry["file"])
        request = urllib.request.Request(entry["url"], headers={"User-Agent": "hxime-dict-build"})
        temp = target.with_suffix(target.suffix + ".part")
        with urllib.request.urlopen(request) as response, open(temp, "wb") as handle:
            shutil.copyfileobj(response, handle, 1 << 20)
        size = temp.stat().st_size
        digest = sha256_of(temp)
        if size != entry["bytes"] or digest != entry["sha256"]:
            temp.unlink()
            raise SystemExit("input %s does not match the manifest (size %d/%d, sha256 %s/%s)"
                             % (entry["file"], size, entry["bytes"], digest, entry["sha256"]))
        os.replace(temp, target)
    return cache_dir


def verify_cached_inputs(manifest, cache_dir):
    problems = []
    for entry in manifest["sources"]:
        target = cache_dir / entry["file"]
        if not target.exists():
            problems.append("%s missing" % entry["file"])
        elif target.stat().st_size != entry["bytes"] or sha256_of(target) != entry["sha256"]:
            problems.append("%s does not match the manifest" % entry["file"])
    if problems:
        raise SystemExit("cached inputs are not usable: " + "; ".join(problems))
    log("  all %d cached inputs match the manifest" % len(manifest["sources"]))


def check_dependency_versions(manifest, strict):
    expected = manifest.get("conversion_packages", {})
    actual = {}
    try:
        from importlib.metadata import PackageNotFoundError, version
    except ImportError:  # pragma: no cover - Python < 3.8
        return
    for package in ("pykakasi", "korean-romanizer", "jaconv", "Deprecated", "wrapt"):
        try:
            actual[package] = version(package)
        except PackageNotFoundError:
            actual[package] = None
    problems = []
    for package, wanted in expected.items():
        got = actual.get(package)
        if got == wanted:
            continue
        problems.append("%s %s (expected %s)" % (package, got or "missing", wanted))
    if problems:
        message = "dependency version mismatch: " + ", ".join(problems)
        if strict:
            raise SystemExit(message)
        log("WARNING: %s" % message)
        log("WARNING: the output may differ from the published SHA-256 values.")
    else:
        log("dependency versions match conversion_packages: %s"
            % ", ".join("%s %s" % (k, v) for k, v in sorted(actual.items())))


# --------------------------------------------------------------------------- #
# japanese
# --------------------------------------------------------------------------- #

def japanese_weight(cost):
    return max(1, JAPANESE_COST_BASE - cost)


def build_japanese(cache_dir, manifest, sample_path):
    from pykakasi import kakasi

    converter = kakasi()
    romanised = {}
    pairs = {}
    filtered = 0
    skipped = 0
    source_rows = 0

    for entry in sorted(source_entries(manifest, "mozc-dictionary"),
                        key=lambda item: item["file"]):
        path = cache_dir / entry["file"]
        with open(path, encoding="utf-8") as handle:
            for line in handle:
                line = line.rstrip("\n")
                if not line:
                    continue
                columns = line.split("\t")
                if len(columns) not in (5, 6):
                    skipped += 1
                    continue
                source_rows += 1
                reading = columns[0]
                if any(char not in JAPANESE_ALLOWED for char in reading):
                    filtered += 1
                    continue
                code = romanised.get(reading)
                if code is None:
                    code = "".join(item["hepburn"] for item in converter.convert(reading))
                    romanised[reading] = code
                if not code or not JAPANESE_CODE.match(code):
                    filtered += 1
                    continue
                key = (columns[4], code)
                cost = int(columns[3])
                previous = pairs.get(key)
                if previous is None or cost < previous:
                    pairs[key] = cost

    log("Japanese: %d source rows, %d filtered, %d skipped, %d unique (surface, code) pairs"
        % (source_rows, filtered, skipped, len(pairs)))
    rows = {}
    for (word, code), cost in pairs.items():
        rows[(word, code)] = japanese_weight(cost)
    for word, code, _weight in read_rows(sample_path):
        key = (word, code)
        if rows.get(key, 0) < JAPANESE_SAMPLE_WEIGHT:
            rows[key] = JAPANESE_SAMPLE_WEIGHT

    full = sorted(((word, code, weight) for (word, code), weight in rows.items()),
                  key=lambda row: (row[1], -row[2], row[0]))
    compact = sorted(rows.items(), key=lambda item: (-item[1], item[0][1], item[0][0]))
    compact = [(word, code, weight) for (word, code), weight in compact[:JAPANESE_COMPACT_SIZE]]
    compact.sort(key=lambda row: (row[1], -row[2], row[0]))
    statistics = {"source_rows": source_rows, "filtered_rows": filtered}
    return ({"japanese_mozc.dict.tsv.txt": full,
             "japanese_mozc_common.dict.tsv.txt": compact}, statistics)


# --------------------------------------------------------------------------- #
# korean
# --------------------------------------------------------------------------- #

KOREAN_ENTRY = re.compile(r"<LexicalEntry\b.*?</LexicalEntry>", re.S)
KOREAN_LEMMA = re.compile(r'<Lemma>\s*<feat att="writtenForm" val="([^"]*)"', re.S)
KOREAN_LEVEL = re.compile(r'<feat att="vocabularyLevel" val="([^"]*)"\s*/>')
KOREAN_WORDFORM = re.compile(r"<WordForm>(.*?)</WordForm>", re.S)
KOREAN_FORM_REPRESENTATION = re.compile(r"<FormRepresentation>.*?</FormRepresentation>", re.S)
KOREAN_FORM_TYPE = re.compile(r'<feat att="type" val="([^"]*)"\s*/>')
KOREAN_PRONUNCIATION = re.compile(r'<feat att="pronunciation" val="([^"]*)"\s*/>')


def korean_pronunciation(text):
    """Return the romanisable form of a pronunciation, or None when unusable."""
    cleaned = text.replace("\u02d0", "").replace(" ", "")
    if not cleaned or not all("\uac00" <= char <= "\ud7a3" for char in cleaned):
        return None
    return cleaned


def read_korean_entries(cache_dir, manifest):
    entries = []
    invalid_characters = 0
    for source in sorted(source_entries(manifest, "nikl-"), key=lambda item: item["file"]):
        if not source["file"].endswith(".xml"):
            continue
        with open(cache_dir / source["file"], encoding="utf-8") as handle:
            raw = handle.read()
        kept = []
        for char in raw:
            if ord(char) < 0x20 and char not in "\t\n\r":
                invalid_characters += 1
            else:
                kept.append(char)
        raw = "".join(kept)
        for entry in KOREAN_ENTRY.findall(raw):
            lemma = KOREAN_LEMMA.search(entry)
            if lemma is None:
                continue
            level = KOREAN_LEVEL.search(entry)
            base = []
            inflections = []
            for block in KOREAN_WORDFORM.findall(entry):
                direct = KOREAN_FORM_REPRESENTATION.sub("", block)
                types = KOREAN_FORM_TYPE.findall(direct)
                prons = KOREAN_PRONUNCIATION.findall(direct)
                if not types or not prons:
                    continue
                if types[0] == "발음":
                    base.extend(prons)
                elif types[0] == "활용":
                    inflections.append(prons[0])
            entries.append((lemma.group(1), base, inflections,
                            level.group(1) if level else ""))
    return entries, invalid_characters


def build_korean(cache_dir, manifest, sample_path):
    from korean_romanizer.romanizer import Romanizer

    romanised = {}

    def romanize(text):
        cached = romanised.get(text)
        if cached is None:
            cached = Romanizer(text).romanize().lower().replace(" ", "")
            romanised[text] = cached
        return cached

    entries, invalid_characters = read_korean_entries(cache_dir, manifest)
    log("Korean: %d source entries, %d invalid XML characters removed"
        % (len(entries), invalid_characters))

    filtered = 0
    pairs = {}
    for raw_word, base, inflections, level in entries:
        word = raw_word.strip()
        if not KOREAN_WORD.match(word):
            filtered += 1
            continue
        weight = KOREAN_LEVEL_WEIGHT.get(level, 10)
        codes = [romanize(word)]
        for pronunciation in base:
            cleaned = korean_pronunciation(pronunciation)
            if cleaned:
                codes.append(romanize(cleaned))
        for pronunciation in inflections:
            cleaned = korean_pronunciation(pronunciation)
            if cleaned:
                codes.append(romanize(cleaned))
        for code in codes:
            if not JAPANESE_CODE.match(code):
                continue
            key = (word, code)
            previous = pairs.get(key)
            if previous is None or weight > previous:
                pairs[key] = weight

    log("Korean: %d entries filtered, %d unique (word, code) pairs" % (filtered, len(pairs)))
    rows = dict(pairs)
    for word, code, _weight in read_rows(sample_path):
        key = (word, code)
        if rows.get(key, 0) < KOREAN_SAMPLE_WEIGHT:
            rows[key] = KOREAN_SAMPLE_WEIGHT

    full = sorted(((word, code, weight) for (word, code), weight in rows.items()),
                  key=lambda row: (row[1], -row[2], row[0]))
    graded = sorted(((word, code, weight) for (word, code), weight in rows.items()
                     if weight >= KOREAN_GRADED_MIN_WEIGHT),
                    key=lambda row: (-row[2], row[1], row[0]))
    compact = graded[:KOREAN_COMPACT_SIZE]
    compact.sort(key=lambda row: (row[1], -row[2], row[0]))
    statistics = {"source_entries": len(entries),
                  "filtered_entries": filtered,
                  "removed_invalid_xml_characters": invalid_characters}
    return ({"korean_nikl.dict.tsv.txt": full,
             "korean_nikl_common.dict.tsv.txt": compact}, statistics)


# --------------------------------------------------------------------------- #
# shared helpers
# --------------------------------------------------------------------------- #

def read_rows(path):
    with open(path, encoding="utf-8") as handle:
        for line in handle:
            if line.startswith("#") or not line.strip():
                continue
            word, code, weight = line.rstrip("\n").split("\t")
            yield word, code, int(weight)


def write_dictionary(path, header, rows):
    """Write the dictionary atomically as UTF-8 with LF line endings."""
    path.parent.mkdir(parents=True, exist_ok=True)
    temp = path.with_name(path.name + ".tmp")
    with open(temp, "w", encoding="utf-8", newline="\n") as handle:
        for line in header:
            handle.write(line + "\n")
        for word, code, weight in rows:
            handle.write("%s\t%s\t%d\n" % (word, code, weight))
    os.replace(temp, path)


def verify(manifest, out_dir):
    failures = 0
    for name, expected in manifest["outputs"].items():
        path = out_dir / name
        if not path.exists():
            log("  %-34s MISSING" % name)
            failures += 1
            continue
        digest = sha256_of(path)
        size = path.stat().st_size
        with open(path, encoding="utf-8") as handle:
            entries = sum(1 for line in handle if not line.startswith("#"))
        ok = (digest == expected["sha256"] and size == expected["bytes"]
              and entries == expected["entries"])
        log("  %-34s %s  entries=%d bytes=%d sha256=%s"
            % (name, "PASS" if ok else "FAIL", entries, size, digest[:16] + "..."))
        failures += 0 if ok else 1
    return failures


def compare_statistics(manifest, measured):
    """Cross-check the counters reported by the build against manifest statistics."""
    expected = manifest.get("statistics", {})
    failures = 0
    for language, values in sorted(measured.items()):
        for key, value in sorted(values.items()):
            wanted = expected.get(language, {}).get(key)
            if wanted == value:
                log("  statistics %s.%s = %d" % (language, key, value))
            else:
                log("  statistics %s.%s = %s (manifest records %s) MISMATCH"
                    % (language, key, value, wanted))
                failures += 1
    return failures


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--check", action="store_true",
                        help="build into a temporary directory and verify only")
    parser.add_argument("--out-dir", type=Path, default=None,
                        help="directory for the generated files (default: Dicts/)")
    parser.add_argument("--cache-dir", type=Path, default=DEFAULT_CACHE,
                        help="where the upstream inputs are cached (default: Temp/dict-cache)")
    parser.add_argument("--refresh-inputs", action="store_true",
                        help="re-download the inputs even when the cache matches")
    parser.add_argument("--skip-download", action="store_true",
                        help="fail instead of downloading missing inputs")
    parser.add_argument("--strict-versions", action="store_true",
                        help="treat a conversion library version mismatch as an error")
    args = parser.parse_args(argv)

    manifest = load_manifest()
    log("HXIME dictionary build")
    log("  repo       : %s" % REPO_ROOT)
    log("  cache      : %s" % args.cache_dir)
    check_dependency_versions(manifest, args.strict_versions)

    if args.skip_download:
        log("using cached inputs")
        verify_cached_inputs(manifest, args.cache_dir)
    else:
        log("verifying upstream inputs")
        fetch_inputs(manifest, args.cache_dir, args.refresh_inputs)

    japanese, japanese_statistics = build_japanese(args.cache_dir, manifest, JAPANESE_SAMPLE)
    korean, korean_statistics = build_korean(args.cache_dir, manifest, KOREAN_SAMPLE)
    results = {}
    results.update(japanese)
    results.update(korean)

    statistics_failures = compare_statistics(manifest, {"japanese": japanese_statistics,
                                                        "korean": korean_statistics})
    if statistics_failures:
        log("WARNING: %d statistics value(s) differ from the manifest" % statistics_failures)

    out_dir = args.out_dir
    if out_dir is None:
        out_dir = (REPO_ROOT / "Temp" / "dict-build") if args.check else DICT_DIR
    out_dir.mkdir(parents=True, exist_ok=True)

    for name, rows in sorted(results.items()):
        write_dictionary(out_dir / name, HEADERS[name], rows)
        log("wrote %s (%d rows)" % (out_dir / name, len(rows)))

    log("verifying against %s" % MANIFEST_PATH.name)
    failures = verify(manifest, out_dir)
    if failures:
        log("%d file(s) do not match the manifest" % failures)
        return 1
    log("all dictionaries match the manifest")
    return 0


if __name__ == "__main__":
    sys.exit(main())
