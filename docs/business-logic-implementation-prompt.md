# Fintrox — промпт за цялата бизнес логика

Работиш по Fintrox — централизирана счетоводна/ERP платформа, използваема самостоятелно и от други мои приложения чрез активируема API интеграция.

Репозитории:
- https://github.com/viktor132607/Fintrox.Server
- https://github.com/viktor132607/Fintrox.Client

Целта е финансова коректност и модулност от ERP клас и използваемост от типа на Xero. Това е продуктова цел, не твърдение, че текущият проект вече е равностоен на SAP/Xero.

## 1. Как да започнеш

Прочети актуалния main на двете репота, приложимите AGENTS.md, README, docs/architecture.md, docs/activation-experience.md, architecture/modules.json, architecture/experience-integration.json и архитектурните проверки. Работи по реалния код, не само по описанията.

Първата задача е рекурсивен baseline и migration matrix: всеки endpoint, entity, service, repository, migration, background worker, integration contract, FE route и тест. За всяка функционалност запиши: текущ файл, собственик в новата архитектура, зависимости, налични тестове, реален статус, пропуски и критерии за приемане. Никакви представителни извадки. Отбелязвай „готово“ само при работеща имплементация с доказателства.

Структурният етап добавя 23 модула: първоначалните 21 бизнес модула плюс Capabilities и Experience. Новите папки и assemblies НЕ означават, че бизнес логиката е прехвърлена или имплементирана. Integrations е разширен структурно за външни приложения и инсталации. SDK папките са само резервирани. Каталогът съдържа reserved feature IDs; widgets и default layout списъците са празни по замисъл.

В backend има започната реална логика в старите хоризонтални проекти. Провери и запази поведението ѝ: authentication, organizations/permissions, auditing, COA, fiscal calendar, journals/posting/reversals, counterparties, reports, VAT/currencies/rates, sales, purchases/expenses, payments/allocations, auto-posting, integration clients/scopes, idempotency/external references, inbox/outbox/webhooks/retries. Не я пишѝ повторно от нулата и не приемай историческото „21/41“ като доказателство за текуща завършеност.

FE вече е Next.js + strict TypeScript. Старият Blazor/.NET frontend е премахнат в отделен етап: НЕ го възстановявай. Проверявай текущия код преди всяка промяна.

## 2. Задължителни технически граници

- ASP.NET Core Web API / .NET 10 според текущия repo baseline; Next.js App Router + TypeScript; PostgreSQL / EF Core / Npgsql. Не прави несвързани version upgrades.
- Един модулен монолит и отделен FE. Пази възможността за бъдещо отделяне на API; не въвеждай microservices предварително.
- Всеки модул има Domain, Application, Contracts, Infrastructure, Presentation. Domain е чист; use cases са в Application; persistence/adapters са в Infrastructure; HTTP mapping е в Presentation; API host е composition root.
- Използвай вертикални функционални области вътре в модулите. Не създавай огромни универсални services, repositories или common business модели.
- Чужд модул се използва през одобрен versioned Contract/port. Няма директно използване на чужди entities, repositories, DbContext, private classes или SQL таблици.
- Всеки модул притежава своя бъдеща схема, DbContext и migration history. Споделеният legacy DbContext остава временна зависимост до контролирана миграция.
- Не изтривай/преномерирай приложени миграции. Не пресъздавай базата. Подготви backfill, сверка и rollback за промени в собствеността на данните.
- В монолита използвай in-process adapters. HTTP/message adapters се въвеждат при реална нужда; без loopback HTTP между локални модули.
- Финансовата транзакционна граница е изрично определена. Не заменяй сегашен ACID flow с ненадеждно publish-after-save. Междумодулна асинхронност изисква durable state, outbox/inbox, idempotency, компенсация и reconciliation.
- Tenant/organization, actor, correlation и causation контекст се пренасят и проверяват. Header или подаден organizationId сами по себе си не доказват достъп.
- FE не притежава бизнес сметки, данъчни правила или permissions. Използвай генериран типизиран API client, strict TS, allowJs:false. Не връщай .js/.jsx приложен код.
- Един логически API base URL и модулни FE adapters. Бъдещ gateway не трябва да изисква пренаписване на бизнес компонентите.
- Не генерирай изображения. Не променяй чужди проекти или deployment без конкретен обхват за това.

## 3. Финансови инварианти

За всяка приложима функционалност предвиди и тествай:

