# Modern Web Guidance scan

Date: 2026-11-11. Application revision: `00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509`.

Reviewed using Google's [Modern Web Guidance skill][skill], CLI version `1.0.0`, and upstream revision `4dd968349e54ca45a05dd5c818fb035c9054b022`. The initial scan did not change application code or dependencies. The skill was read from a temporary upstream checkout and used through its CLI, without a persistent skill installation.

## Implementation update

Following approval of this report, opportunities **1–7 are implemented**. Opportunity **8 remains deferred until performance impact is measured**. The original findings below link to the preimplementation revision above.

- **1 — Native confirmation:** `ConfirmDialog` opens through `showModal()`, uses native modality/focus containment, provides a feature-detected light-dismiss fallback, and guards dismissal while deletion is pending. Both callers provide the actual invoker, including browsers that do not focus buttons on pointer activation. The browser restores a surviving invoker; a deleted invoker uses the existing workflow fallback. Delete errors and retry remain inside the modal.
- **2 — Contrast:** shared input borders now use a dedicated token. Verified ratios are 4.74:1 against the panel and 3.85:1 against the input fill; placeholder text is 5.63:1 against the fill. A production-CSS Chromium check confirms a visible 3px focus outline with forced colors active.
- **3 — Validation:** controls have native required/type/range/step/length constraints and names. Empty numeric editing states are preserved, and preferred votes are no longer truncated to integers while editing. Analysis validates its text separately from creation and enforces its 20-option limit; creation retains the 100-option and 1,000,000-respondent limits. Server validation remains authoritative. API errors retain field/index keys, associate messages with controls, and provide a focused, linked error summary. Starting a new action clears obsolete feedback from the previous action.
- **4 — Structure:** anonymous and authenticated views each have one main landmark and one primary heading. The skip link targets a focusable heading. Forms have names derived from their visible headings, and related controls have legends.
- **5 — Announcements:** routine results use polite, visible status messages; analysis warnings have one summary announcement. Individual warning cards do not each interrupt speech. Field errors synchronize `aria-invalid` and conditional descriptions, with one shared delayed polite announcer that ignores removed/disabled/corrected fields and updates outside an open modal. Error space is prepared before blur to avoid moving a button between pointer-down and pointer-up.
- **6 — Tables:** the results table has a caption, explicit column/row headers, and a named keyboard-scrollable region. Headers remain visible while scrolling; the question column is sticky only where the container is wide enough to avoid covering narrow-screen data.
- **7 — Option focus:** adding an option focuses its input. Removing an option focuses the next surviving option, or the previous one when removing the last row, using stable option IDs. Remove buttons share the vote input control row so they remain vertically aligned at wide and narrow widths, including when validation errors appear.

Component tests now load the application CSS. Browser selection is configurable through `UI_TEST_BROWSER`, defaulting to Chromium. Cross-browser verification also corrected a test-only readiness assertion: its Query error property is now observed during render instead of first being read by a later poll, which could miss an already-settled error notification.

Run the UI suite from the repository root:

```powershell
npm.cmd test --prefix src/client/ui -- --run
$env:UI_TEST_BROWSER = 'firefox' # or 'webkit'
npm.cmd test --prefix src/client/ui -- --run
Remove-Item Env:UI_TEST_BROWSER
npm.cmd run build --prefix src/client/ui
```

From `src/client/ui`, install additional local test engines when needed with `npx.cmd --no-install playwright install firefox webkit`. No application dependency or lockfile changes are required.

Implementation verification includes the production UI build, full browser suites, native modal/fallback behavior, validation and field associations, deletion failure/retry, focus recovery, 320px layouts and keyboard table scrolling, contrast calculations, and a forced-colors focus check. This is automated browser verification; a live authenticated deployment and real screen-reader speech were not exercised. The original scan-only limits below remain a historical record of that earlier pass.

Final results: **production UI build passed; all 166 tests in 9 files passed in each of Chromium, Firefox, and WebKit**. The existing large-bundle build warning remains within deferred opportunity 8. Broader Biome checks also report existing formatting differences in untouched components and hook-factory diagnostics in readiness test fixtures.

