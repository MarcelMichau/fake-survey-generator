# Fake Survey Generator

Fake Survey Generator creates simulated Survey outcomes for entertainment. Its draft language distinguishes editable Survey information from a generated Survey.

## Language

**Survey draft**:
The editable question, audience, respondent count, and options intended for Survey generation, distinct from a generated Survey. Retaining submitted information does not make later edits part of that generated Survey; they form a fresh Survey draft.
_Avoid_: Pending Survey, generated Survey (when referring to editable information)

**User registration**:
The association of an authenticated external identity with a persisted User in Fake Survey Generator. Repeating registration for the same identity represents the same User, not another registration.
_Avoid_: Sign-in, authentication (when referring to persisted User registration)

**Registration readiness**:
The current authenticated session's successful User registration, which is the prerequisite for all Survey operations. Registration from a previous session does not establish readiness for the current session.
_Avoid_: Logged in (when referring to readiness for Survey operations)