1. Дебит = кредит в изискваната ledger/base currency; използвай decimal и явни precision/scale/rounding правила. Не използвай float/double за пари.
2. Валутата на документа, базовата валута и отчетната валута имат ясни роли. Съхранявай rate/date/source и snapshot на приложените правила.
3. Осчетоводени записи и издадени финансови документи не се редактират или трият произволно. Корекциите са чрез проследими reversals/credit/debit документи.
4. Заключени периоди, numbering, документни преходи, concurrent writes и повторени заявки не могат да нарушат финансовото състояние.
5. Всички references, unique constraints и разрешения са organization scoped; тестовете включват опити за достъп до друга организация.
6. Settlements/allocations не надвишават разрешените остатъци, включително при concurrent заявки и различни валути.
7. Audit и връзките source document → journal → settlement → report остават проследими. Audit е append-only.
8. Финансовите отчети се сверяват с ledger/subledger; няма неотчетени дублирани събития, плащания или stock postings.
9. Правилата имат дата на действие и jurisdiction, когато е приложимо. Проверявай актуалните официални нормативни източници при реално имплементиране на данъчни/payroll/localization правила; не измисляй ставки и срокове.
10. Датата на счетоводния документ и UTC timestamp на събитието са отделни понятия; не допускай неявни timezone промени на периода.

## 4. Плъгин интеграции и активации — задължително разширение

Другите ми проекти трябва да могат да включат Fintrox като счетоводна услуга. Те изпращат бизнес факти/събития, а Fintrox определя счетоводните операции. Външен магазин не трябва да решава какви дебит/кредит сметки да използва.

Изпълни следните подетапи, без да преномерираш възстановения roadmap 1–96:

| ID | Отговорност | Резултат |
|---|---|---|
| X01 | Capabilities registry | Стабилни IDs, owner, version, configuration schema; отделна дефиниция и runtime активация |
| X02 | Dependencies и activation | Organization-scoped module/feature activation; dependency validation; цикли/несъвместимости; забрана за невалидно деактивиране |
| X03 | App registry и installations | App registration, отделни инсталации по организация, външен tenant mapping, configuration, version compatibility |
| X04 | Installation lifecycle | Connect/activate/suspend/deactivate/reconnect/revoke; предвидими in-flight операции; без изтриване на история |
| X05 | Access и secrets | Credentials/scopes, rotation/revocation, audit, effective availability; инсталацията не получава повече права от предоставените |
| X06 | Connector/SDK boundary | Versioned .NET и TypeScript SDK, generated contracts, adapter configuration, retries/idempotency, contract tests |
| X07 | Experience profiles | Точно Simple / Accountant / Expert като базови UI presets, а не security roles |
| X08 | Feature/widget registry | Отделни stable feature/widget IDs, metadata, availability и configuration validation; една feature може да има няколко widgets |
| X09 | Layout persistence | Versioned templates, user/organization/profile overrides, concurrency, save/reset/switch и безопасна миграция |
| X10 | Layout editor/renderer | Добавяне/махане на достъпни функционалности, widgets, секции; подредба/размери/навигация; общи компоненти за трите профила |
| X11 | Effective UI | Base preset + organization activations + permissions + personal choices; backend enforce и UI recalculation при промяна |
| X12 | Acceptance flows | Tenant isolation, profile switch/reset, permission loss, activation dependencies, revoke, replay, restored layouts, реален API consumer в контролирана среда |

X01–X05 са след основите за Identity/Organizations/Contracts и преди consumer integration rollout. X06 е с OpenAPI/event API етапите. X07–X11 са преди завършването на ERP workspace; X12 придружава съответните етапи, не се оставя само за финала.

Не дублирай съществуващите clients/scopes/idempotency/inbox/outbox/webhook implementations. Мигрирай и разшири ги. External app и installation са различни понятия. Една app може да има много tenant-bound installations. Credential issuance не е равнозначно на activation.

Ефективният достъп е пресечната точка на активирана организационна функционалност, права на caller-а и приложимите installation scopes/status. Не се разширява от layout или Expert профил.

Deactivation спира новата работа според документирана политика и обработва вече приетата работа предвидимо. Не изтрива posted documents/audit и не допуска двойно осчетоводяване при reactivation. Scopes/revocation се проверяват и при worker обработка, според изрично избраната политика за вече приети събития.

Не зареждай чужди DLL/JS plugins в процеса. „Плъгин“ е управляема API интеграция. Външни конфигурации не съдържат произволен SQL или изпълним код.