## Coverage and results

Ran the repository-wide modernization scan prescribed by the skill:

```powershell
npx.cmd -y modern-web-guidance@1.0.0 legacy-scan . --json
```

Result: **0 findings**, across **35 active rules / 33 guides**; 11 documentation/test files were skipped. The scanner states that its deterministic rules cover approximately 20% of the guidance corpus. Zero matches therefore does not establish compliance with all guides.

Manually inspected the React UI, shared CSS, form/draft behavior, error handling, existing relevant tests, and server validation constraints. Searched and retrieved the `dialog`, `forms`, `validate-input-after-interaction`, `accessible-error-announcement`, `responsive-table`, `accessibility`, `html`, `color`, `performance`, and `responsive-design` guides.

Eight opportunities survived review. High priority means a direct accessibility gap worth addressing first; medium priority means a useful improvement, not an observed production incident. Performance benefits need measurement.

| Order | Priority | Opportunity | Main guidance | Scope |
| --- | --- | --- | --- | --- |
| 1 | High | Use native modal behavior for delete confirmation | `dialog`, `accessibility` | Shared dialog and both callers |
| 2 | High | Increase input boundary and placeholder contrast | `color` | Shared CSS |
| 3 | Medium | Use native form constraints and associated errors | `forms`, validation guides | Field, creation, lookup, error representation |
| 4 | Medium | Complete signed-in landmarks and form names | `html`, `accessibility`, `forms` | App shell and forms |
| 5 | Medium | Match announcement urgency to message purpose | `accessibility` | Shared alerts and result updates |
| 6 | Medium | Improve table navigation on narrow screens | `responsive-table`, `accessibility` | My Surveys |
| 7 | Medium | Preserve keyboard focus when options change | `html`, `accessibility` | Dynamic option editor |
| 8 | Medium / measure first | Defer signed-in UI code from the initial bundle | `performance` | App composition and loading states |

## 1. Native confirmation dialog

