# AGENTS.md

## Rules

These rules should be followed unless the user asks otherwise.

- Do not stage/unstage/commit anything.
- Speak English unless another language is required.
- No EF Core migrations; the database will be recreated.
- Fix insignificant flaws (e.g., typos, broken formatting, obvious grammar errors) in documents (including code) directly connected with the current task.
- Do not fix significant flaws in documents (including code) yourself; report them and wait for the user's decision.
- Raise objections and suggestions before proceeding when you believe the task or a decision is explicitly wrong. Do not silently comply with a task you believe is incorrect; state the concern and wait for the user's confirmation.
- If the current task is analysis task (when user asks to analyze bug, debt, user story):
    - Report considerations in chat. Write new task items as files in ./docs/tasks directory.
- If the user asks you to work through multiple tasks (a list, backlog, or sequence):
    - Complete one task, then stop and wait for explicit approval before starting the next.