## 5. Трите нива и персонализацията

- Simple: малко стъпки, понятни бизнес действия, подходящо за краен клиент.
- Accountant: счетоводна работа, документи, записи, периоди, сверки и отчети.
- Expert: подробни конфигурации, аналитичности, автоматизации и диагностика в рамките на предоставените права.

Accountant е каноничното име на описаното по-рано mid/Professional ниво. Не създавай четвърти preset за същото понятие. Custom означава лична конфигурация върху един от трите presets.

Всяка достъпна функционалност може да бъде добавяна/махана към всеки базов layout. Не ограничавай персонализацията до цели модули. Скриването на елемент не деактивира бизнес модул и не променя правата.

Ключът за персонализация включва organizationId, userId, profileId, layoutId и version. Запази отделни overrides за всеки профил. Смяната възстановява неговите запазени настройки или базовия шаблон; reset засяга само избраната персонална конфигурация. Templates са versioned; optimistic concurrency предотвратява тихо презаписване.

При изгубени permissions/деактивация скрий недостъпните елементи от ефективния UI и блокирай backend операциите. Запазените предпочитания не трябва да се унищожават. Retired/unknown widget IDs се обработват безопасно при version migration. Не съхранявай secrets/tokens в layout или NEXT_PUBLIC.

Frontend composition свързва публичните module exports с общия experience renderer. Experience layer зависи само от себе си и shared; business modules не зависят от experience/composition. Не копирай pages и business rules по три пъти.

## 6. Roadmap 1–96 — обхват за изпълнение

Това е консолидирана работна формулировка на извлечения обновен план, не стенограма на чатовете. Старият ред 1–41 е заменен от него; функционалните изисквания от стария план са запазени в секция 7. Статусът на всяка точка се установява от кода.

### Основи и миграция: 1–26

1. Current-state baseline: пълен рекурсивен inventory и Legacy → Module migration matrix; endpoints/entities/migrations/tests/FE/contracts.
2. Architecture ADRs: boundaries, ownership, conventions и решенията за gradual migration/extraction.
3. Architecture CI gate v2: reference direction, forbidden implementation dependencies, FE import boundaries и contract compatibility.
4. Module bootstrap standard: explicit service/endpoint registration в host, без duplicate registration и скрити зависимости.
5. Per-module DbContext convention: отделен context и ясна собственост, без нов shared business DbContext.
6. Per-module migration history: design-time tooling, deployment ordering, migration assemblies и isolation.
7. PostgreSQL ownership strategy: schema/data ownership, съществуващи FK/joins, безопасно разделяне и сверка.
8. Contracts standard: API/event versioning, DTOs, errors, pagination, compatibility и correlation metadata.
9. Cross-module communication: ports и in-process adapters; асинхронни интеграционни събития при обоснована нужда.
10. Transaction boundary rules: ACID в consistency boundary; compensation/reconciliation при cross-module workflows.
11. Money/rounding/time conventions: precision, currencies, FX rates, rounding, dates, UTC timestamps.
12. Tenant/security conventions: organization context, membership, scopes/permissions, worker context, isolation tests.
13. Correlation/Causation/Actor model: проследимост през HTTP, jobs, events, audit и external clients.
14. Next.js application shell: наличния TS shell се развива контролирано, routing, API client и context boundaries.
15. Identity migration: users, sessions, auth, refresh rotation, roles/permissions и наличните защити към Modules/Identity.
16. Organizations migration: companies, memberships, settings, branches и scope enforcement.
17. Audit migration: immutable trail/access history; запазване на transaction-local гаранциите при прехвърляне.
18. Currencies migration: валути, base currency и исторически обменни курсове.
19. Tax migration: VAT master data, effective dates и налични данъчни конфигурации.
20. Counterparties migration: customers/suppliers, contacts, addresses, identifiers и payment terms.
21. Accounting migration: COA, fiscal calendar, journals, posting/reversals и auto-posting с parity tests.
22. Sales migration: наличните invoices, snapshots, numbering и AR зависимости.
23. Purchases migration: supplier invoices, expenses, snapshots, recoverability и lifecycle.
24. Payments migration: incoming/outgoing payments, allocations, settlement и concurrency guards.
25. Integrations + Reporting migration: clients/scopes, idempotency, inbox/outbox/webhooks и текущите отчети без ownership нарушения.
26. Legacy retirement gate: parity, data reconciliation и премахване на старите runtime зависимости само след доказана замяна.

