# Localization checker
# Usage: run with Python 3.8+ from repo root
# Scans for L.T("key") usages and compares against keys defined in Services/LocalizationService.cs

import re
from pathlib import Path

root = Path('.').resolve()
service_file = root / 'Services' / 'LocalizationService.cs'

lt_regex = re.compile(r'L\.T\("([A-Za-z0-9_.-]+)"\)')
key_def_regex = re.compile(r"\[\"([A-Za-z0-9_.-]+)\"\]\s*=\s*\")

# gather referenced keys
refs = set()
for p in root.rglob('*.cshtml'):
    try:
        txt = p.read_text(encoding='utf-8')
    except Exception:
        continue
    for m in lt_regex.finditer(txt):
        refs.add(m.group(1))
for p in root.rglob('*.cs'):
    # skip designer/obj folders
    if 'obj' in p.parts or 'bin' in p.parts:
        continue
    try:
        txt = p.read_text(encoding='utf-8')
    except Exception:
        continue
    for m in lt_regex.finditer(txt):
        refs.add(m.group(1))

# gather defined keys for sq and en
sq_keys = set()
en_keys = set()
if service_file.exists():
    txt = service_file.read_text(encoding='utf-8')
    # crude split by _translations["sq"] and _translations["en"] blocks
    sq_block = re.search(r'_translations\["sq"\]\s*=\s*new Dictionary<string, string>\s*\{([\s\S]*?)\};', txt)
    en_block = re.search(r'_translations\["en"\]\s*=\s*new Dictionary<string, string>\s*\{([\s\S]*?)\};', txt)
    if sq_block:
        for m in key_def_regex.finditer(sq_block.group(1)):
            sq_keys.add(m.group(1))
    if en_block:
        for m in key_def_regex.finditer(en_block.group(1)):
            en_keys.add(m.group(1))

# compare
missing_in_sq = sorted([k for k in refs if k not in sq_keys])
missing_in_en = sorted([k for k in refs if k not in en_keys])

out = []
out.append(f"Total L.T() refs found: {len(refs)}")
out.append(f"Defined keys (sq): {len(sq_keys)}")
out.append(f"Defined keys (en): {len(en_keys)}")
out.append('\nMissing in sq:')
for k in missing_in_sq:
    out.append('  ' + k)
out.append('\nMissing in en:')
for k in missing_in_en:
    out.append('  ' + k)

report = '\n'.join(out)
print(report)

# write a report file
(report_path := root / 'tools' / 'localization_report.txt').write_text(report, encoding='utf-8')
print(f"Wrote report to {report_path}")