Evidence: [ConfirmDialog.tsx:33](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/ConfirmDialog.tsx#L33) implements initial/restored focus, busy-state focus, a global Escape handler, and a button-only Tab trap. [ConfirmDialog.tsx:82](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/ConfirmDialog.tsx#L82) renders a fixed-position `div` with `role="dialog"` and `aria-modal="true"`. There is no mechanism making the background DOM inert.

The ARIA attribute describes modality but does not enforce it. The custom keyboard trap does not prevent programmatic focus outside the modal, and it only knows about buttons. The native element can remove much of this maintenance burden while providing top-layer rendering and actual background inertness. See the [dialog guide][dialog] and [accessibility guide][accessibility].

Recommended change:

- Use `<dialog>` with a ref and `showModal()` / `close()`, synchronized with React state and native close/cancel events. The existing state-controlled, asynchronous delete flow makes the script API a suitable starting point.
- Keep safe initial focus on Cancel, the busy-state guard, the retry flow, and a surviving-control focus fallback after a successful deletion removes the trigger. The existing [dialog tests](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/ConfirmDialog.test.tsx#L1) already capture useful behavior to preserve.
- Use `useId()` for title/description references and style `::backdrop`. Do not carry the custom Tab trap or redundant modal ARIA onto a native modal.
- Put delete failures inside the open dialog. They currently render outside it in [MySurveys.tsx:122](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/MySurveys.tsx#L122) and [SurveyResult.tsx:140](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/SurveyResult.tsx#L140). Once outside content is inert, those errors would be inaccessible while the modal remains open.

Compatibility: the native dialog is the core improvement. The guide marks `closedby` as limited availability, so it cannot be the sole protection against dismissal while busy. Preserve existing backdrop dismissal with the guide's feature-detected fallback if required; otherwise document the graceful degradation. Newer declarative `commandfor` controls are optional and have their own fallback requirements. Follow the guide's scroll-containment notes rather than adding root scroll locks automatically.

Validation when implementing: keyboard containment, initial Cancel focus, dismissal, busy-state protection, failure/retry messaging inside the modal, backdrop interaction, and focus after the triggering row/result disappears. Check Chromium, Firefox, and WebKit behavior.

## 2. Input contrast

Evidence: [index.css:94](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/index.css#L94) uses the `line` token for input borders, `surface` for input backgrounds, and `#7d858a` for placeholder text. Against the declared opaque colors, WCAG relative-luminance calculations give:

- Placeholder `#7d858a` on input `#30353a`: approximately **3.30:1**, below the color guide's 4.5:1 normal-text target.
- Border `#4a5157` against panel `#222629`: approximately **1.89:1**; against input fill `#30353a`: **1.54:1**. The fill itself is also too similar to the panel to provide a 3:1 boundary instead.

Use a dedicated input-border token and brighter placeholder text that meet the [color guide][color], while retaining the current visual style. Do not brighten every table separator indiscriminately; decorative separators have different purposes from control boundaries. Disabled controls are a separate case.

Validation when implementing: check normal and hover states against their adjacent surfaces, placeholder contrast, keyboard focus, and forced-colors behavior.

**Dropped false positive:** the input's component rule says `outline: none !important`, but the earlier base layer also supplies an important outline. Important cascade-layer precedence makes the base outline win. An isolated headless Chromium check using the production CSS confirmed a 3px outline both normally and with forced colors active; only the shadow disappeared. There is no verified missing-focus-ring finding here.

## 3. Native constraints and accessible field errors

Evidence: [Field.tsx:29](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/Field.tsx#L29) always renders `type="text"` and exposes no native validation or input-hint props. Both respondent count ([CreateSurvey.tsx:62](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/CreateSurvey.tsx#L62)) and survey ID ([GetSurvey.tsx:54](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/GetSurvey.tsx#L54)) use it and coerce input to numbers on every edit. Required prose fields have no `required` or `maxLength` constraints. The preferred-votes input already has a numeric type and min/max; retain that useful baseline.

Server constraints are explicit in [CreateSurveyCommand.cs:34](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/server/FakeSurveyGenerator.Application/Features/Surveys/CreateSurveyCommand.cs#L34): respondent count 1–1,000,000, required text up to 250 characters, and 1–100 options. [AnalyzeSurveyCommand.cs:49](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/server/FakeSurveyGenerator.Application/Features/Surveys/AnalyzeSurveyCommand.cs#L49) separately limits analysis to 20 options.

Recommended change:

- Let `Field` accept suitable native input attributes. Use `type="number"`, `min`, `max`, and `step="1"` for respondent quantity. Choose numeric input hints/constraints for the ID without unnecessarily treating an identifier as a quantity.
- Preserve an empty editing state instead of converting a cleared value immediately to zero. Parse/validate at the appropriate boundary.
- Add required/length constraints and persistent instructions. Present validation after interaction or attempted submission with `:user-invalid`, plus associated error text and synchronized ARIA state. Keep server validation authoritative.
- Preserve property keys from server validation responses: [ApiError.ts:15](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/api/ApiError.ts#L15) currently flattens errors into strings, and [CreateSurvey.tsx:212](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/CreateSurvey.tsx#L212) renders separate generic alerts. Retaining field identity enables `aria-describedby`, error-summary links, and useful focus routing. Keep domain-wide errors in the summary.
- Give analysis its own validation path: its button calls `draft.analyze()` directly rather than submitting the form. Adding HTML constraints to the creation submit alone will not validate analysis. Do not make the respondent-count requirement block analysis, whose command has no respondent count.

See [forms][forms], [validation after interaction][validation], and [accessible error announcement][error-announcement]. The guides classify `:user-invalid` as widely available; a default Baseline widely-available policy does not require an extra legacy fallback.

Validation when implementing: blank/negative/fractional/oversized respondent counts, empty/long text, invalid IDs, focus on failed submission, corrected error clearing, and creation versus analysis limits. Recheck creation and analysis independently.

## 4. Landmarks, headings, and form names

Evidence: the anonymous [Splash.tsx:2](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/Splash.tsx#L2) supplies a `main` and `h1`. The signed-in branch in [App.tsx:27](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/App.tsx#L27) is a generic grid containing section headings starting at `h2`; [NavBar.tsx:21](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/NavBar.tsx#L21) uses a styled `div` for the product name. The three main forms lack accessible names, and the creation fieldset has no legend.

Provide one `main` and an appropriate `h1` for each view, plus a skip link past the repeated navigation. Avoid nesting a new shell `main` around the existing splash `main`. Name the creation, lookup, and survey-list forms from their visible headings; use `<search>` for lookup if appropriate. Give related groups meaningful legends while preserving the fieldset's existing disabled-state behavior. These are small structural changes supported by [HTML][html], [accessibility][accessibility], and [forms][forms].

Validation when implementing: inspect the accessibility tree in both authentication states, heading order, named forms, skip-link focus, and group labels.

## 5. Announcement urgency

Evidence: [Alert.tsx:18](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/Alert.tsx#L18) applies `role="alert"` for every message, including successful creation, empty survey lists, and each analysis warning. That is assertive behavior even for routine updates. Successful lookup/list rendering has no explicit result-status announcement.

Apply the [accessibility guide's live-region urgency rules][accessibility]: use polite status for routine success/results, reserve assertive announcements for urgent blocking failures, and summarize batches of analysis feedback instead of emitting multiple competing alerts. Coordinate announcements with modal inertness. Do not automatically move focus to every updated result or announce every brief loading state.

Validation when implementing: test actual announcements with a screen reader, especially multi-warning analysis, successful lookup, empty results, and delete failure while a modal is open. Source inspection alone cannot establish the spoken experience.

## 6. Responsive table navigation

Evidence: [MySurveys.tsx:69](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/MySurveys.tsx#L69) wraps an at-least-850px-wide table in `overflow-x-auto`. The wrapper has no accessible name or explicit keyboard focus; the table has no caption, explicit header scopes, or row headers.

The existing semantic table and horizontal overflow are a sound starting point. Follow [responsive tables][table] to make the overflow region explicitly keyboard-reachable and named, associate a caption with the table, and add header scopes/appropriate row headers. Some browsers automatically focus some scroll containers, but that is not a consistent cross-browser guarantee. Consider sticky headers or a sticky question column to retain context during scrolling.

A container-query stacked presentation is an optional enhancement, not a prerequisite. If introduced, validate its header relationships and generated labels with assistive technology rather than copying the sample CSS without checking the resulting semantics.

Validation when implementing: 320px viewport, zoom, keyboard scrolling without relying on a mouse, long question/option text, row identification, and header announcements. See also [accessibility][accessibility].

## 7. Focus after adding/removing options

Evidence: [CreateSurvey.tsx:127](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/components/CreateSurvey.tsx#L127) removes a row through its focused button; [useSurveyDraft.ts:184](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/hooks/useSurveyDraft.ts#L184) updates the data without a UI focus transition. Adding an option similarly appends it without helping keyboard users reach the new field.

Use stable option IDs and input refs to put focus somewhere logical after a focused row is removed: an adjacent option, then Add Option if necessary. Consider focusing the new option's input after the explicit Add Option action. Keep DOM focus handling in the UI; the draft hook can continue owning data transitions. This follows [HTML focus maintenance][html] and [accessibility focus guidance][accessibility].

Validation when implementing: add using the keyboard, remove a middle/last row, and verify the next keystroke operates on a predictable control rather than restarting navigation from the document.

## 8. Initial bundle: investigate a signed-in loading boundary

Evidence: `npm.cmd run build --prefix src/client/ui` passed and emitted one JavaScript asset of **628.75 kB minified / 183.05 kB gzip**, plus **22.59 kB CSS / 5.56 kB gzip**. Vite reported its greater-than-500-kB chunk warning. [App.tsx:6](https://github.com/MarcelMichau/fake-survey-generator/blob/00177fdaa8ba1e0d7d6882a0ef73d8a8c757c509/src/client/ui/src/App.tsx#L6) eagerly imports creation, lookup, and survey-list components even when only the anonymous splash is rendered.

The [performance guide][performance] recommends dynamic imports for code not needed immediately. A useful experiment is a lazy authenticated workspace with a stable loading fallback. Keep the authentication/registration and session-cache boundaries correct. Vendor splitting alone is not enough: it can produce more files while still requiring all of them for the first view.

This is an optimization candidate, not a measured LCP/INP regression. Build size is evidence of payload, not user-visible latency. Compare cold anonymous and authenticated loads on a throttled device/network before choosing a boundary; account for post-login loading and failure states. The build output's gzip size is a compression estimate, not proof of production transfer size or compression configuration.

## Lower-priority follow-up and things to retain

- [Responsive design][responsive] suggests dynamic viewport sizing. The app shell uses `min-h-screen`, the body uses `100vh`, and the HTML body/root use fixed viewport/full heights. A small future pass could evaluate `dvh` or `svh` according to the desired mobile-toolbar behavior, checking short and long pages. No mobile viewport failure was reproduced in this review.
- Existing Grid/Flexbox layouts, system fonts, explicit labels with `useId`, native buttons, a named navigation landmark, textual result values, `Intl` formatting, and reduced-motion CSS already align with several guides.
- Preserve session-scoped TanStack Query cancellation/caching. Do not add service-worker caching for authenticated API data simply because the performance guide discusses it.
- Tailwind's source `@import "tailwindcss"` is compiled at build time; it is not evidence of a runtime CSS request waterfall. The production module script is already deferred by module semantics.
- No active image/video/font-loading bottleneck or heavy client computation was established. Adding image priority hints, workers, `scheduler.yield()`, or `content-visibility` without an applicable workload would add complexity without demonstrated benefit.

## Suggested implementation sequence

1. Fix input contrast and signed-in semantics; these are small, direct changes.
2. Modernize the shared modal and move failure messaging into it, preserving current deletion behavior.
3. Improve native form validation and field-error identity together; handle creation and analysis separately.
4. Refine announcements, table navigation, and option-editor focus.
5. Measure and experiment with the authenticated bundle boundary.

The initial scan included no implementation. Relevant behavior tests should accompany functional changes; visual CSS adjustments can use focused browser/contrast verification. At the initial scan, UI browser tests covered Chromium only; the implementation update above records the additional browser coverage.

## Verification and limits

- Completed: upstream skill/guide review, automated legacy scan, manual source review, production UI build, contrast calculations, and an isolated headless Chromium focus-style check with normal/forced colors.
- Not performed: a full running-app audit, authenticated end-to-end exercise, Lighthouse/axe audit, Core Web Vitals capture, real screen-reader testing, or Firefox/WebKit testing. Existing behavior tests were read but the test suite was not rerun for this documentation-only change.
- Hindsight search/list calls timed out; no memory claims were used.
- No repository browser-support policy was found in the reviewed UI configuration. Recommendations use the upstream skill's default: Baseline widely-available features, with guide-specific fallback/degradation for newer features. Build targets and TypeScript library settings alone are not a browser-support policy.

[skill]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/SKILL.md
[dialog]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/guides/ui-components/dialog.md
[forms]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/guides/forms/forms.md
[validation]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/guides/forms/validate-input-after-interaction.md
[error-announcement]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/guides/accessibility/accessible-error-announcement.md
[table]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/guides/ui-atoms/responsive-table.md
[accessibility]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/guides/accessibility/accessibility.md
[html]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/guides/html/html.md
[color]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/guides/visual-design/color.md
[performance]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/guides/performance/performance.md
[responsive]: https://github.com/GoogleChrome/modern-web-guidance/blob/4dd968349e54ca45a05dd5c818fb035c9054b022/skills/modern-web-guidance/guides/css/responsive-design.md