### Счетоводно ядро: 27–41

27. COA v2: account groups/classes/templates, control/reconciliation accounts, hierarchy и activation rules.
28. Accounting dimensions: Cost Center, Profit Center, Segment, Branch, Project, Department и ownership на definitions/values.
29. Dimension validation/derivation: задължителни аналитичности по account/transaction, validity и deterministic rules.
30. Parallel ledgers/accounting principles: Local GAAP, IFRS и management ledger; explicit posting scope и сверки.
31. Fiscal calendar v2: special periods, close/reopen permissions, overlap protection и year-end rules.
32. Journal Engine v2: manual/system/imported/recurring journals, drafts, validation и lifecycle.
33. Accruals/deferrals: schedules, periodic postings, changes/cancellations, idempotent execution и reversals.
34. Posting Engine v2: account determination, derivation, validation, document splitting и atomic posting.
35. Open Item Management: customer/vendor/control-account items и връзките със source documents.
36. Clearing Engine: partial/full clearing, reset clearing, residual items и traceable corrections.
37. Automatic allocations: recurring distributions, percentages, dimensions, balancing и повторяемост.
38. FX revaluation: unrealized gains/losses, rates, revaluation postings и reversals.
39. Year-end closing: carryforward, retained earnings, opening balances и controlled reopening.
40. Financial statements: Balance Sheet, P&L, Cash Flow, classifications и drill-down до posted transactions.
41. Accounting integrity suite: debit/credit balance, immutability, subledger reconciliation и failure/concurrency сценарии.

### Продажби и покупки: 42–54

42. Sales Quotes: lines, prices, validity, versions, statuses и conversion към order.
43. Sales Orders: fulfillment/reservation references, partial processing и контролирани state transitions.
44. Sales Invoices v2: recurring invoices, snapshots, annual numbering и e-invoice-ready contract model.
45. Credit Notes / Refunds / Debit Notes: source links, partial amounts, reversals и settlement effects.
46. Accounts Receivable: open items, aging buckets, due dates, overdue state и reconciled totals.
47. Dunning: проследим процес за напомняния/събиране на просрочени вземания с конфигурация и audit.
48. Purchase Requisitions + Purchase Orders: request/approval/order lifecycle, suppliers и commitments.
49. Goods/Service Receipts: partial receipts, source links и количествена/стойностна проследимост.
50. Two-way/three-way matching: PO ↔ receipt ↔ invoice, tolerance policy и exception handling.
51. Supplier Invoices + Expenses v2: receiving, tax snapshots, recoverability, numbering и accounting.
52. Supplier Credit Notes: references, adjustment limits и AP/accounting effects.
53. Accounts Payable: aging, due dates, payment proposals и balances.
54. End-to-end O2C/P2P reconciliation: source document → fulfillment/receipt → invoice → ledger → payment/clearing.

### Банки, плащания, данъци и валути: 55–63

55. Bank/cash accounts: account metadata, organization ownership, ledger mapping и currencies.
56. Bank statement imports: CSV/API/standard-format adapters, validation, preview/errors и duplicate prevention.
57. Transaction matching engine: candidates, confidence/rules, manual decisions и reversible links.
58. Bank reconciliation: one-to-one/many-to-one, exceptions, reconciliation state и closing balances.
59. Payment batches/runs/files: proposal/approval/execution/result lifecycle; retries не изпращат повторно плащане.
60. Cash positioning и short-term cash forecasting: actual/expected sources и transparent assumptions.
61. Tax Determination Engine: jurisdiction/effective dates, reverse charge, exemptions, recoverability и snapshot rules.
62. Tax returns/VAT reports/localization: reporting periods, reconciled totals и country-specific adapters.
63. Currency providers, triangulation, historical rate policies и end-to-end FX reconciliation.

### Оперативни ERP модули: 64–72

64. Inventory Item Master: products/services, SKU, UOM и categories.
65. Warehouses/bins, stock movements и transfers; organization/location ownership.
66. Reservations/receipts/issues/availability с concurrency и ясни negative-stock правила.
67. Inventory valuation: FIFO, Weighted Average, Standard Cost с deterministic costing и policy changes.
68. Stock counts/adjustments/lots/serials, audit и traceability.
69. Inventory ↔ COGS accounting: value movements, cost adjustments и stock/ledger reconciliation.
70. Fixed Assets: register, classes, capitalization, source documents и asset accounting.
71. Depreciation books/runs, transfers, impairment/disposal и idempotent postings.
72. Projects: project/cost centers, time, expenses, profitability, WIP/billing и allocations.

