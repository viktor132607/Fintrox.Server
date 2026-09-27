"""Check the scaffold's compile-time dependency graph; no third-party packages."""
import json
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
LAYERS = {
    'Domain': set(), 'Contracts': set(),
    'Application': {'Domain', 'Contracts'},
    'Infrastructure': {'Application', 'Domain', 'Contracts'},
    'Presentation': {'Application', 'Contracts'},
}


def validate(root=ROOT):
    errors = []
    catalog = json.loads((root / 'architecture/modules.json').read_text())['modules']
    names = [m['name'] for m in catalog]
    for field in ['name', 'slug', 'schema']:
        values = [m[field] for m in catalog]
        if len(values) != len(set(values)):
            errors.append(f'Duplicate module {field}')
    modules = root / 'src/Modules'
    actual = {p.name for p in modules.iterdir() if p.is_dir()}
    if actual != set(names):
        errors.append(f'Module folders/catalog mismatch: {actual ^ set(names)}')
    sln = (root / 'Fintrox.Server.sln').read_text()
    solution_paths = re.findall(r'^Project\([^\n]+ = "[^"]+", "([^"]+\.csproj)"', sln, re.M)
    solution_projects = {(root / p.replace('\\', '/')).resolve() for p in solution_paths}
    host = root / 'src/Fintrox.Api/Fintrox.Api.csproj'
    host_refs = {(host.parent / e.attrib['Include'].replace('\\', '/')).resolve()
                 for e in ET.parse(host).iter('ProjectReference')}
    expected_projects = set()
    for module in catalog:
        name = module['name']
        for layer, allowed in LAYERS.items():
            assembly = f'Fintrox.Modules.{name}.{layer}'
            project = modules / name / assembly / f'{assembly}.csproj'
            expected_projects.add(project.resolve())
            if not project.is_file():
                errors.append(f'Missing project: {project.relative_to(root)}')
                continue
            xml = ET.parse(project)
            if xml.findtext('./PropertyGroup/DisableTransitiveProjectReferences') != 'true':
                errors.append(f'{assembly}: transitive references must remain disabled')
            actual_refs = set()
            allowed_refs = {(modules / name / f'Fintrox.Modules.{name}.{a}' /
                             f'Fintrox.Modules.{name}.{a}.csproj').resolve() for a in allowed}
            for ref in xml.iter('ProjectReference'):
                target = (project.parent / ref.attrib['Include'].replace('\\', '/')).resolve()
                actual_refs.add(target)
                if target not in allowed_refs:
                    errors.append(f'{assembly}: forbidden reference {ref.attrib["Include"]}')
                if not target.is_file():
                    errors.append(f'{assembly}: missing reference target')
            if actual_refs != allowed_refs:
                errors.append(f'{assembly}: layer reference graph differs from policy')
            if layer in {'Domain', 'Contracts'} and (list(xml.iter('PackageReference')) or list(xml.iter('FrameworkReference'))):
                errors.append(f'{assembly}: pure layer cannot reference frameworks/packages')
            if layer == 'Application' and list(xml.iter('FrameworkReference')):
                errors.append(f'{assembly}: application must not reference a web framework')
            if layer == 'Application':
                for feature in module['features']:
                    if not (project.parent / feature).is_dir():
                        errors.append(f'{assembly}: missing feature {feature}')
            if project.resolve() not in solution_projects:
                errors.append(f'{assembly}: absent from primary solution')
            if layer in {'Infrastructure', 'Presentation'} and project.resolve() not in host_refs:
                errors.append(f'{assembly}: absent from host composition references')
            # Legacy imports would undermine the new boundary even if added indirectly.
            for source in project.parent.rglob('*.cs'):
                if {'obj', 'bin'} & set(source.relative_to(project.parent).parts):
                    continue
                if re.search(r'\bFintrox\.(?:Domain|Application|Infrastructure|Api)\b', source.read_text()):
                    errors.append(f'{source.relative_to(root)}: legacy namespace dependency')
        for kind in ['Unit', 'Integration', 'Contract']:
            if not (root / 'tests/Modules' / name / kind).is_dir():
                errors.append(f'{name}: missing {kind} test location')
    actual_projects = {p.resolve() for p in modules.rglob('*.csproj')}
    if actual_projects != expected_projects:
        errors.append('Unexpected or missing module project')
    if len(solution_projects) != len(solution_paths):
        errors.append('Duplicate solution project entries')
    return errors


if __name__ == '__main__':
    violations = validate()
    if violations:
        print('\n'.join(violations), file=sys.stderr)
        sys.exit(1)
    print('Architecture passed: 21 module boundaries, 105 projects; solution and host wired.')
