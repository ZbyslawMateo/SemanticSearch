# Code Review — Semantic Search (search branch)

Quick review of the embedding/search code before it gets merged into the app's
search function. Three parts: **bugs I fixed**, **things that need your call**, and
**what's still missing** for the agreed requirements.

---

## Bugs I fixed

1. **Incomplete migration.** `AddBrandsAndColorTable` created the `Brands` and
   `Colors` tables but never added the `BrandId` / `ColorId` columns and foreign
   keys to `Items` — even though its `Down()` tried to drop them. Seeding crashed
   with *column "BrandId" does not exist*. Added the missing
   `AddColumn` / `CreateIndex` / `AddForeignKey` calls.

2. **Seeder isn't atomic.** The only guard was `if (Posts.Any()) return;`, with no
   transaction. When bug #1 crashed mid-save, the DB was left half-seeded and every
   restart then *skipped* seeding (because posts already existed). Wrapped the whole
   seed in a transaction so it's all-or-nothing.

3. **Brand/color set on only 3 items.** They were hard-assigned to `items[0..2]` by
   position, so every other item had no brand/color and the assignment broke if the
   JSON order changed. Now distributed across all items.

4. **Duplicate text in the embedding.** The post title and tags were each written
   **twice** into the text sent to the model, over-weighting them. Removed the
   duplicates.

5. **Double `Document:` prefix.** The builder text already started with `Document:`
   and the caller added another one (`Document: Document: Post...`). Now prefixed
   once.

6. **Dead `Include` chains in search.** EF Core ignores `Include` when the query ends
   in a `.Select(...)` projection, so those 5 lines did nothing. Removed them (the
   projection already loads tags/items). The same pattern in the background worker
   **is** correct and was left alone.

7. **`dynamic` in the rankings list.** `DistinctBy` / `OrderBy` ran on `dynamic` —
   no compile-time checks, and it deduped by label only (a tag and an item with the
   same text would collide and one would vanish). Replaced with a typed `RankingRow`
   record, deduped by *(type + label)*.

8. **Middleware order.** `UseHttpsRedirection()` was registered *after* the
   endpoints. Moved it above them.

9. **No query length limit.** Search forwarded any-length input straight to the
   model. Added a 500-character cap.

---

## Needs your decision (left unchanged)

- **The "similarity matrix" doesn't contain similarities.** Every cell is just the
  post's score (or `0` if the item isn't in that post). Items have no embeddings of
  their own, so real per-item similarity isn't possible without adding item-level
  embeddings. Either rename it (it's really a post↔item *membership* map) or decide
  we want item vectors too.

- **Tag constraints vs seed data.** `Tags.Value` is unique + max 30 chars. The
  current data is fine, but a duplicate or long tag in `Tags.json` will fail seeding.
  Left as-is to avoid silently dropping tag links.

---

## Still missing for the requirements

Bigger work for when this is wired into the app's search branch:

- Embedding should live in a **separate table**, not a column on `Posts`.
- Embedding should be created **when the post is created**. Today there's no create
  endpoint — a background worker fills embeddings in every 10s instead.
- **Model isn't free for commercial use** (Jina). Swap to e.g.
  `bge-base-en-v1.5`, `e5-base-v2`, or `nomic-embed-text-v1.5` (all 768-dim, so no
  DB column change). If you switch, also update the `Document:` / `Query:` prefixes
  to that model's convention.
- **Result count and similarity threshold are hardcoded** (`10` and `0.10`). Move
  both into `appsettings.json`.

---

## Setup note

The Postgres port in `docker-compose.yml` + `appsettings.json` was changed
`5433 → 5434` to avoid a local Postgres install on my machine. Change it back if you
don't have that conflict.