### Планиране и групови финанси: 73–78

73. Budgeting: versions, annual/period budgets, dimensions и locking.
74. Forecasts/scenarios и budget-vs-actual variance със съпоставими dimensions/periods.
75. Consolidation: group/company hierarchy, ownership percentages и effective dates.
76. Intercompany matching/reconciliation/eliminations с source traceability.
77. Consolidation currency translation и adjustments; отделни statutory/group books.
78. Consolidated Balance Sheet/P&L/Cash Flow и group close с reconciliation.

### Платформа, документи и отчети: 79–86

79. Payroll core и localization abstraction; pay runs, contributions и accounting postings. Country rules изискват отделна валидирана спецификация.
80. Documents: attachments, templates, retention, imports и OCR-ready pipeline; safe storage/access/content handling.
81. Workflows: configurable approvals, tasks, delegation, escalation, immutable decisions и segregation of duties.
82. Notifications: email/in-app, business events, delivery status и retry/deduplication.
83. External Business Event API: старото 22/41 върху новия Integrations модул; validation, durable receipt, status/retry/replay, explicit scopes.
84. Bulk import/export, connector framework, API/webhook management и SDKs; row-level errors и predictable partial success policy.
85. Reporting semantic/read-model layer: projections/snapshots с freshness metadata; без cross-module SQL ownership violations.
86. Custom reports, dashboards, scheduled exports и transactional drill-down с tenant/permission enforcement.

### UX и production готовност: 87–96

87. Full Next.js ERP workspace: навигация според effective availability, company/period context, responsive UI и X07–X11.
88. Global Search, Command Palette, recent/favorites; резултатите са permission/tenant filtered.
89. Enterprise tables: saved views, filters, grouping, columns, sorting/pagination и безопасни bulk operations.
90. Localization/i18n: language, dates/numbers/currency formats и jurisdiction packages; не смесвай UI locale със счетоводно законодателство.
91. Security hardening: OWASP, rate limits, MFA, secrets, encryption, permission matrix, webhook delivery protections и sensitive-data handling.
92. Performance: load/contention tests, indexes, caches, pagination и големи ledgers; измерими критерии, не произволни claims.
93. Data lifecycle: archival, partitioning, retention и audit retention без нарушаване на финансовите references.
94. Backup/restore/disaster recovery: пълни архиви, encryption/access policy, documented recovery и автоматична проверка чрез restore в изолирана база.
95. CI/CD: build/tests, compatibility checks, migration gates, controlled rollout/rollback, Docker/Render configuration и безопасни deploy процеси.
96. Service-extraction rehearsal: реален модул с отделен API в изолирана среда; доказани data ownership, contracts, auth, failure recovery и непроменени FE imports. Не предприемай production extraction без отделен deployment обхват.

## 7. Изисквания от старите чатове, които остават задължителни

Старият roadmap 1–41 е заменен като ред, но следният обхват не се губи:

| Стар етап | Изискване | Място в новия обхват |
|---|---|---|
| 1–21 | API/DB/auth/tenant foundation, accounting/documents/payments и durable integration foundation | Baseline, 1–41, 51, 55–63, 83; съществуващото се мигрира с parity |
| 22 | External business event API | 83, X03–X06 |
| 23 | OpenAPI и generated SDK | 8, 84, X06; .NET и TypeScript |
| 24 | Unit/integration test hardening | Критерии за всеки етап, 41, 54, 91–96 |
| 25–26 | Next.js scaffold, Auth UI и shell | 14, 15, 87; запази текущия strict TS |
| 27 | Dashboard | 86–87, X07–X11 |
| 28 | Accounts UI | 27, 87, 89 |
| 29 | Journal UI | 31–34, 87, 89 |
| 30 | Counterparties UI | 20, 87, 89 |
| 31 | Sales/Purchases UI | 42–54, 87, 89 |
| 32 | Payments UI | 24, 55–60, 87 |
| 33 | Reports UI | 40, 85–87 |
| 34 | Integration management UI | 83–84, X03–X06 |
| 35 | Audit/admin/settings | 15–17, 87, X01–X11 |
| 36 | PDF/Excel/CSV exports | 80, 84, 86; permission-checked и финансово сверени |
| 37 | Backup/Restore | 94 |
| 38–39 | Docker/Render и GitHub Actions | 95; запази/поправяй текущите pipelines |
| 40 | Първи реален проект като consumer | X06/X12, 83–84; adapter и contract tests преди реално включване |
| 41 | Останалите проекти един по един | Повторяем consumer onboarding след пилота; няма подразбиращо се разрешение за промени в чужди репота |

