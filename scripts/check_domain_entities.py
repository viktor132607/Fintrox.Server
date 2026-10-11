"""Verify the complete module-entity inventory and executable test coverage references."""
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]


def validate(root=ROOT):
    errors = []
    entries = json.loads((root / 'architecture/domain-entities.json').read_text())['entities']
    modules = {m['name'] for m in json.loads((root / 'architecture/modules.json').read_text())['modules']}
    paths = [e['path'] for e in entries]
    actual = {p.relative_to(root).as_posix() for p in (root / 'src/Modules').glob('*/Fintrox.Modules.*.Domain/Entities/*.cs')}
    if len(paths) != len(set(paths)) or set(paths) != actual:
        errors.append('Entity inventory has duplicates, missing files or unlisted source files')
    if {e['module'] for e in entries} != modules:
        errors.append('Every module must have an inventoried domain model')
    test_project = root / 'tests/Fintrox.Domain.Tests/Fintrox.Domain.Tests.csproj'
    refs = {(test_project.parent / e.attrib['Include'].replace('\\', '/')).resolve()
            for e in ET.parse(test_project).iter('ProjectReference')}
    for entry in entries:
        path = root / entry['path']
        if not path.exists():
            continue
        source = path.read_text()
        module, entity = entry['module'], entry['entity']
        namespace = f'Fintrox.Modules.{module}.Domain.Entities'
        if f'namespace {namespace};' not in source or f'public sealed class {entity} : OrganizationEntity' not in source:
            errors.append(f'Wrong ownership/declaration: {path}')
        if re.search(r'\b(float|double)\b|public\s+set\s*;', source):
            errors.append(f'Unsafe numeric type or public mutation: {path}')
        project = path.parent.parent / f'Fintrox.Modules.{module}.Domain.csproj'
        if project.resolve() not in refs:
            errors.append(f'Missing test reference: {module}')
    return errors, len(modules), len(entries)


if __name__ == '__main__':
    failures, module_count, entity_count = validate()
    if failures:
        raise SystemExit('\n'.join(failures))
    print(f'Domain inventory passed: {entity_count} entities across {module_count} modules.')