Пази общите foundations: number sequences, currency rules, attachments, immutable audit и business-event-driven accounting. External references/idempotency включват организацията и външния source/event key; еднакъв key с различен payload да не се приема като същата операция. Определи replay/status/error договора и запази съвместимостта с вече работещия API.

За FE предходната посока е Tailwind/shadcn/ui, React Hook Form/Zod и generated TS API client. Въвеждай ги към реалните UI задачи след проверка на текущите зависимости; не прави произволен визуален redesign като заместител на функционалността.

## 8. Какво означава завършена функционалност

За всеки use case опиши вход/изход, permissions/scopes, organization boundary, entity lifecycle, validation, transaction boundary, idempotency/concurrency, audit/events, error behavior и приложимата FE работа.

За готов етап трябва да има:
- истинска Domain/Application/Infrastructure/Presentation имплементация, не празни handlers или success placeholders;
- persistence/migration при необходимост и безопасна migration стратегия;
- versioned contracts/OpenAPI и актуализиран generated client, когато се променя API;
- happy-path, validation, permission/tenant, concurrent/retry и failure tests, когато са приложими;
- PostgreSQL integration tests за финансови constraints/transactions; in-memory provider не доказва PostgreSQL поведение;
- UI loading/empty/error states и acceptance flow, когато етапът е UI;
- build, typecheck, архитектурни проверки и засегнатите тестове; отчет за реално пуснатите проверки;
- docs/status update и commit на завършената логическа единица.

За финансовите инварианти използвай property/invariant tests и reconciliation, когато са подходящи. Не гони фиктивни 100% coverage чрез тестове на getters/празни класове. Покажи рисковите сценарии и непокритите ограничения.

## 9. Начин на изпълнение и предаване между чатове

1. Създай docs/implementation-status.md с всички 96 етапа и X01–X12, status, dependencies, acceptance criteria, files, tests и commit references. Не пропускай модули заради обема.
2. Първо изпълни baseline 1/96. Представи конкретния ред за следващите зависими единици. Не третирай старо „Next“ или „22“ като разрешение да прескочиш новите foundations.
3. Ако потребителят посочи конкретен етап, изпълни него и необходимите предварителни условия в разрешения обхват. При общо възлагане продължавай последователно; не спирай само с план.
4. Запази започнатото. Мигрирай capability по capability, със сравнение на поведението и финансовите данни; премахвай legacy code след доказана замяна.
5. Работи директно в правилните GitHub репота. Commit в main след завършен етап; провери актуалния head и включи concurrent промени без force push. Не презаписвай чужда работа.
6. Не използвай общи success твърдения. Всеки статус се опира на diff, реални тестове и commit. Изрично отбелязвай blocked/not tested; не маркирай целия модул готов заради един endpoint.
7. При продължаване в нов чат прочети status документа и актуалния код и продължи от първата незавършена зависима задача. Не започвай отначало и не измисляй нова номерация.
8. Финалният отговор след етап е кратък: завършен етап, проверки, commit-и, следващ етап и съществен blocker, ако има.

## 10. Произход и приоритет на изискванията

Консолидиран обхват от проектните разговори „Преглед на проект и предложения“ (оригинален план 1–41), „Проверка на предния чат“ (напредък 17–21 и обновен план 1–96), „ERP архитектура SAP мащаб“ (platform/capabilities/experience/integration модел), „FE с TypeScript“ и текущия разговор (довършване на структурата и следваща бизнес имплементация).

Историческите формулировки са извлечени като контекст, не пълни транскрипти. Този документ е работна консолидация: подробните acceptance правила конкретизират изискванията, не са представени като дословни цитати. Не внасяй изисквания от Paladinhub, Orisia, DGVision или други проекти само защото са се появили в резултатите от търсене.

Последните изрични потребителски указания имат приоритет пред старите предложения. Актуалният код е източникът за текущо изпълнение; архитектурната цел определя бъдещата посока. Ако конкретна интеграция, държава или нормативен режим още няма потвърдена спецификация, изолирай съответния adapter/policy и отбележи зависимостта, без да измисляш production правила.
